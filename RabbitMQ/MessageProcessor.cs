using AutoMapper;
using EDM.Infohub.BPO.Models.SQS;
using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration;
using EDM.Infohub.Integration.DataAccess;
using EDM.Infohub.Integration.Exceptions;
using EDM.Infohub.Integration.Models;
using EDM.Infohub.Integration.RabbitMQ;
using EDM.Infohub.Integration.RabbitMQ.impl;
using EDM.Infohub.Integration.Services;
using EDM.Infohub.Integration.Services.Impl;
using EDM.Infohub.Integration.SQS;
using EDM.Infohub.Integration.SQS.Interfaces;
using EDMFixedIncomeOnService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Price;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("EDM.Infohub.BPO.Test")]
namespace EDM.Infohub.BPO
{
    public class MessageProcessor
    {
        private IConfiguration _config;
        private ILogger<MessageProcessor> _logger;
        private IEDMCommonService _commonService;
        private readonly IFixedIncomeOn _fixedIncomeService;
        private readonly IMapper _mapper;
        private readonly IEDMLuzService _edmLuzService;
        private ISender _rabbitSender;
        private readonly DecodeLuzMapper _decodeLuzMapper;
        private readonly AuxiliaryMessageProcessor _auxiliaryMessageProcessor;
        private IMemoryCache _cacheNack;
        private PuDeEventosRepository _puDeEventos;
        private RastreamentoProcessor _rastreamento;
        private readonly INifiService _nifiServices;

        public MessageProcessor(IEDMCommonService commonService, IFixedIncomeOn fixedIncomeService, ILogger<MessageProcessor> logger, IMapper mapper, IEDMLuzService luzService, ISender rabbitSender, DecodeLuzMapper decodeLuzMapper, AuxiliaryMessageProcessor auxiliaryMessageProcessor, IMemoryCache cacheNack, IConfiguration config, PuDeEventosRepository puDeEventos, RastreamentoProcessor rastreamento, INifiService nifiServices)
        {
            _logger = logger;
            _commonService = commonService;
            _fixedIncomeService = fixedIncomeService;
            _mapper = mapper;
            _edmLuzService = luzService;
            _rabbitSender = rabbitSender;
            _decodeLuzMapper = decodeLuzMapper;
            _auxiliaryMessageProcessor = auxiliaryMessageProcessor;
            _cacheNack = cacheNack;
            _config = config;
            _puDeEventos = puDeEventos;
            _rastreamento = rastreamento;
            _nifiServices = nifiServices;
        }
        public void Process(List<DadosPreco> message, IDictionary<string, object> headers)
        {
            var COD_PRACA = Encoding.UTF8.GetString((byte[])headers["COD_PRACA"]);
            var COD_FEEDER = (int)headers["COD_FEEDER"];
            bool HOMOLOGACAO = Convert.ToBoolean(Encoding.UTF8.GetString((byte[])headers["HOMOLOGACAO"]));
            var idRequisicaoRastreamento = new Guid((byte[])headers["EVENTO_ID"]);
            var dataRequisicaoRastreamento = (DateTime)JsonConvert.DeserializeObject(Encoding.UTF8.GetString((byte[])headers["EVENTO_DATAINICIO"]));
            string usuarioRastreamento = null;
            if (headers["EVENTO_USER"] != null)
                usuarioRastreamento = Encoding.UTF8.GetString((byte[])headers["EVENTO_USER"]);
            var priceObj = _mapper.Map<List<PricePricing>>(message);
            var tipoLog = TipoLogEnum.PRECO;
            var tipoRequisicao = TipoRequisicaoEnum.PRECO;
            var metodo = "v2/Preco/relatorio-dia/";

            _rastreamento.Papeis(idRequisicaoRastreamento, Utils.ListaSnaPrecos(priceObj), dataRequisicaoRastreamento, null, tipoLog, StatusMensagemEnum.PROCESSADO_MDP, StatusProcessamentoEnum.PROCESSANDO, "", usuarioRastreamento);

            List<PriceHistoricalData> priceListHD = new List<PriceHistoricalData>();
            var count = 0;
            var continua = false;
            foreach (PricePricing dadosElement in priceObj)
            {
                if (HOMOLOGACAO)
                    dadosElement.CodigoSNA = Utils.CodSnaHomologacao(dadosElement.CodigoSNA);
                try
                {
                    _logger.LogInformation($"Montando Objeto para HD {count}");
                    dadosElement.CodAtivo = _commonService.CodCetipToCodAtivo(dadosElement.CodigoSNA);
                    priceListHD.AddRange(dadosElement.Map(dadosElement.Data, COD_PRACA, COD_FEEDER));
                    count++;
                    _rastreamento.Papel(idRequisicaoRastreamento, dadosElement.CodigoSNA, dataRequisicaoRastreamento, DateTime.Now, tipoLog, StatusMensagemEnum.FINALIZADO, StatusProcessamentoEnum.SUCESSO, "", usuarioRastreamento);
                }
                catch (Exception e)
                {
                    var errorMessage = $"Erro ao montar objeto do papel {dadosElement.CodAtivo}: " + e.Message;
                    _logger.LogError(errorMessage);

                    var errorPapel = new RastreamentoPapel() { IdRequisicao = idRequisicaoRastreamento, Papel = dadosElement.CodigoSNA, DataInicioEvento = dataRequisicaoRastreamento, DataFimEvento = DateTime.Now, TipoLog = tipoLog.ToString(), StatusMensagem = StatusMensagemEnum.ERRO_MDP.ToString(), StatusPapel = StatusProcessamentoEnum.ERRO.ToString(), MensagemErro = errorMessage, Usuario = usuarioRastreamento };
                    _rabbitSender.SendObj("DeadLetterPrecoQueue", errorPapel, true);
                    _rastreamento.Evento(idRequisicaoRastreamento, tipoRequisicao, dataRequisicaoRastreamento, DateTime.Now, metodo, StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, "Erro ao processar preços."), usuarioRastreamento);

                    continua = true;
                }
                if (continua) continue;
            }
            //_commonService.SalvarPrecosLote(priceListHD.ToArray());
            _nifiServices.SalvarPrecosLote(priceListHD.ToArray());
            if (!continua)
                _rastreamento.Evento(idRequisicaoRastreamento, tipoRequisicao, dataRequisicaoRastreamento, DateTime.Now, metodo, StatusProcessamentoEnum.SUCESSO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
        }

