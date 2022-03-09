using Amazon.CloudWatchLogs.Model.Internal.MarshallTransformations;
using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration.Services.Impl;
using EDMCommonService;
using EDMFixedIncomeOnService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    public class DecodeLuzMapper
    {
        private IMemoryCache _cache;
        private readonly IFixedIncomeOn _fixedIncomeService;
        private readonly ILogger<DecodeLuzMapper> _logger;
        private readonly FIOnDomainContract[] _dominios;
        private readonly IConfiguration _config;
        public DecodeLuzMapper(IMemoryCache cache, IFixedIncomeOn fixedIncomeService, ILogger<DecodeLuzMapper> logger, IConfiguration config)
        {
            _cache = cache;
            _fixedIncomeService = fixedIncomeService;
            _logger = logger;
            _dominios = GetDomains();
            _config = config;
        }
        public FIOnDomainContract[] GetDomains()
        {
            FIOnDomainContract[] dominios;
            try
            {
                if (!_cache.TryGetValue("DominiosMDP", out dominios))
                {
                    _logger.LogInformation("Inicializando dominios");
                    dominios = _fixedIncomeService.GetDomains();
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                    _cache.Set("DominiosMDP", dominios, cacheEntryOptions);
                }
                else
                {
                    dominios = _cache.Get<FIOnDomainContract[]>("DominiosMDP");
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Erro ao inicializar os dominios");
                throw e;
            }
            return dominios;            
        }
        internal FIOnInstrumentTypeContract TipoInstrumentoMap(string tp_papel)
        {
            FIOnInstrumentTypeContract result = new FIOnInstrumentTypeContract();

            if (tp_papel == "CRA") {
                result.Description = "CRA";
                result.InstrumentTypeId = 38;
                result.ProductId = 0;
                result.ProductName = null;
            }
            else if (tp_papel == "Debênture")
            {
                result.Description = "Debentures";
                result.InstrumentTypeId = 4;
                result.ProductId = 0;
                result.ProductName = null;
            }
            else if (tp_papel == "CRI")
            {
                result.Description = "CRI";
                result.InstrumentTypeId = 10;
                result.ProductId = 0;
                result.ProductName = null;
            }
            else if (tp_papel == "CCB")
            {
                result.Description = "CCB";
                result.InstrumentTypeId = 7;
                result.ProductId = 0;
                result.ProductName = null;
            }

            return result;
        }

        internal string GrupoContabilMap(string tp_papel)
        {
            //[{"key":"ADA", "value":"ADA"},{"key":"BOXC", "value":"BOXC"},{"key":"CCCB", "value":"CCCB"},{"key":"DIR_TC", "value":"DIR_TC"},{"key":"LCI", "value":"LCI"},{"key":"LCIV", "value":"LCIV"},  {"key":"LF-DI", "value":"LF-DI"},{"key":"TDA", "value":"TDA"},{"key":"ASTN", "value":"ASTN"},{"key":"CCE-COMP", "value":"CCE-COMP"},{"key":"CDB", "value":"CDB"},{"key":"CDB_ESC", "value":"CDB_ESC"},  {"key":"CDBV-DI", "value":"CDBV-DI"},{"key":"CFA", "value":"CFA"},{"key":"CPRC", "value":"CPRC"},{"key":"DIR", "value":"DIR"},{"key":"LCIX", "value":"LCIX"},{"key":"LH", "value":"LH"},  {"key":"CCI", "value":"CCI"},{"key":"CDCA", "value":"CDCA"},{"key":"CDI-PRGE", "value":"CDI-PRGE"},{"key":"CDP", "value":"CDP"},{"key":"CPRB", "value":"CPRB"},{"key":"CS", "value":"CS"},  {"key":"CTEE", "value":"CTEE"},{"key":"CTN", "value":"CTN"},{"key":"BOXV", "value":"BOXV"},{"key":"CCB PRE", "value":"CCB PRE"},{"key":"CDI-PRNF", "value":"CDI-PRNF"},{"key":"DIR-TL", "value":"DIR-TL"},  {"key":"DPGE_S", "value":"DPGE_S"},{"key":"NOTAC", "value":"NOTAC"},{"key":"NOTAP", "value":"NOTAP"},{"key":"CCB GER", "value":"CCB GER"},{"key":"CCI-COMP", "value":"CCI-COMP"},{"key":"CDB-DI", "value":"CDB-DI"},  {"key":"CDI-SUBX", "value":"CDI-SUBX"},{"key":"COE", "value":"COE"},{"key":"CRA", "value":"CRA"},{"key":"CRI", "value":"CRI"},{"key":"LCA-ACC", "value":"LCA-ACC"},{"key":"NC", "value":"NC"},  {"key":"CCE", "value":"CCE"},{"key":"CFF", "value":"CFF"},{"key":"CIA", "value":"CIA"},{"key":"DIRR", "value":"DIRR"},{"key":"DPGE", "value":"DPGE"},{"key":"LC", "value":"LC"},  {"key":"NCE-COMP", "value":"NCE-COMP"},{"key":"NOTA", "value":"NOTA"},{"key":"CCB", "value":"CCB"},{"key":"CDI", "value":"CDI"},{"key":"CDI-DI", "value":"CDI-DI"},{"key":"CPRD", "value":"CPRD"},  {"key":"FIDC", "value":"FIDC"},{"key":"LAM", "value":"LAM"},{"key":"LCA", "value":"LCA"},{"key":"LFSN", "value":"LFSN"},{"key":"NCE", "value":"NCE"},{"key":"RDB DPGE", "value":"RDB DPGE"},  {"key":"RDB-DI", "value":"RDB-DI"},{"key":"CCB-COMP", "value":"CCB-COMP"},{"key":"CDBS", "value":"CDBS"},{"key":"CDI-RURA", "value":"CDI-RURA"},{"key":"CPR", "value":"CPR"},{"key":"CVS", "value":"CVS"},  {"key":"DEB", "value":"DEB"},{"key":"LCAX", "value":"LCAX"},{"key":"LF", "value":"LF"},{"key":"LFS", "value":"LFS"},  {"key":"RDB", "value":"RDB"}, {"key":"DIRP", "value":"DIRP"}, {"key":"DIRG", "value":"DIRG"}, {"key":"LFG", "value":"LFG"}, {"key":"CBIO", "value":"CBIO"}]
            var dominiosGrupoContabil = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[23].ValueJson);

            if (tp_papel == "CRA")
                return dominiosGrupoContabil.Where(d => d.Key == "CRA").ToList().First().Value;
            else if (tp_papel == "Debênture")
                return dominiosGrupoContabil.Where(d => d.Key == "DEB").ToList().First().Value;
            else if (tp_papel == "CRI")
                return dominiosGrupoContabil.Where(d => d.Key == "CRI").ToList().First().Value;
            else if (tp_papel == "CCB")
                return dominiosGrupoContabil.Where(d => d.Key == "CCB").ToList().First().Value;
            else
                return null;
        }

        internal short? ProductCodeMap(string tp_papel)
        {
            if (tp_papel == "CRA")
                return 50;
            else if (tp_papel == "Debênture")
                return 69;
            else if (tp_papel == "CRI")
                return 52;
            else if (tp_papel == "CCB")
                return 1;
            else
                return null;
        }

        internal string TipoNominalMap()
        {
            //[{"key": "Emissão","value": "E"},{"key": "Resgate","value": "R"}]
            var dominiosTipoNominal = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[15].ValueJson);
            return dominiosTipoNominal.Where(d => d.Key == "Emissão").ToList().First().Value;
        }

        internal int TipoVencimentoMap()
        {
            //[{"key": "Normal","value": "0"},{"key": "Perpétuo","value": "1"}]
            var dominiosTipoVencimento = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[19].ValueJson);
            return dominiosTipoVencimento.Where(d => d.Key == "Normal").ToList().First().Value;
        }

        internal int? ModeloTaxaMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Exponencial" ,"value": "0"},{"key": "Taxa Over" ,"value": "1"},{"key": "Taxa Exp 252" ,"value": "2"},{"key": "Taxa Linear 360" ,"value": "3"},{"key": "Taxa Exp 360" ,"value": "4"},{"key": "Taxa YTM" ,"value": "5"},{"key": "Taxa Cupom" ,"value": "6"},{"key": "Taxa Exp Mensal" ,"value": "7"},{"key": "Sem Taxa" ,"value": "8"},{"key": "Taxa Exp 365" ,"value": "9"}]
            var dominiosModeloTaxa = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[6].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") || 
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosModeloTaxa.Where(d => d.Key == "Taxa Exp 252").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") || 
                     (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                     (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosModeloTaxa.Where(d => d.Key == "Taxa Exp 360").ToList().First().Value;
            else
                return null;
        }

        internal int? ModeloPrazoMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Sem Prazo","value": "0"},{"key": "Dias Corridos 360","value": "1"},{"key": "Dias Úteis","value": "2"},{"key": "Prazo 360","value": "3"},{"key": "Dias Corridos 365","value": "4"},{"key": "Número de meses X 30 por 360 - dias 360","value": "5"},{"key": "Número de meses X 30 por 365","value": "6"},{"key": "Número de meses X 21 por 252","value": "7"},{"key": "Número de meses X 30 por 360 - dias corridos","value": "8"}, {"key": "30 por 360 - dias corridos","value": "9"}]
            var dominiosModeloPrazo = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[8].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosModeloPrazo.Where(d => d.Key == "Dias Úteis").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                     (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosModeloPrazo.Where(d => d.Key == "Dias Corridos 360").ToList().First().Value;
            else
                return null;
        }

        internal int? ModeloPrazoPrincipalMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Sem Prazo","value": "0"},{"key": "Dias Corridos 360","value": "1"},{"key": "Dias Úteis","value": "2"},{"key": "Prazo 360","value": "3"},{"key": "Dias Corridos 365","value": "4"},{"key": "Número de meses X 30 por 360 - dias 360","value": "5"},{"key": "Número de meses X 30 por 365","value": "6"},{"key": "Número de meses X 21 por 252","value": "7"},{"key": "Número de meses X 30 por 360 - dias corridos","value": "8"}, {"key": "30 por 360 - dias corridos","value": "9"}]
            var dominiosModeloPrazo = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[8].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosModeloPrazo.Where(d => d.Key == "Dias Úteis").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosModeloPrazo.Where(d => d.Key == "Dias Corridos 360").ToList().First().Value;
            else
                return null;
        }

        internal int? ModeloTaxaPreMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{ "key": "Exponencial" ,"value": "0"},{ "key": "Taxa Over" ,"value": "1"},{ "key": "Taxa Exp 252" ,"value": "2"},{ "key": "Taxa Linear 360" ,"value": "3"},{ "key": "Taxa Exp 360" ,"value": "4"},{ "key": "Taxa YTM" ,"value": "5"},{ "key": "Taxa Cupom" ,"value": "6"},{ "key": "Taxa Exp Mensal" ,"value": "7"},{ "key": "Sem Taxa" ,"value": "8"},{ "key": "Taxa Exp 365" ,"value": "9"}]
            var dominiosMoedaTxJurPre = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[7].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") || 
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosMoedaTxJurPre.Where(d => d.Key == "Taxa Exp 252").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || 
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") || (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30")
                || (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") || (criterioCalculoIndexador == null && criterioCalculoJuros == "30")) //<d.u.>, <d.c.>, <30>, <30E>, <Actual>
                return dominiosMoedaTxJurPre.Where(d => d.Key == "Taxa Exp 360").ToList().First().Value;
            else
                return null;
        }

        internal int? ModeloTaxaPrincipalMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Exponencial" ,"value": "0"},{"key": "Taxa Over" ,"value": "1"},{"key": "Taxa Exp 252" ,"value": "2"},{"key": "Taxa Linear 360" ,"value": "3"},{"key": "Taxa Exp 360" ,"value": "4"},{"key": "Taxa YTM" ,"value": "5"},{"key": "Taxa Cupom" ,"value": "6"},{"key": "Taxa Exp Mensal" ,"value": "7"},{"key": "Sem Taxa" ,"value": "8"},{"key": "Taxa Exp 365" ,"value": "9"}]
            var dominiosModeloTaxa = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[6].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosModeloTaxa.Where(d => d.Key == "Taxa Exp 252").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosModeloTaxa.Where(d => d.Key == "Taxa Exp 360").ToList().First().Value;
            else
                return null;
        }

        internal string CustodiaMap(string clearing)
        {
            //[{"key": "BVMF","value": "BF"},
            //{"key": "CETIP","value": "CE"},
            //{"key": "SELIC","value": "SE"},
            //{"key": "CSD","value": "CS"}]
            var dominiosCustodia = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[2].ValueJson);
            switch (clearing)
            {                
                case "CETIP":
                    return dominiosCustodia.Where(d => d.Key == "CETIP").ToList().First().Value;
                case "BVMF":
                    return dominiosCustodia.Where(d => d.Key == "BVMF").ToList().First().Value;
                case "SELIC":
                    return dominiosCustodia.Where(d => d.Key == "SELIC").ToList().First().Value;
                case "CSD":
                    return dominiosCustodia.Where(d => d.Key == "CSD").ToList().First().Value;
                default:
                    return null;
            }
        }

        internal int? TipoRegimeMap(string tipoRegime)
        {
            //[{"key": "Registrado","value": "1"},{"key": "Depositado","value": "2"}]
            var dominiosTipoRegime = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[25].ValueJson);
            if (null == tipoRegime)
            {
                return null;
            }
            if (tipoRegime.Contains("Registrado"))
            {
                return dominiosTipoRegime.Where(d => d.Key == "Registrado").ToList().First().Value;
            } 
            if (tipoRegime.Contains("Depositado"))
            {
                return dominiosTipoRegime.Where(d => d.Key == "Depositado").ToList().First().Value;
            }
            else
            {
                return null;
            }
        }

        internal int? ModeloPrazoPreMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Sem Prazo","value": "0"},{"key": "Dias Corridos 360","value": "1"},{"key": "Dias Úteis","value": "2"},{"key": "Prazo 360","value": "3"},{"key": "Dias Corridos 365","value": "4"},{"key": "Número de meses X 30 por 360 - dias 360","value": "5"},{"key": "Número de meses X 30 por 365","value": "6"},{"key": "Número de meses X 21 por 252","value": "7"},{"key": "Número de meses X 30 por 360 - dias corridos","value": "8"}, {"key": "30 por 360 - dias corridos","value": "9"}]
            var dominiosModeloPrazoPre = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[9].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") || 
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosModeloPrazoPre.Where(d => d.Key == "Dias Úteis").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                     (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") ||
                     (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c."))
                return dominiosModeloPrazoPre.Where(d => d.Key == "Dias Corridos 360").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") ||
                     (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") ||
                     (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosModeloPrazoPre.Where(d => d.Key == "Número de meses X 30 por 360 - dias 360").ToList().First().Value;
            else
                return null;
        }

        internal int CodTipoFluxoMap(string tipoEvento)
        {
            //<Amortização Extra>, <Juros>, <Amortização>, <Incorporação> , <Prêmio>
            //[{"key": "Juros","value": "1"},{"key": "Amortização","value": "4"},{"key": "Incorporação","value": "8"},{"key": "Amortização Extraord.","value": "9"}]
            var dominiosCodTipoFluxo = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[26].ValueJson);
            switch (tipoEvento)
            {
                case "Amortização Extra":
                    return dominiosCodTipoFluxo.Where(d => d.Key == "Amortização Extraord.").ToList().First().Value;
                case "Juros":
                    return dominiosCodTipoFluxo.Where(d => d.Key == "Juros").ToList().First().Value;
                case "Amortização":
                    return dominiosCodTipoFluxo.Where(d => d.Key == "Amortização").ToList().First().Value;
                case "Incorporação":
                    return dominiosCodTipoFluxo.Where(d => d.Key == "Incorporação").ToList().First().Value;
                default:
                    return 0;
            }
        }

        internal double RoundTaxa(double valorTaxa, string tipoEvento)
        {
            switch (tipoEvento)
            {
                case "Amortização Extra":
                    return Math.Round(valorTaxa, 4);
                case "Amortização":
                    return Math.Round(valorTaxa, 4);
                default:
                    return valorTaxa;
            }
        }

        internal int? BaseAnualMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Ano 360","value": "360"},{"key": "Ano 252","value": "252"},{"key": "Ano 365","value": "365"},{"key": "Mês 30","value": "30"},{"key": "Sem","value": "0"}]
            var dominiosBaseIndexador = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[1].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosBaseIndexador.Where(d => d.Key == "Ano 252").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") || 
                     (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") || 
                     (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") ||
                     (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                     (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosBaseIndexador.Where(d => d.Key == "Ano 360").ToList().First().Value;
            else
                return null;
        }

        internal int? BaseAnualPrincipalMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Ano 360","value": "360"},{"key": "Ano 252","value": "252"},{"key": "Ano 365","value": "365"},{"key": "Mês 30","value": "30"},{"key": "Sem","value": "0"}]
            var dominiosBaseIndexador = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[1].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) || 
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosBaseIndexador.Where(d => d.Key == "Ano 252").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosBaseIndexador.Where(d => d.Key == "Ano 360").ToList().First().Value;
            else
                return null;
        }

        internal int? BaseAnualCarregamentoMap(string criterioCalculoIndexador, string criterioCalculoJuros)
        {
            //[{"key": "Ano 360","value": "360"},{"key": "Ano 252","value": "252"},{"key": "Ano 365","value": "365"},{"key": "Mês 30","value": "30"},{"key": "Sem","value": "0"}]
            var dominiosBaseIndexador = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[1].ValueJson);

            if ((criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == "30") || (criterioCalculoIndexador == "d.u." && criterioCalculoJuros == null) ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.u.") || (criterioCalculoIndexador == null && criterioCalculoJuros == null))
                return dominiosBaseIndexador.Where(d => d.Key == "Ano 252").ToList().First().Value;
            else if ((criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.u.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == "d.c." && criterioCalculoJuros == "30") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "d.c.") ||
                (criterioCalculoIndexador == null && criterioCalculoJuros == "30"))
                return dominiosBaseIndexador.Where(d => d.Key == "Ano 360").ToList().First().Value;
            else
                return null;
        }

        internal FIOnInstrumentExceptionDataContract ExcecaoMap(int CodAtivosLuz)
        {
            var excecaoCadastroBtg = new FIOnInstrumentExceptionDataContract()
            {
                CodAtivo = CodAtivosLuz,
                DataInclusao = DateTime.Now,
                Id = 0,
                IdExcecao = 139,
                IdTipoExcecao = 781,
                UserLogin = _config["Usuario"]
            };
            return excecaoCadastroBtg;
        }
        internal FIOnInstrumentExceptionDataContract ExcecaoEmAnaliseMap(int CodAtivosLuz)
        {
            var excecaoCadastroBtg = new FIOnInstrumentExceptionDataContract()
            {
                CodAtivo = CodAtivosLuz,
                DataInclusao = DateTime.Now,
                Id = 0,
                IdExcecao = 58,
                IdTipoExcecao = 722,
                UserLogin = _config["Usuario"]
            };
            return excecaoCadastroBtg;
        }
        //[{"key": "Pagamento no Final" ,"value": "0"},{"key": "Amortização de Juros em período uniforme" ,"value": "1"},{"key": "Amortização de Juros em período não uniforme" ,"value": "2"},{"key": "Amortização de Juros com indexador em período uniforme" ,"value": "3"},{"key": "Amortização de Juros com indexador em período não uniforme" ,"value": "4"},{"key": "Amortização de Juros e Atualização em período não uniforme" ,"value": "5"},{"key": "Fluxo gerado pela tabela price" ,"value": "6"}, {"key": "Pagamento de Juros Sobre Amortização" ,"value": "7"}]
        internal int? TipoJurosMap(string indexador)
        {
            var dominiosTipoJuros = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[13].ValueJson);
            if (indexador == "IGP-M" || indexador == "IPCA" || indexador == "INCC" || indexador == "IGP-DI" || indexador == "Pré" || indexador == "USD")
                return dominiosTipoJuros.Where(d => d.Key == "Amortização de Juros em período não uniforme").ToList().First().Value;
            else if (indexador == "CDI" || indexador == "TR")
                return dominiosTipoJuros.Where(d => d.Key == "Amortização de Juros com indexador em período não uniforme").ToList().First().Value;
            else
                return null;
        }
        //[{"key": "Pagamento no Final" ,"value": "0"},{"key": "Per. Var. sobre o saldo nominal em períodos não uniformes" ,"value": "1"},{"key": "Per. Var. sobre o saldo nominal em períodos uniformes" ,"value": "2"},{"key": "Per. Fix. sobre o saldo nominal em períodos uniformes" ,"value": "3"},{"key": "Per. Fix. sobre o saldo nominal em períodos não uniformes" ,"value": "4"},{"key": "Per. Var. sobre o valor nominal em períodos não uniformes" ,"value": "5"},{"key": "Per. Var. sobre o valor nominal em períodos uniformes" ,"value": "6"},{"key": "Per. Fix. sobre o valor nominal em períodos uniformes" ,"value": "7"},{"key": "Per. Fix. sobre o valor nominal em períodos não uniformes" ,"value": "8"},{"key": "Per. Fix. sobre o valor nominal com correção da parcela" ,"value": "9"},{"key": "Fluxo gerado pela tabela price" ,"value": "10"}]
        internal int? TipoAmortizaMap(string tipoAmortizacao)
        {
            var dominiosTipoAmortizacao = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[14].ValueJson);
            if (tipoAmortizacao == "Saldo Atualizado")
                return dominiosTipoAmortizacao.Where(d => d.Key == "Per. Var. sobre o saldo nominal em períodos não uniformes").ToList().First().Value;
            else if (tipoAmortizacao == "Valor Nominal de Emissão")
                return dominiosTipoAmortizacao.Where(d => d.Key == "Per. Var. sobre o valor nominal em períodos não uniformes").ToList().First().Value;
            else 
                return null;
        }

        internal string CodIndiceMap(string indexador)
        {
            switch (indexador)
            {
                case "IGP-M":
                    return "IGPM";
                case "IPCA":
                    return "IPCA";
                case "INCC":
                    return "INCC";
                case "IGP-DI":
                    return "IGP";
                case "CDI":
                    return "CDIE";
                case "Pré":
                    return "PRE";
                case "TR":
                    return "TR";
                case "USD":
                    return "PTXV";
                default:
                    return null;
            }
        }

        internal string CodIndicePrecificacaoMap(string indexador)
        {
            switch (indexador)
            {
                case "IGP-M":
                    return "NIGPM";
                case "IPCA":
                    return "NIPCA";
                case "INCC":
                    return "NINCC";
                case "IGP-DI":
                    return "NIGP";
                case "CDI":
                    return "CDIEAN";
                case "Pré":
                    return "PRE";
                case "TR":
                    return "TR";
                case "USD":
                    return "PTXV";
                default:
                    return null;
            }
        }

        internal bool NaoConsideraDeflacaoMap(string projecao)
        {
            if (projecao == "Último Índice Positivo")
                return true;
            else
                return false;
        }

        internal string TipoUnidadeTempoMap(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador, int valorReferenciaComp)
        {
            //[{ "key": "Final","value": "Final"},{ "key": "Dias","value": "Dias"},{ "key": "Meses","value": "Meses"},{ "key": "Anos","value": "Anos"}]
            var dominiosUnidadesTempo = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[20].ValueJson);
            if (unidadeIndexador == "Diária" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Dias").ToList().First().Value;
            else if (periodicidadeCorrecao == "Diária" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Dias").ToList().First().Value;
            else if (periodicidadeCorrecao == "Mensal" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Meses").ToList().First().Value;
            else if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Anos").ToList().First().Value;
            else if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Anos").ToList().First().Value;
            else
                return null;
        }
        internal string TipoUnidadeTempoInflacaoMap(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador, int valorReferenciaComp)
        {
            //[{ "key": "Final","value": "Final"},{ "key": "Dias","value": "Dias"},{ "key": "Meses","value": "Meses"},{ "key": "Anos","value": "Anos"}]
            var dominiosUnidadesTempo = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[20].ValueJson);
            if (unidadeIndexador == "Diária" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Dias").ToList().First().Value;
            else if (periodicidadeCorrecao == "Diária" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Meses").ToList().First().Value;
            else if (periodicidadeCorrecao == "Mensal" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Meses").ToList().First().Value;
            else if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Meses").ToList().First().Value;
            else if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < valorReferenciaComp)
                return dominiosUnidadesTempo.Where(d => d.Key == "Meses").ToList().First().Value;
            else
                return null;
        }
        internal int PeriodoDefasagemMap(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador, int valorReferenciaComp)
        {
            //[{ "key": "Final","value": "Final"},{ "key": "Dias","value": "Dias"},{ "key": "Meses","value": "Meses"},{ "key": "Anos","value": "Anos"}]
            var dominiosUnidadesTempo = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[20].ValueJson);
            if (unidadeIndexador == "Diária" && defasagemIndexador >= valorReferenciaComp)
                return 0;
            else if (periodicidadeCorrecao == "Diária" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return 0;
            else
                return 1;
        }
        internal int PeriodoDefasagemInflacaoMap(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador)
        {
            if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < 1)
                return -1;
            else
                return (defasagemIndexador * (-1)) - 1;
        }

        internal int PeriodoDefasagemCDIInflacaoMap(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador)
        {
            if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < 2)
                return -1;
            else
                return (defasagemIndexador * (-1));
        }

        internal int? MesDeReferenciaParaAtualizacaoMap(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador, int mesReferenciaIndexador, int valorReferenciaComp)
        {
            if (unidadeIndexador == "Diária" && defasagemIndexador >= valorReferenciaComp)
                return 0;
            else if (periodicidadeCorrecao == "Diária" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return mesReferenciaIndexador;
            else if (periodicidadeCorrecao == "Mensal" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return mesReferenciaIndexador;
            else if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador >= valorReferenciaComp)
                return mesReferenciaIndexador;
            else if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < valorReferenciaComp)
                return mesReferenciaIndexador;
            else
                return null;
        }
        //[{"key": "Normal","value": "0"},{"key": "Variável","value": "1"}]
        internal int TipoAniversarioMap(string tipoAniversario)
        {
            var dominiosTipoAniversario = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[11].ValueJson);
            if (tipoAniversario == "Variável")
                return dominiosTipoAniversario.Where(d => d.Key == "Variável").ToList().First().Value;
            else
                return dominiosTipoAniversario.Where(d => d.Key == "Normal").ToList().First().Value;
        }

        internal int? DiaAniversarioMap(string tipoAniversario, DateTime dataInicioRentabilidade, DateTime dataVencimento, int diaReferenciaIndexador)
        {
            switch (tipoAniversario)
            {
                case "Fixo":
                    return diaReferenciaIndexador;
                case "Início":
                    return diaReferenciaIndexador;//dataInicioRentabilidade.Day;
                case "Vencimento":
                    return diaReferenciaIndexador;//dataVencimento.Day;
                case "Variável":
                    return null;
                default:
                    return 0;
            }
        }

        internal List<DateTime> ListaAniversarios(string tipoAniversario, FIOnCashFlowContract[] fluxos)
        {
            List<DateTime> ListaAniversarios = new List<DateTime>();
            var dominiosCodTipoFluxo = JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(_dominios[26].ValueJson);
            if (tipoAniversario == "Variável")
            {
                foreach (FIOnCashFlowContract fluxo in fluxos)
                {
                    if(fluxo.CodTipoFluxo == dominiosCodTipoFluxo.Where(d => d.Key == "Juros").ToList().First().Value || 
                        fluxo.CodTipoFluxo == dominiosCodTipoFluxo.Where(d => d.Key == "Incorporação").ToList().First().Value)
                        ListaAniversarios.Add((DateTime)fluxo.DataEventoReal);
                }
            }
            return ListaAniversarios;
        }

        internal string MoedaProjecaoMap(string indexador, string projecao, int defasagemIndexador)
        {
            if (indexador == "IGP-M" && projecao == "Projeção Anbima")
                return "IGPM_MESA";
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return "NIGPM";
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return "NIGPM";
            else if (indexador == "IPCA" && projecao == "Projeção Anbima")
                return "IPCA_MESA";
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return "NIPCA";
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return "NIPCA";
            else if (indexador == "INCC" && (projecao == "Último Índice" || projecao == "Último Índice Positivo"))
                return "NINCC";
            else if (indexador == "IGP-DI" && projecao == "Projeção Anbima")
                return "IGP_MESA";
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return "NIGP";
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return "NIGP";
            else if (indexador == "CDI")
                return "CDIEAN";
            else if (indexador == "Pré")
                return "PRE";
            else if (indexador == "TR")
                return "TR";
            else if (indexador == "USD")
                return "PTXV";
            else
                return null;
        }
        //[{"key": "Sem projeção","value": "nao"},{"key": "Última Inflação","value": "ultima"},{"key": "Inflação Andima","value": "andima"},{"key": "Inflação BMF","value": "bmf"},{"key": "Inflação FGV","value": "fgv"},{"key": "Inflação Interna","value": "interna"},{"key": "Curvas","value": "curvas"},{"key": "Último índice divulgado","value": "ultimoIndDivulgado"}]
        internal string TipoProjecaoMap(string indexador, string projecao, int defasagemIndexador)
        {
            var dominiosTipoProjecao = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[16].ValueJson);
            if (indexador == "IGP-M" && projecao == "Projeção Anbima")
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IPCA" && projecao == "Projeção Anbima")
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "INCC" && (projecao == "Último Índice" || projecao == "Último Índice Positivo"))
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IGP-DI" && projecao == "Projeção Anbima")
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "CDI")
                return dominiosTipoProjecao.Where(d => d.Key == "Curvas").ToList().First().Value;
            else if (indexador == "Pré")
                return dominiosTipoProjecao.Where(d => d.Key == "Sem projeção").ToList().First().Value;
            else if (indexador == "TR")
                return dominiosTipoProjecao.Where(d => d.Key == "Sem projeção").ToList().First().Value;
            else if (indexador == "USD")
                return dominiosTipoProjecao.Where(d => d.Key == "Curvas").ToList().First().Value;
            else
                return null;
        }

        internal bool ProjetarIndiceMap(string indexador, string projecao, int defasagemIndexador)
        {
            if (indexador == "IGP-M" && projecao == "Projeção Anbima")
                return true;
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return true;
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return true;
            else if (indexador == "IPCA" && projecao == "Projeção Anbima")
                return true;
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return true;
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return true;
            else if (indexador == "INCC" && (projecao == "Último Índice" || projecao == "Último Índice Positivo"))
                return true;
            else if (indexador == "IGP-DI" && projecao == "Projeção Anbima")
                return true;
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return true;
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return true;
            else
                return false;
        }

        internal string MoedaProjecaoComplMap(string indexador, string projecao, int defasagemIndexador)
        {
            if (indexador == "IGP-M" && projecao == "Projeção Anbima")
                return "IGPM_MESA";
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return "IGPM_MESA";
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return "NIGPM";
            else if (indexador == "IPCA" && projecao == "Projeção Anbima")
                return "IPCA_MESA";
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return "IPCA_MESA";
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return "NIPCA";
            else if (indexador == "INCC" && (projecao == "Último Índice" || projecao == "Último Índice Positivo"))
                return "NINCC";
            else if (indexador == "IGP-DI" && projecao == "Projeção Anbima")
                return "IGP_MESA";
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return "IGP_MESA";
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return "NIGP";
            else if (indexador == "CDI")
                return "CDIEAN";
            else if (indexador == "Pré")
                return "PRE";
            else if (indexador == "TR")
                return "TR";
            else if (indexador == "USD")
                return "PTXV";
            else
                return null;
        }

        //[{"key": "Sem projeção","value": "nao"},{"key": "Última Inflação","value": "ultima"},{"key": "Inflação Andima","value": "andima"},{"key": "Inflação BMF","value": "bmf"},{"key": "Inflação FGV","value": "fgv"},{"key": "Inflação Interna","value": "interna"},{"key": "Curvas","value": "curvas"},{"key": "Último índice divulgado","value": "ultimoIndDivulgado"}]
        internal string TipoProjecaoComplMap(string indexador, string projecao, int defasagemIndexador)
        {
            var dominiosTipoProjecao = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_dominios[16].ValueJson);
            if (indexador == "IGP-M" && projecao == "Projeção Anbima")
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IGP-M" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IPCA" && projecao == "Projeção Anbima")
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IPCA" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "INCC" && (projecao == "Último Índice" || projecao == "Último Índice Positivo"))
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "IGP-DI" && projecao == "Projeção Anbima")
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador < 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Inflação Interna").ToList().First().Value;
            else if (indexador == "IGP-DI" && (projecao == "Último Índice" || projecao == "Último Índice Positivo") && defasagemIndexador >= 1)
                return dominiosTipoProjecao.Where(d => d.Key == "Última Inflação").ToList().First().Value;
            else if (indexador == "CDI")
                return dominiosTipoProjecao.Where(d => d.Key == "Curvas").ToList().First().Value;
            else if (indexador == "Pré")
                return dominiosTipoProjecao.Where(d => d.Key == "Sem projeção").ToList().First().Value;
            else if (indexador == "TR")
                return dominiosTipoProjecao.Where(d => d.Key == "Sem projeção").ToList().First().Value;
            else if (indexador == "USD")
                return dominiosTipoProjecao.Where(d => d.Key == "Curvas").ToList().First().Value;
            else
                return null;
        }

        internal bool ExisteDefasagem(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador)
        {
            if (unidadeIndexador == "Diária" && defasagemIndexador >= 1)
                return true;
            if (periodicidadeCorrecao == "Diária" && unidadeIndexador == "Acumulada" && defasagemIndexador >= 1)
                return true;
            if (periodicidadeCorrecao == "Mensal" && unidadeIndexador == "Acumulada" && defasagemIndexador >= 1)
                return true;
            if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador >= 1)
                return true;
            if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < 1)
                return true;
            else
                return false;
        }

        internal bool ExisteDefasagemCDI(string periodicidadeCorrecao, string unidadeIndexador, int defasagemIndexador)
        {
            if (unidadeIndexador == "Diária" && defasagemIndexador >= 2)
                return true;
            if (periodicidadeCorrecao == "Diária" && unidadeIndexador == "Acumulada" && defasagemIndexador >= 2)
                return true;
            if (periodicidadeCorrecao == "Mensal" && unidadeIndexador == "Acumulada" && defasagemIndexador >= 2)
                return true;
            if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador >= 2)
                return true;
            if (periodicidadeCorrecao == "Anual" && unidadeIndexador == "Acumulada" && defasagemIndexador < 2)
                return true;
            else
                return false;
        }

        internal string TipoEventoMap(string tipoEvento)
        {
            if (tipoEvento == "Juros")
                return "Juros";
            if (tipoEvento == "Amortização")
                return "Amortizacao";
            if (tipoEvento == "Amortização Extra")
                return "AmortizacaoExtraordi";
            if (tipoEvento == "Vencimento")
                return "Vencimento";
            if (tipoEvento == "Prêmio")
                return "Premio";
            return null;
        }

        internal List<PuDeEventosLuz> ConverteEventoLuzToMdp(List<DadosPreco> relatorioPu, DateTime data)
        {
            List<PuDeEventosLuz> relatorioPuDeEventos = new List<PuDeEventosLuz>();
            foreach (DadosPreco preco in relatorioPu)
            {
                if (preco.StatusPgto == "Programado")
                {
                    if (preco.PagamentoJuros != 0)
                        relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Juros", PuEvento = preco.PagamentoJuros, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    if (preco.PagamentoAmortizacao != 0)
                        relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Amortização", PuEvento = preco.PagamentoAmortizacao, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    if (preco.PagamentoAmex != 0)
                        relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Amortização Extra", PuEvento = preco.PagamentoAmex, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    if (preco.PagamentoVencimento != 0)
                        relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Vencimento", PuEvento = preco.PagamentoVencimento, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    if (preco.PagamentoPremio != 0)
                        relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Prêmio", PuEvento = preco.PagamentoPremio, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                }
                else if (preco.StatusPgto == "Confirmado" || preco.StatusPgto == "Não há")
                {
                    relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Juros", PuEvento = preco.PagamentoJuros, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Amortização", PuEvento = preco.PagamentoAmortizacao, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Amortização Extra", PuEvento = preco.PagamentoAmex, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Vencimento", PuEvento = preco.PagamentoVencimento, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                    relatorioPuDeEventos.Add(new PuDeEventosLuz() { CodigoSNA = preco.CodigoSNA, DataEvento = preco.Data, TipoEvento = "Prêmio", PuEvento = preco.PagamentoPremio, StatusPagamento = preco.StatusPgto, AlteracaoStatusPagamento = preco.DataAttStatusPgto });
                }
            }
            return relatorioPuDeEventos;
        }

        internal string AusenciaDadosMsg(FIOnInstrumentContract cadastroBdLuz, List<DadosCaracteristicos> message)
        {
            var cadastro = Utils.messageToCadastro(message, cadastroBdLuz.CodCetip);
            var listaDadosFaltantes = new List<string>();

            if (cadastroBdLuz.Fluxos.Length == 0)
                listaDadosFaltantes.Add("Fluxos");
            if (cadastro.Tipo == null)
                listaDadosFaltantes.Add("Tipo");
            if (cadastro.Clearing == null)
                listaDadosFaltantes.Add("Clearing");
            if (cadastro.CriterioCalculoJuros == null)
                listaDadosFaltantes.Add("Critério Juros");
            if (cadastro.Indexador == null)
                listaDadosFaltantes.Add("Indexador");
            if (cadastroBdLuz.CgeEmissor == null)
                listaDadosFaltantes.Add("CNPJ Emissor invalido");
            else if (cadastro.CnpjEmissor == null)
                listaDadosFaltantes.Add("CNPJ Emissor");

            string errorMessage;

            if (listaDadosFaltantes.Count == 0)
                errorMessage = $"Erro de cadastro do ativo {cadastro.CodigoSNA} no MDP.";
            else
            {
                var dadosFaltantes = string.Join(", ", listaDadosFaltantes);
                if (cadastroBdLuz.CodAtivo == 0)
                {
                    errorMessage = $"Erro ao tentar realizar novo cadastro do ativo {cadastro.CodigoSNA}. Dado(s) inconsistente(s): {dadosFaltantes}.";
                }
                else
                {
                    errorMessage = $"Erro de cadastro do ativo {cadastro.CodigoSNA} no MDP. Dado(s) inconsistente(s): {dadosFaltantes}.";
                }
            }

            return errorMessage;
        }
    }
}