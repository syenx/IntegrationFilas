using Newtonsoft.Json;
using System;

namespace EDM.Infohub.Integration.Models
{
    public class DadosPreco
    {
        [JsonProperty(PropertyName = "codigoSNA")]
        public string CodigoSNA { get; set; }
        [JsonProperty(PropertyName = "tipo")]
        public string Tipo { get; set; }
        [JsonProperty(PropertyName = "indexador")]
        public string Indexador { get; set; }
        [JsonProperty(PropertyName = "taxaPos")]
        public decimal TaxaPos { get; set; }
        [JsonProperty(PropertyName = "taxaPre")]
        public decimal TaxaPre { get; set; }
        [JsonProperty(PropertyName = "dataEvento")]
        public DateTime Data { get; set; }
        [JsonProperty(PropertyName = "valorNominalBase")]
        public decimal PuNominalSemProjecao { get; set; }
        [JsonProperty(PropertyName = "valorNominalAtualizado")]
        public decimal PuNominal { get; set; }
        [JsonProperty(PropertyName = "fatorCorrecao")]
        public decimal FatorCorrecao { get; set; }
        [JsonProperty(PropertyName = "fatorJuros")]
        public decimal FatorJuros { get; set; }
        [JsonProperty(PropertyName = "puAbertura")]
        public decimal PuParAbertura { get; set; }
        [JsonProperty(PropertyName = "pagamentos")]
        public decimal Pagamentos { get; set; }
        [JsonProperty(PropertyName = "puFechamento")]
        public decimal PuPar { get; set; }
        [JsonProperty(PropertyName = "principal")]
        public decimal PrincipalFechamento { get; set; }
        [JsonProperty(PropertyName = "inflacao")]
        public decimal Inflacao { get; set; }
        [JsonProperty(PropertyName = "juros")]
        public decimal Juros { get; set; }
        [JsonProperty(PropertyName = "incorporado")]
        public decimal Incorporado { get; set; }
        [JsonProperty(PropertyName = "incorporar")]
        public decimal Incorporar { get; set; }
        [JsonProperty(PropertyName = "pagamentoJuros")]
        public decimal PagamentoJuros { get; set; }
        [JsonProperty(PropertyName = "pagamentoAmortizacao")]
        public decimal PagamentoAmortizacao { get; set; }
        [JsonProperty(PropertyName = "pagamentoAmex")]
        public decimal PagamentoAmex { get; set; }
        [JsonProperty(PropertyName = "pagamentoVencimento")]
        public decimal PagamentoVencimento { get; set; }
        [JsonProperty(PropertyName = "pagamentoPremio")]
        public decimal PagamentoPremio { get; set; }
        [JsonProperty(PropertyName = "porcentualAmortizado")]
        public decimal PorcentualAmortizado { get; set; }
        [JsonProperty(PropertyName = "porcentualJurosIncorporado")]
        public decimal PorcentualJurosIncorporado { get; set; }
        [JsonProperty(PropertyName = "statusPagamento")]
        public string StatusPgto { get; set; }
        [JsonProperty(PropertyName = "alteracaoStatusPagamento")]
        public DateTime DataAttStatusPgto { get; set; }
    }
}
