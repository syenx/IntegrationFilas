using Newtonsoft.Json;
using System;

namespace EDM.Infohub.BPO.RabbitMQ
{
    public class DadosCaracteristicos
    {
        [JsonProperty(PropertyName = "codigoSNA")]
        public string CodigoSNA { get; set; }
        [JsonProperty(PropertyName = "tipo")]
        public string Tipo { get; set; }
        [JsonProperty(PropertyName = "isin")]
        public string Isin { get; set; }
        [JsonProperty(PropertyName = "emissor")]
        public string Emissor { get; set; }
        [JsonProperty(PropertyName = "cnpjEmissor")]
        public string CnpjEmissor { get; set; }
        [JsonProperty(PropertyName = "dataEmissao")]
        public DateTime DataEmissao { get; set; }
        [JsonProperty(PropertyName = "dataInicioRentabilidade")]
        public DateTime DataInicioRentabilidade { get; set; }
        [JsonProperty(PropertyName = "dataVencimento")]
        public DateTime DataVencimento { get; set; }
        [JsonProperty(PropertyName = "valorNominalEmissao")]
        public decimal ValorNominalEmissao { get; set; }
        [JsonProperty(PropertyName = "instrucaoCVM")]
        public string InstrucaoCVM { get; set; }
        [JsonProperty(PropertyName = "clearing")]
        public string Clearing { get; set; }
        [JsonProperty(PropertyName = "agenteFiduciario")]
        public string AgenteFiduciario { get; set; }
        [JsonProperty(PropertyName = "possibilidadeResgateAntecipado")]
        public bool PossibilidadeResgateAntecipado { get; set; }
        [JsonProperty(PropertyName = "conversivelAcao")]
        public bool ConversivelAcao { get; set; }
        [JsonProperty(PropertyName = "debentureIncentivada")]
        public bool DebentureIncentivada { get; set; }
        [JsonProperty(PropertyName = "criterioCalculoIndexador")]
        public string CriterioCalculoIndexador { get; set; }
        [JsonProperty(PropertyName = "criterioCalculoJuros")]
        public string CriterioCalculoJuros { get; set; }
        [JsonProperty(PropertyName = "indexador")]
        public string Indexador { get; set; }
        [JsonProperty(PropertyName = "taxaPre")]
        public decimal TaxaPre { get; set; }
        [JsonProperty(PropertyName = "taxaPos")]
        public decimal TaxaPos { get; set; }
        [JsonProperty(PropertyName = "projecao")]
        public string Projecao { get; set; }
        [JsonProperty(PropertyName = "tipoAmortizacao")]
        public string Amortizacao { get; set; }
        [JsonProperty(PropertyName = "periodicidadeCorrecao")]
        public string PeriodicidadeCorrecao { get; set; }
        [JsonProperty(PropertyName = "unidadeIndexador")]
        public string UnidadeIndexador { get; set; }
        [JsonProperty(PropertyName = "defasagemIndexador")]
        public int DefasagemIndexador { get; set; }
        [JsonProperty(PropertyName = "diaReferenciaIndexador")]
        public int DiaReferenciaIndexador { get; set; }
        [JsonProperty(PropertyName = "mesReferenciaIndexador")]
        public int MesReferenciaIndexador { get; set; }
        [JsonProperty(PropertyName = "devedor")]
        public string Devedor { get; set; }
        [JsonProperty(PropertyName = "tipoRegime")]
        public string TipoRegime { get; set; }
        [JsonProperty(PropertyName = "tipoAniversario")]
        public string TipoAniversario { get; set; }
        [JsonProperty(PropertyName = "consideraCorrecaoNegativa")]
        public bool ConsideraCorrecaoNegativa { get; set; }
        [JsonProperty(PropertyName = "dataUltimaAlteracao")]
        public DateTime DataUltimaAlteracao { get; set; }
        [JsonProperty(PropertyName = "cnpjDevedor")]
        public string CnpjDevedor { get; set; }
        [JsonProperty(PropertyName = "cnpjAgenteFiduciario")]
        public string CnpjAgenteFiduciario { get; set; }
    }
}