        public void ProcessPUHistorico(List<DadosPreco> message, IDictionary<string, object> headers)
        {
            var COD_PRACA = Encoding.UTF8.GetString((byte[])headers["COD_PRACA"]);
            var COD_FEEDER = (int)headers["COD_FEEDER"];
            bool HOMOLOGACAO = Convert.ToBoolean(Encoding.UTF8.GetString((byte[])headers["HOMOLOGACAO"]));
            var idRequisicaoRastreamento = new Guid((byte[])headers["EVENTO_ID"]);
            var dataRequisicaoRastreamento = (DateTime)JsonConvert.DeserializeObject(Encoding.UTF8.GetString((byte[])headers["EVENTO_DATAINICIO"]));
            string usuarioRastreamento = null;
            if (headers["EVENTO_USER"] != null)
                usuarioRastreamento = Encoding.UTF8.GetString((byte[])headers["EVENTO_USER"]);
            var priceObj = _mapper.Map<List<PricePricing>>(message);
            var tipoRequisicao = TipoRequisicaoEnum.PRECO_HISTORICO;
            var metodo = $"v1/Historico/{priceObj[0].CodigoSNA}/";

            List<PriceHistoricalData> priceListHD = new List<PriceHistoricalData>();
            try
            {
                var CodSNALuz = priceObj[0].CodigoSNA;
                if (HOMOLOGACAO)
                    CodSNALuz = Utils.CodSnaHomologacao(CodSNALuz);
                var CodAtivo = _commonService.CodCetipToCodAtivo(CodSNALuz);
                _logger.LogInformation($"Montando Objeto Historico para HD");

                foreach (PricePricing dadosElement in priceObj)
                {
                    dadosElement.CodAtivo = CodAtivo;
                    priceListHD.AddRange(dadosElement.Map(dadosElement.Data, COD_PRACA, COD_FEEDER));
                }
                //_commonService.SalvarPrecosLote(priceListHD.ToArray());
                _nifiServices.SalvarPrecosLote(priceListHD.ToArray());
                _rastreamento.Evento(idRequisicaoRastreamento, tipoRequisicao, dataRequisicaoRastreamento, DateTime.Now, metodo, StatusProcessamentoEnum.SUCESSO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
            }
            catch (NullReferenceException e)
            {
                var errorMessage = $"Erro ao montar objetos do papel {priceObj[0].CodigoSNA}: " + e.Message;
                _logger.LogError(errorMessage);
                _rastreamento.Evento(idRequisicaoRastreamento, tipoRequisicao, dataRequisicaoRastreamento, DateTime.Now, metodo, StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, errorMessage), usuarioRastreamento);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void ProcessCadastro(List<DadosCaracteristicos> message, IDictionary<string, object> headers)
        {
            var DATA_ATUALIZACAO = DateTime.Parse(Encoding.UTF8.GetString((byte[])headers["DATA_ATUALIZACAO"]));
            var HOMOLOGACAO = Convert.ToBoolean(Encoding.UTF8.GetString((byte[])headers["HOMOLOGACAO"]));
            var idRequisicaoRastreamento = new Guid((byte[])headers["EVENTO_ID"]);
            var idRequisicaoRastreamentoFluxos = Guid.NewGuid();
            var dataRequisicaoRastreamento = (DateTime)JsonConvert.DeserializeObject(Encoding.UTF8.GetString((byte[])headers["EVENTO_DATAINICIO"]));
            string usuarioRastreamento = null;
            if (headers["EVENTO_USER"] != null)
                usuarioRastreamento = Encoding.UTF8.GetString((byte[])headers["EVENTO_USER"]);
            var impactoList = new List<FIOnInstrumentContract>();
            var envioTracking = false;
            var erroEvento = false;
            var emissorPendencia = new List<string>();
            var fiduciarioPendencia = new List<string>();
            var garantidorPendencia = new List<string>();

            _rastreamento.Papeis(idRequisicaoRastreamento, Utils.ListaSnaCadastros(message, HOMOLOGACAO), dataRequisicaoRastreamento, null, TipoLogEnum.CADASTRO, StatusMensagemEnum.PROCESSADO_MDP, StatusProcessamentoEnum.PROCESSANDO, "", usuarioRastreamento);
            _rastreamento.Evento(idRequisicaoRastreamentoFluxos, TipoRequisicaoEnum.FLUXO, dataRequisicaoRastreamento, null, "v1/Fluxo/{papel}/", StatusProcessamentoEnum.PROCESSANDO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
            try
            {
                var parallelOptions = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 15
                };
                Parallel.ForEach(message, parallelOptions, cadastro =>
                {
                    FIOnInstrumentContract[] cadastroBdLuz = new FIOnInstrumentContract[0];
                    string CodSNALuz = cadastro.CodigoSNA;

                    if (HOMOLOGACAO)
                    {
                        CodSNALuz = Utils.CodSnaHomologacao(cadastro.CodigoSNA);
                        cadastroBdLuz = _fixedIncomeService.GetFIOnInstrumentBySNA(CodSNALuz);

                        if (cadastroBdLuz.Length == 0)
                        {
                            _logger.LogInformation($"Iniciando um novo cadastro para o ativo {CodSNALuz}.");

                            cadastroBdLuz = _fixedIncomeService.GetFIOnInstrumentBySNA(cadastro.CodigoSNA);
                            if (cadastroBdLuz.Length == 0)
                            {
                                cadastroBdLuz = new FIOnInstrumentContract[1];
                                cadastroBdLuz[0] = new FIOnInstrumentContractDefault().papelPadrao();
                            }
                            cadastroBdLuz[0].CodAtivo = 0;
                        }
                    }
                    else
                    {
                        cadastroBdLuz = _fixedIncomeService.GetFIOnInstrumentBySNA(CodSNALuz);

                        if (cadastroBdLuz.Length == 0)
                        {
                            _logger.LogInformation($"Iniciando um novo cadastro para o ativo {CodSNALuz}.");
                            cadastroBdLuz = new FIOnInstrumentContract[1];
                            cadastroBdLuz[0] = new FIOnInstrumentContractDefault().papelPadrao();
                            cadastroBdLuz[0].CodAtivo = 0;
                        }
                    }

                    cadastroBdLuz[0].AJusteDiaUtil = 0;
                    cadastroBdLuz[0].ArredondamentoMoeda = "Arredonda";
                    cadastroBdLuz[0].ArredondamentoTaxa = "Arredonda";
                    cadastroBdLuz[0].AtivoTvm = true;
                    cadastroBdLuz[0].BaseAnual = _decodeLuzMapper.BaseAnualMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].BaseAnualPrincipal = _decodeLuzMapper.BaseAnualPrincipalMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].BaseAnualCarregamento = _decodeLuzMapper.BaseAnualCarregamentoMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].ClassTaxa = cadastro.DebentureIncentivada;
                    cadastroBdLuz[0].CodCetip = CodSNALuz;
                    cadastroBdLuz[0].ConvAcao = cadastro.ConversivelAcao;
                    cadastroBdLuz[0].Curvas = _auxiliaryMessageProcessor.ProcessCurvas(cadastroBdLuz, cadastro);
                    cadastroBdLuz[0].Custodia = _decodeLuzMapper.CustodiaMap(cadastro.Clearing);
                    cadastroBdLuz[0].Defasagens = _auxiliaryMessageProcessor.ProcessDefasagens(cadastroBdLuz, cadastro);
                    cadastroBdLuz[0].DiaAniversario = _decodeLuzMapper.DiaAniversarioMap(cadastro.TipoAniversario, cadastro.DataInicioRentabilidade, cadastro.DataVencimento, cadastro.DiaReferenciaIndexador);
                    cadastroBdLuz[0].DiaPagamentoFinal = 0;
                    cadastroBdLuz[0].DiaPagamentoVariavel = "UTI";
                    cadastroBdLuz[0].DirecaoMontagem = 2;
                    cadastroBdLuz[0].DtEmissao = cadastro.DataEmissao;
                    cadastroBdLuz[0].DtIniRent = cadastro.DataInicioRentabilidade;
                    cadastroBdLuz[0].DtVencimento = cadastro.DataVencimento;
                    cadastroBdLuz[0].Excecoes = _fixedIncomeService.GetInstrumentException(cadastroBdLuz[0].CodAtivo ?? 0);
                    cadastroBdLuz[0].Fluencia = 0;
                    cadastroBdLuz[0].GrupoContabil = _decodeLuzMapper.GrupoContabilMap(cadastro.Tipo);
                    cadastroBdLuz[0].ISIN = cadastro.Isin;
                    cadastroBdLuz[0].InstrCvm = cadastro.InstrucaoCVM;
                    cadastroBdLuz[0].ModeloPrazo = _decodeLuzMapper.ModeloPrazoMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].ModeloPrazoPre = _decodeLuzMapper.ModeloPrazoPreMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].ModeloPrazoPrincipal = _decodeLuzMapper.ModeloPrazoPrincipalMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].ModeloTaxa = _decodeLuzMapper.ModeloTaxaMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].ModeloTaxaPre = _decodeLuzMapper.ModeloTaxaPreMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].ModeloTaxaPrincipal = _decodeLuzMapper.ModeloTaxaPrincipalMap(cadastro.CriterioCalculoIndexador, cadastro.CriterioCalculoJuros);
                    cadastroBdLuz[0].MoedaProjecao = _decodeLuzMapper.MoedaProjecaoMap(cadastro.Indexador, cadastro.Projecao, cadastro.DefasagemIndexador);
                    cadastroBdLuz[0].MoedaProjecaoCompl = _decodeLuzMapper.MoedaProjecaoComplMap(cadastro.Indexador, cadastro.Projecao, cadastro.DefasagemIndexador);
                    cadastroBdLuz[0].PartLucros = false;
                    cadastroBdLuz[0].PeriodoAmortiza = 0;
                    cadastroBdLuz[0].PeriodoJuros = 0;
                    cadastroBdLuz[0].PrecisaoInternaMoeda = 16;
                    cadastroBdLuz[0].PrecisaoSaidaMoeda = 8;
                    cadastroBdLuz[0].PrecisaoTaxa = 9;
                    cadastroBdLuz[0].PrecisaoTaxaPrincipal = 9;
                    cadastroBdLuz[0].ProRata = 1;
                    cadastroBdLuz[0].ProductCode = _decodeLuzMapper.ProductCodeMap(cadastro.Tipo);
                    cadastroBdLuz[0].ProjetarIndice = _decodeLuzMapper.ProjetarIndiceMap(cadastro.Indexador, cadastro.Projecao, cadastro.DefasagemIndexador);
                    cadastroBdLuz[0].ResgateAnt = cadastro.PossibilidadeResgateAntecipado;
                    cadastroBdLuz[0].Status = true;
                    cadastroBdLuz[0].TipoAmortiza = _decodeLuzMapper.TipoAmortizaMap(cadastro.Amortizacao);
                    cadastroBdLuz[0].TipoAniversario = _decodeLuzMapper.TipoAniversarioMap(cadastro.TipoAniversario);
                    cadastroBdLuz[0].TipoInstrumento = _decodeLuzMapper.TipoInstrumentoMap(cadastro.Tipo);
                    cadastroBdLuz[0].TipoJuros = _decodeLuzMapper.TipoJurosMap(cadastro.Indexador);
                    cadastroBdLuz[0].TipoNominal = _decodeLuzMapper.TipoNominalMap();
                    cadastroBdLuz[0].TipoProjecao = _decodeLuzMapper.TipoProjecaoMap(cadastro.Indexador, cadastro.Projecao, cadastro.DefasagemIndexador);
                    cadastroBdLuz[0].TipoProjecaoCompl = _decodeLuzMapper.TipoProjecaoComplMap(cadastro.Indexador, cadastro.Projecao, cadastro.DefasagemIndexador);
                    cadastroBdLuz[0].TipoRegime = _decodeLuzMapper.TipoRegimeMap(cadastro.TipoRegime);
                    cadastroBdLuz[0].TipoUnidadeTempoAmortiza = 7;
                    cadastroBdLuz[0].TipoUnidadeTempoJuros = 7;
                    cadastroBdLuz[0].TipoVencimento = _decodeLuzMapper.TipoVencimentoMap();
                    cadastroBdLuz[0].Usuario = _config["Usuario"];
                    cadastroBdLuz[0].ValorNominal = (double?)cadastro.ValorNominalEmissao;

                    if (cadastro.CnpjEmissor != null)
                    {
                        var validCge = _fixedIncomeService.GetCge(cadastro.CnpjEmissor);
                        if (validCge != 0)
                            cadastroBdLuz[0].CgeEmissor = validCge;
                        else
                            emissorPendencia.Add(cadastroBdLuz[0].CodCetip);
                    }
                    else
                        emissorPendencia.Add(cadastroBdLuz[0].CodCetip);

                    if (cadastro.CnpjAgenteFiduciario != null)
                    {
                        var validCge = _fixedIncomeService.GetCgeFiduciario(cadastro.CnpjAgenteFiduciario);
                        if (validCge != 0)
                            cadastroBdLuz[0].CgeFiduciario = validCge;
                        else
                            fiduciarioPendencia.Add(cadastroBdLuz[0].CodCetip);
                    }
                    else
                        fiduciarioPendencia.Add(cadastroBdLuz[0].CodCetip);

                    if (cadastro.CnpjDevedor != null && (cadastro.Tipo.Equals("CRI") || cadastro.Tipo.Equals("CRA")))
                    {
                        var validCge = _fixedIncomeService.GetCgeGarantidor(cadastro.CnpjDevedor);
                        if (validCge != 0)
                        {
                            cadastroBdLuz[0].ListaGarantidores = new TipoGarantidorContract[1];
                            cadastroBdLuz[0].ListaGarantidores[0] = new TipoGarantidorContract()
                            {
                                CodAtivo = cadastroBdLuz[0].CodAtivo ?? 0,
                                CodCge = validCge
                            };
                        }
                        else
                            garantidorPendencia.Add(cadastroBdLuz[0].CodCetip);
                    }
                    else if(cadastro.CnpjDevedor == null)
                        garantidorPendencia.Add(cadastroBdLuz[0].CodCetip);

                    cadastroBdLuz[0].Fluxos = _auxiliaryMessageProcessor.ProcessFluxosRastreamento(cadastroBdLuz, cadastro, DATA_ATUALIZACAO, idRequisicaoRastreamentoFluxos.ToString(), JsonConvert.SerializeObject(dataRequisicaoRastreamento).Replace("\"", ""), usuarioRastreamento, HOMOLOGACAO);
                    _rastreamento.Papel(idRequisicaoRastreamentoFluxos, cadastroBdLuz[0].CodCetip, dataRequisicaoRastreamento, null, TipoLogEnum.FLUXO, StatusMensagemEnum.PROCESSADO_MDP, StatusProcessamentoEnum.PROCESSANDO, "", usuarioRastreamento);
                    cadastroBdLuz[0].ListaAniversarios = _decodeLuzMapper.ListaAniversarios(cadastro.TipoAniversario, cadastroBdLuz[0].Fluxos.ToArray()).ToArray();

                    impactoList.Add(cadastroBdLuz[0]);
                });

                foreach (FIOnInstrumentContract cadastroMDP in impactoList)
                {
                    try
                    {
                        var codAtivoPapel = _fixedIncomeService.InsertUpdateInstrument(cadastroMDP);
                        _logger.LogInformation($"Ativo {cadastroMDP.CodCetip} atualizado");

                        _auxiliaryMessageProcessor.PapeisEmHomologacaoException(cadastroMDP, codAtivoPapel);
                        string pendenciaMessage = _auxiliaryMessageProcessor.GerarPendencias(cadastroMDP.CodCetip, codAtivoPapel, emissorPendencia, garantidorPendencia, fiduciarioPendencia, Utils.messageToCadastro(message, cadastroMDP.CodCetip).CnpjEmissor, Utils.messageToCadastro(message, cadastroMDP.CodCetip).CnpjDevedor, Utils.messageToCadastro(message, cadastroMDP.CodCetip).CnpjAgenteFiduciario);
                        if(pendenciaMessage != null)
                        {
                            var rabbitMessage = new RastreamentoPapel() { IdRequisicao = idRequisicaoRastreamento, Papel = cadastroMDP.CodCetip, DataInicioEvento = dataRequisicaoRastreamento, DataFimEvento = DateTime.Now, TipoLog = TipoLogEnum.CADASTRO.ToString(), StatusMensagem = StatusMensagemEnum.ERRO_MDP.ToString(), StatusPapel = StatusProcessamentoEnum.ERRO.ToString(), MensagemErro = pendenciaMessage, Usuario = usuarioRastreamento };
                            _rabbitSender.SendObj("DeadLetterCadastroQueue", rabbitMessage, true);
                            _rastreamento.Evento(idRequisicaoRastreamento, TipoRequisicaoEnum.CADASTRO, dataRequisicaoRastreamento, DateTime.Now, "v2/Caracteristica/relatorio-dia/", StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, pendenciaMessage), usuarioRastreamento);
                            _rastreamento.Papel(idRequisicaoRastreamentoFluxos, cadastroMDP.CodCetip, dataRequisicaoRastreamento, DateTime.Now, TipoLogEnum.FLUXO, StatusMensagemEnum.FINALIZADO, StatusProcessamentoEnum.SUCESSO, "", usuarioRastreamento);
                            erroEvento = true;
                        }
                        else
                        {
                            _rastreamento.Papel(idRequisicaoRastreamento, cadastroMDP.CodCetip, dataRequisicaoRastreamento, DateTime.Now, TipoLogEnum.CADASTRO, StatusMensagemEnum.FINALIZADO, StatusProcessamentoEnum.SUCESSO, "", usuarioRastreamento);
                            _rastreamento.Papel(idRequisicaoRastreamentoFluxos, cadastroMDP.CodCetip, dataRequisicaoRastreamento, DateTime.Now, TipoLogEnum.FLUXO, StatusMensagemEnum.FINALIZADO, StatusProcessamentoEnum.SUCESSO, "", usuarioRastreamento);
                        }
                    }
                    catch (CamposObrigatoriosException e)
                    {
                        string errorMessage;
                        if (e.Message.Contains("Erro ao carregar CodeDecode"))
                        {
                            errorMessage = "Serviço CodeDecode indisponível. Tente novamente mais tarde.";
                        }
                        else
                            errorMessage = _decodeLuzMapper.AusenciaDadosMsg(cadastroMDP, message);

                        _logger.LogError($"O ativo {cadastroMDP.CodCetip} nao foi atualizado. {errorMessage} {e.Message}");
                        _logger.LogInformation($"Enviando o ativo {cadastroMDP.CodCetip} para DeadLetterCadastroQueue");

                        var rabbitMessage = new RastreamentoPapel() { IdRequisicao = idRequisicaoRastreamento, Papel = cadastroMDP.CodCetip, DataInicioEvento = dataRequisicaoRastreamento, DataFimEvento = DateTime.Now, TipoLog = TipoLogEnum.CADASTRO.ToString(), StatusMensagem = StatusMensagemEnum.ERRO_MDP.ToString(), StatusPapel = StatusProcessamentoEnum.ERRO.ToString(), MensagemErro = errorMessage, Usuario = usuarioRastreamento };
                        _rabbitSender.SendObj("DeadLetterCadastroQueue", rabbitMessage, true);
                        _rastreamento.Papel(idRequisicaoRastreamentoFluxos, cadastroMDP.CodCetip, dataRequisicaoRastreamento, DateTime.Now, TipoLogEnum.FLUXO, StatusMensagemEnum.ERRO_MDP, StatusProcessamentoEnum.ERRO, errorMessage, usuarioRastreamento);
                        _rastreamento.Evento(idRequisicaoRastreamento, TipoRequisicaoEnum.CADASTRO, dataRequisicaoRastreamento, DateTime.Now, "v2/Caracteristica/relatorio-dia/", StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, "Erro ao processar cadastros."), usuarioRastreamento);
                        _rastreamento.Evento(idRequisicaoRastreamentoFluxos, TipoRequisicaoEnum.FLUXO, dataRequisicaoRastreamento, DateTime.Now, "v1/Fluxo/{papel}/", StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, "Erro ao processar fluxos."), usuarioRastreamento);
                        envioTracking = true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);

                Dictionary<string, int> nackList;
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));

                if (!_cacheNack.TryGetValue("nackList", out nackList))
                {
                    _logger.LogInformation("Inicializando lista de nack de cadastros");
                    _logger.LogInformation($"Nack 1 para o lote do {message[0].CodigoSNA}");
                    nackList = new Dictionary<string, int>();
                    nackList.Add(message[0].CodigoSNA, 1);
                    _cacheNack.Set("nackList", nackList, cacheEntryOptions);
                }
                else
                {
                    nackList = _cacheNack.Get<Dictionary<string, int>>("nackList");

                    if (nackList.Any(n => n.Key.Equals(message[0].CodigoSNA)))
                    {
                        if (nackList[message[0].CodigoSNA] >= 2)
                        {
                            envioTracking = true;
                            nackList[message[0].CodigoSNA] = nackList[message[0].CodigoSNA] + 1;
                        }
                        else
                            nackList[message[0].CodigoSNA] = nackList[message[0].CodigoSNA] + 1;

                        _cacheNack.Set("nackList", nackList, cacheEntryOptions);
                        _logger.LogInformation($"Nack {nackList[message[0].CodigoSNA]} para o lote do {message[0].CodigoSNA}");
                    }
                    else
                    {
                        nackList.Add(message[0].CodigoSNA, 1);
                        _cacheNack.Set("nackList", nackList, cacheEntryOptions);
                        _logger.LogInformation($"Nack 1 para o lote do {message[0].CodigoSNA}");
                    }
                }
                if (envioTracking)
                {
                    var tracking = new List<RastreamentoPapel>();
                    _logger.LogInformation($"Servico indisponivel. Enviando para DeadLetterCadastroQueue.");
                    var errorMessage = "Serviço indisponível. Tente novamente mais tarde.";
                    foreach (DadosCaracteristicos trackingCadastro in message)
                    {
                        if (HOMOLOGACAO)
                            trackingCadastro.CodigoSNA = Utils.CodSnaHomologacao(trackingCadastro.CodigoSNA);

                        tracking.Add(new RastreamentoPapel() { IdRequisicao = idRequisicaoRastreamento, Papel = trackingCadastro.CodigoSNA, DataInicioEvento = dataRequisicaoRastreamento, DataFimEvento = DateTime.Now, TipoLog = TipoLogEnum.CADASTRO.ToString(), StatusMensagem = StatusMensagemEnum.ERRO_MDP.ToString(), StatusPapel = StatusProcessamentoEnum.ERRO.ToString(), MensagemErro = errorMessage, Usuario = usuarioRastreamento });
                    }
                    _rabbitSender.SendBulk<RastreamentoPapel>("DeadLetterCadastroQueue", tracking, true);
                    _rastreamento.Papeis(idRequisicaoRastreamentoFluxos, Utils.ListaSnaCadastros(message, HOMOLOGACAO), dataRequisicaoRastreamento, DateTime.Now, TipoLogEnum.FLUXO, StatusMensagemEnum.ERRO_MDP, StatusProcessamentoEnum.ERRO, errorMessage, usuarioRastreamento);
                    _rastreamento.Evento(idRequisicaoRastreamento, TipoRequisicaoEnum.CADASTRO, dataRequisicaoRastreamento, DateTime.Now, "v2/Caracteristica/relatorio-dia/", StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, "Erro ao processar cadastros."), usuarioRastreamento);
                    _rastreamento.Evento(idRequisicaoRastreamentoFluxos, TipoRequisicaoEnum.FLUXO, dataRequisicaoRastreamento, DateTime.Now, "v1/Fluxo/{papel}/", StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, "Erro ao processar fluxos."), usuarioRastreamento);
                }
                else
                    throw;
            }

            if (!envioTracking && !erroEvento)
            {
                _rastreamento.Evento(idRequisicaoRastreamento, TipoRequisicaoEnum.CADASTRO, dataRequisicaoRastreamento, DateTime.Now, "v2/Caracteristica/relatorio-dia/", StatusProcessamentoEnum.SUCESSO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
                _rastreamento.Evento(idRequisicaoRastreamentoFluxos, TipoRequisicaoEnum.FLUXO, dataRequisicaoRastreamento, DateTime.Now, "v1/Fluxo/{papel}/", StatusProcessamentoEnum.SUCESSO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
            }
            if(!envioTracking && erroEvento)
                _rastreamento.Evento(idRequisicaoRastreamentoFluxos, TipoRequisicaoEnum.FLUXO, dataRequisicaoRastreamento, DateTime.Now, "v1/Fluxo/{papel}/", StatusProcessamentoEnum.SUCESSO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
        }
        public void ProcessAssinatura(AssinaturaMdp message, IDictionary<string, object> headers)
        {
            var assinaturaMdp = _mapper.Map<AssinaturaMdp, FIOnAssinaturaBpoContract>(message);

            var assinaturaList = new List<FIOnAssinaturaBpoContract>();
            assinaturaList.Add(assinaturaMdp);

            try
            {
                _fixedIncomeService.SetAssinaturaBpo(assinaturaList);

            }
            catch (FaultException e)
            {

                _logger.LogInformation($"Enviando o ativo de assinatura {message.Papel} para DeadLetterAssinaturaQueue");

                JObject rabbitMessage = new JObject();
                rabbitMessage.Add("mensagem", JsonConvert.SerializeObject(message));
                rabbitMessage.Add("erro", e.Message);
                _rabbitSender.SendObj("DeadLetterAssinaturaQueue", rabbitMessage, true);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }

        public void ProcessPuDeEventos(List<DadosPreco> message, IDictionary<string, object> headers)
        {
            bool HOMOLOGACAO = Convert.ToBoolean(Encoding.UTF8.GetString((byte[])headers["HOMOLOGACAO"]));
            var DATA_ATUALIZACAO = DateTime.Parse(Encoding.UTF8.GetString((byte[])headers["DATA_ATUALIZACAO"]));
            var idRequisicaoRastreamento = new Guid((byte[])headers["EVENTO_ID"]);
            var dataRequisicaoRastreamento = (DateTime)JsonConvert.DeserializeObject(Encoding.UTF8.GetString((byte[])headers["EVENTO_DATAINICIO"]));
            string usuarioRastreamento = null;
            if (headers["EVENTO_USER"] != null)
                usuarioRastreamento = Encoding.UTF8.GetString((byte[])headers["EVENTO_USER"]);

            _rastreamento.Papeis(idRequisicaoRastreamento, Utils.ListaSnaPuDeEventos(message, HOMOLOGACAO), dataRequisicaoRastreamento, null, TipoLogEnum.EVENTO, StatusMensagemEnum.PROCESSADO_MDP, StatusProcessamentoEnum.PROCESSANDO, "", usuarioRastreamento);

            var pusDeEventos = _auxiliaryMessageProcessor.MontaObjetosPuDeEvento(message, HOMOLOGACAO, DATA_ATUALIZACAO);

            foreach (PuDeEventos puDeEvento in pusDeEventos)
            {
                try
                {
                    var puDeEventoExistente = _puDeEventos.Get(puDeEvento.codigo_instrumento, puDeEvento.dataLiquidacao, puDeEvento.tipo_evento, puDeEvento.data_referencia);

                    if (puDeEventoExistente == null)
                    {
                        if (puDeEvento.pu_do_evento_open != 0)
                        {
                            var idEvento = _puDeEventos.GerarPuEventoOperacao(puDeEvento);
                            _logger.LogInformation($"PU de evento {idEvento} incluido para o ativo {puDeEvento.codigo_instrumento}");
                        }
                    }
                    else if (puDeEventoExistente.situacaoInterna == "PEN")
                    {
                        if (puDeEvento.pu_do_evento_open == 0)
                        {
                            _puDeEventos.DeletePuDeEventos(puDeEventoExistente.idEvento);
                            _logger.LogInformation($"PU de evento {puDeEventoExistente.idEvento} cancelado para o ativo {puDeEventoExistente.codigo_instrumento}");
                        }
                        else
                        {
                            puDeEventoExistente.pu_do_evento_open = (double)puDeEvento.pu_do_evento_open;
                            _puDeEventos.UpdatePuDeEventos(puDeEventoExistente);
                            _logger.LogInformation($"PU de evento {puDeEventoExistente.idEvento} atualizado para o ativo {puDeEventoExistente.codigo_instrumento}");
                        }
                    }
                    else
                        _logger.LogInformation($"PU de evento {puDeEventoExistente.idEvento} nao atualizado para o ativo {puDeEventoExistente.codigo_instrumento}. Evento nao esta pendente.");

                    _rastreamento.Papel(idRequisicaoRastreamento, puDeEvento.codigo_instrumento, dataRequisicaoRastreamento, DateTime.Now, TipoLogEnum.EVENTO, StatusMensagemEnum.FINALIZADO, StatusProcessamentoEnum.SUCESSO, "", usuarioRastreamento);
                }
                catch (Exception e)
                {
                    var errorMessage = $"Nao foi possivel atualizar o evento {puDeEvento.idEvento} para o ativo {puDeEvento.codigo_instrumento}. ";
                    _logger.LogError(errorMessage + e.Message);
                    var rabbitMessage = new RastreamentoPapel() { IdRequisicao = idRequisicaoRastreamento, Papel = puDeEvento.codigo_instrumento, DataInicioEvento = dataRequisicaoRastreamento, DataFimEvento = DateTime.Now, TipoLog = TipoLogEnum.EVENTO.ToString(), StatusMensagem = StatusMensagemEnum.ERRO_MDP.ToString(), StatusPapel = StatusProcessamentoEnum.ERRO.ToString(), MensagemErro = errorMessage, Usuario = usuarioRastreamento };
                    _rabbitSender.SendObj("DeadLetterPuDeEventosQueue", rabbitMessage, true);
                    _rastreamento.Evento(idRequisicaoRastreamento, TipoRequisicaoEnum.EVENTO, dataRequisicaoRastreamento, DateTime.Now, "v1/PuDeEventos/relatorio-dia/", StatusProcessamentoEnum.ERRO, Utils.JsonErrorMessage(message, "Erro ao processar pu de eventos."), usuarioRastreamento);
                    throw e;
                }
            }
            _rastreamento.Evento(idRequisicaoRastreamento, TipoRequisicaoEnum.EVENTO, dataRequisicaoRastreamento, DateTime.Now, "v1/PuDeEventos/relatorio-dia/", StatusProcessamentoEnum.SUCESSO, Utils.JsonErrorMessage(message, null), usuarioRastreamento);
        }
    }
}