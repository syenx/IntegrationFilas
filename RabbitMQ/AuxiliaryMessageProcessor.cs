using AutoMapper;
using EDM.Infohub.BPO.Models.SQS;
using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration.Models;
using EDM.Infohub.Integration.RabbitMQ.impl;
using EDM.Infohub.Integration.Services;
using EDM.Infohub.Integration.Services.Impl;
using EDM.Infohub.Integration.SQS.Interfaces;
using EDMCommonService;
using EDMFixedIncomeOnService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.RabbitMQ
{
    public class AuxiliaryMessageProcessor
    {
        private ILogger<AuxiliaryMessageProcessor> _logger;
        private IEDMCommonService _commonService;
        private readonly IFixedIncomeOn _fixedIncomeService;
        private readonly IMapper _mapper;
        private readonly IEDMLuzService _edmLuzService;
        private ISender _rabbitSender;
        private readonly DecodeLuzMapper _decodeLuzMapper;
        private readonly IConfiguration _config;
        private RastreamentoProcessor _rastreamento;

        public AuxiliaryMessageProcessor(IEDMCommonService commonService, IFixedIncomeOn fixedIncomeService, ILogger<AuxiliaryMessageProcessor> logger, IMapper mapper, IEDMLuzService luzService, ISender rabbitSender, DecodeLuzMapper decodeLuzMapper, IConfiguration config, RastreamentoProcessor rastreamento)
        {
            _logger = logger;
            _commonService = commonService;
            _fixedIncomeService = fixedIncomeService;
            _mapper = mapper;
            _edmLuzService = luzService;
            _rabbitSender = rabbitSender;
            _decodeLuzMapper = decodeLuzMapper;
            _config = config;
            _rastreamento = rastreamento;
        }

        internal FIOnCurveContract[] ProcessCurvas(FIOnInstrumentContract[] cadastroBdLuz, DadosCaracteristicos cadastro)
        {
            FIOnCurveContract[] curvasPapel = new FIOnCurveContract[1];
            curvasPapel[0] = new FIOnCurveContract
            {
                CodAtivo = cadastroBdLuz[0].CodAtivo ?? 0,
                CodInstrumento = cadastroBdLuz[0].CodCetip,
                DataInicio = cadastro.DataInicioRentabilidade,
                DataFim = cadastro.DataVencimento,
                ValorNominalTaxa = (double)cadastro.TaxaPre,
                CodIndice = _decodeLuzMapper.CodIndiceMap(cadastro.Indexador),
                CodIndicePrecificacao = _decodeLuzMapper.CodIndicePrecificacaoMap(cadastro.Indexador),
                PercentualIndice = (double)cadastro.TaxaPos>0? (double)cadastro.TaxaPos : 100d ,
                CondicaoResgate = "N",
                NaoConsideraDeflacao = _decodeLuzMapper.NaoConsideraDeflacaoMap(cadastro.Projecao),
                Split = 1,
                Calc_VNA_flat = false
            };

            return curvasPapel;
        }

        internal FIOnLagContract[] ProcessDefasagens(FIOnInstrumentContract[] cadastroBdLuz, DadosCaracteristicos cadastro)
        {
            if(cadastro.Indexador != "CDI")
            {
                if (_decodeLuzMapper.ExisteDefasagem(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador))
                {
                    FIOnLagContract[] defasagensPapel = new FIOnLagContract[1];
                    defasagensPapel[0] = new FIOnLagContract
                    {
                        CodAtivo = cadastroBdLuz[0].CodAtivo ?? 0,
                        CodInstrumento = cadastroBdLuz[0].CodCetip,
                        TipoUnidadeTempo = _decodeLuzMapper.TipoUnidadeTempoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador,1),
                        PeriodoDefasagem = _decodeLuzMapper.PeriodoDefasagemMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador, 1),
                        TipoUnidadeTempoInflacao = _decodeLuzMapper.TipoUnidadeTempoInflacaoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador,1),
                        PeriodoDefasagemInflacao = _decodeLuzMapper.PeriodoDefasagemInflacaoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador),
                        MesDeReferenciaParaAtualizacao = _decodeLuzMapper.MesDeReferenciaParaAtualizacaoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador, cadastro.MesReferenciaIndexador,1),
                        DirecaoMontagemFluxo = "Emissão para vencimento",
                        DataVigencia = cadastro.DataVencimento
                    };

                    return defasagensPapel;
                }
            }
            else
            {
                if (_decodeLuzMapper.ExisteDefasagemCDI(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador))
                {
                    FIOnLagContract[] defasagensPapel = new FIOnLagContract[1];
                    defasagensPapel[0] = new FIOnLagContract
                    {
                        CodAtivo = cadastroBdLuz[0].CodAtivo ?? 0,
                        CodInstrumento = cadastroBdLuz[0].CodCetip,
                        TipoUnidadeTempo = _decodeLuzMapper.TipoUnidadeTempoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador,2),
                        PeriodoDefasagem = _decodeLuzMapper.PeriodoDefasagemMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador, 2),
                        TipoUnidadeTempoInflacao = _decodeLuzMapper.TipoUnidadeTempoInflacaoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador,2),
                        PeriodoDefasagemInflacao = _decodeLuzMapper.PeriodoDefasagemCDIInflacaoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador),
                        MesDeReferenciaParaAtualizacao = _decodeLuzMapper.MesDeReferenciaParaAtualizacaoMap(cadastro.PeriodicidadeCorrecao, cadastro.UnidadeIndexador, cadastro.DefasagemIndexador, cadastro.MesReferenciaIndexador,2),
                        DirecaoMontagemFluxo = "Emissão para vencimento",
                        DataVigencia = cadastro.DataVencimento
                    };

                    return defasagensPapel;
                }
            }
            
            return new FIOnLagContract[0];
        }

        internal FIOnCashFlowContract[] ProcessFluxos(FIOnInstrumentContract[] cadastroBdLuz, DadosCaracteristicos cadastro, DateTime DATA_ATUALIZACAO)
        {
            try
            {
                var fluxosBtg = new List<FIOnCashFlowContract>();
                List<Fluxos> fluxosPapel = null;

                fluxosPapel = _edmLuzService.FluxoPapel(DATA_ATUALIZACAO, cadastro.CodigoSNA).Result;

                foreach (Fluxos fluxo in fluxosPapel)
                {
                    var contratoFluxoBtg = new FIOnCashFlowContract()
                    {
                        CodAtivo = cadastroBdLuz[0].CodAtivo ?? 0,
                        CodInstrumento = cadastroBdLuz[0].CodCetip,
                        DataEventoReal = fluxo.DataBase,
                        DataFluxo = fluxo.DataLiquidacao,
                        Usuario = _config["Usuario"],
                        Valor = fluxo.Taxa,
                        CodTipoFluxo = _decodeLuzMapper.CodTipoFluxoMap(fluxo.TipoEvento)
                    };
                    if (!fluxosBtg.Any(f => f.DataFluxo == contratoFluxoBtg.DataFluxo && f.CodTipoFluxo == contratoFluxoBtg.CodTipoFluxo) && contratoFluxoBtg.CodTipoFluxo != 0)
                        fluxosBtg.Add(contratoFluxoBtg);
                    else
                        _logger.LogInformation($"Fluxo {fluxo.TipoEvento} com DataLiquidacao = {fluxo.DataLiquidacao} do papel {cadastroBdLuz[0].CodCetip} eh repetido ou se refere a um premio, por isso nao foi adicionado.");
                }

                return fluxosBtg.ToArray();
            }
            catch
            {
                throw;
            }
        }

        internal FIOnCashFlowContract[] ProcessFluxosRastreamento(FIOnInstrumentContract[] cadastroBdLuz, DadosCaracteristicos cadastro, DateTime DATA_ATUALIZACAO, string idRequisicaoEvento, string dataRequisicaoRastreamento, string usuarioRastreamento, bool papelHomologacao)
        {
            try
            {
                var fluxosBtg = new List<FIOnCashFlowContract>();
                List<Fluxos> fluxosPapel = null;

                fluxosPapel = _edmLuzService.FluxoPapel(DATA_ATUALIZACAO, cadastro.CodigoSNA, idRequisicaoEvento, dataRequisicaoRastreamento, usuarioRastreamento, papelHomologacao).Result;

                foreach (Fluxos fluxo in fluxosPapel)
                {
                    var contratoFluxoBtg = new FIOnCashFlowContract()
                    {
                        CodAtivo = cadastroBdLuz[0].CodAtivo ?? 0,
                        CodInstrumento = cadastroBdLuz[0].CodCetip,
                        DataEventoReal = fluxo.DataBase,
                        DataFluxo = fluxo.DataLiquidacao,
                        Usuario = _config["Usuario"],
                        Valor = _decodeLuzMapper.RoundTaxa(fluxo.Taxa, fluxo.TipoEvento),
                        CodTipoFluxo = _decodeLuzMapper.CodTipoFluxoMap(fluxo.TipoEvento)
                    };
                    if (!fluxosBtg.Any(f => f.DataFluxo == contratoFluxoBtg.DataFluxo && f.CodTipoFluxo == contratoFluxoBtg.CodTipoFluxo) && contratoFluxoBtg.CodTipoFluxo != 0)
                        fluxosBtg.Add(contratoFluxoBtg);
                    else
                        _logger.LogInformation($"Fluxo {fluxo.TipoEvento} com DataLiquidacao = {fluxo.DataLiquidacao} do papel {cadastroBdLuz[0].CodCetip} eh repetido ou se refere a um premio, por isso nao foi adicionado.");
                }

                return fluxosBtg.ToArray();
            }
            catch
            {
                throw;
            }
        }

        internal void PapeisEmHomologacaoException(FIOnInstrumentContract cadastroBdLuz, int codAtivo)
        {
            try
            {
                if (cadastroBdLuz.CodCetip.Contains("L-"))
                {
                    var excecao = _decodeLuzMapper.ExcecaoEmAnaliseMap(codAtivo);
                    if (!cadastroBdLuz.Excecoes.Any(e => e.IdExcecao == excecao.IdExcecao))
                    {
                        _fixedIncomeService.AddFIOnException(excecao);
                        _logger.LogInformation($"Papel {cadastroBdLuz.CodCetip} em homologacao. Excecao {excecao.IdExcecao} adicionada ao papel.");
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        internal List<PuDeEventos> MontaObjetosPuDeEvento(List<DadosPreco> relatorioPu, bool homologacao, DateTime data)
        {
            var eventos = _decodeLuzMapper.ConverteEventoLuzToMdp(relatorioPu, data);
            var puDeEventos = new List<PuDeEventos>();

            foreach(PuDeEventosLuz evento in eventos)
            {
                var codSna = evento.CodigoSNA;
                if (homologacao)
                {
                    codSna = Utils.CodSnaHomologacao(evento.CodigoSNA);
                }
                
                DateTime dataReferencia;
                if (evento.TipoEvento == "Vencimento")
                {
                    var cadastro = _edmLuzService.CadastroPapel(evento.CodigoSNA).Result;
                    if (cadastro != null)
                        dataReferencia = cadastro.DataVencimento;
                    else
                    {
                        dataReferencia = evento.DataEvento;
                        _logger.LogInformation($"cadastro do ativo {evento.CodigoSNA} nao encontrado. Data referencia igualada com data liquidacao.");
                    }
                }
                else
                {
                    var fluxos = _edmLuzService.FluxoPapel(data, evento.CodigoSNA).Result;
                    var fluxoDataReferencia = fluxos.Where(d => d.CodigoSNA == evento.CodigoSNA && d.DataLiquidacao == evento.DataEvento && d.TipoEvento == evento.TipoEvento).FirstOrDefault();
                    if (fluxoDataReferencia != null)
                        dataReferencia = fluxoDataReferencia.DataBase;
                    else
                    {
                        dataReferencia = evento.DataEvento;
                        _logger.LogInformation($"fluxo do ativo {evento.CodigoSNA} nao encontrado. Data referencia igualada com data liquidacao.");
                    }
                }

                puDeEventos.Add(new PuDeEventos
                {
                    codigo_instrumento = codSna,
                    data_referencia = dataReferencia,
                    tipo_evento = _decodeLuzMapper.TipoEventoMap(evento.TipoEvento),
                    pu_do_evento_open = (double)evento.PuEvento,
                    dataLiquidacao = evento.DataEvento
                });
            }
            return puDeEventos;
        }

        internal string GerarPendencias(string codSNA, int codAtivoPapel, List<string> emissorPendencia, List<string> garantidorPendencia, List<string> fiduciarioPendencia, string cnpjEmissor, string cnpjGarantidor, string cnpjFiduciario)
        {
            bool papelPendencia = false;
            string errorMessage = null;
            if (emissorPendencia.Contains(codSNA))
            {
                string descricaoPendencia = "CGE do emissor não encontrado.";
                errorMessage = errorMessage + descricaoPendencia + " ";
                InserirPendencia(codSNA, codAtivoPapel, descricaoPendencia, "CNPJ do Emissor", "CNPJ Emissor", cnpjEmissor);
                papelPendencia = true;
            }
            if (garantidorPendencia.Contains(codSNA))
            {
                string descricaoPendencia = "CGE do garantidor não encontrado.";
                errorMessage = errorMessage + descricaoPendencia + " ";
                InserirPendencia(codSNA, codAtivoPapel, descricaoPendencia, "CNPJ do Garantidor", "CNPJ Garantidor", cnpjGarantidor);
                papelPendencia = true;
            }
            if (fiduciarioPendencia.Contains(codSNA))
            {
                string descricaoPendencia = "CGE do agente fiduciário não encontrado.";
                errorMessage = errorMessage + descricaoPendencia;
                InserirPendencia(codSNA, codAtivoPapel, descricaoPendencia, "CNPJ do Agente Fiduciário", "CNPJ Fiduciario", cnpjFiduciario);
                papelPendencia = true;
            }
            if (papelPendencia)
            {
                _logger.LogInformation(errorMessage);
                errorMessage = "Cadastro atualizado com pendência. " + errorMessage;
            }
            return errorMessage;
        }
        internal void InserirPendencia(string codSNA, int codAtivo, string descricaoPendencia,string descricaoCampoGlobal, string campoPendencia, string cnpjPendente)
        {
            var pendencia = new PendenciaAtivoDataContract()
            {
                CodInstrumento = codSNA,
                CodAtivo = codAtivo,
                Descricao = descricaoPendencia,
                IdTipoContratoCanonico = 12,
                TipoContratoCanonico = "DEBT_ON"
            };

            var valorPendencia = new ValorPendenciaAtivoDataContract()
            {
                CampoPendencia = campoPendencia,
                CampoGlobal = descricaoCampoGlobal,
                ValorOriginal = cnpjPendente
            };

            _commonService.InserirPendencia(pendencia, valorPendencia);
        }
    }
}