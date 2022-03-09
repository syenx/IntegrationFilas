using System;
using System.Collections.Generic;

namespace EDM.Infohub.Integration.Models
{
    public class PricePricing
    {
        public string? CodigoSNA;
        public int CodAtivo;
        public DateTime Data;
        public decimal? FatorCorrecao;
        public decimal? FatorJuros;
        public decimal? Incorporado;
        public decimal? Incorporar;
        public decimal? Inflacao;
        public decimal? Juros;
        public decimal? PrincipalAbertura;
        public decimal? PrincipalFechamento;
        public decimal? PuNominal;
        public decimal? PuNominalAbertura;
        public decimal? PuNominalEmissorAbertura;
        public decimal? PuNominalEmissorFechamento;
        public decimal? PuNominalSemProjecao;
        public decimal? PuPar;
        public decimal? PuParAbertura;
        public decimal? PuParComProjecao;
        public decimal? PuParEmissorAbertura;
        public decimal? PuParEmissorFechamento;
        public decimal? PuParSemProjecao;


        public List<PriceHistoricalData> Map(DateTime dtReferencia, string CodPraca, int CodFeeder)
        {
            List<PriceHistoricalData> priceList = new List<PriceHistoricalData>();

            //Incorporado
            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_Incorporado, Valor = Convert.ToDouble(this.Incorporado ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });
            //A Incorporar
            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_A_Incorporar, Valor = Convert.ToDouble(this.Incorporar ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });
            //Juros
            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_Juros, Valor = Convert.ToDouble(this.Juros ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });
            //Inflaçao
            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_Correção, Valor = Convert.ToDouble(this.Inflacao ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });
            //PrincipalFechamento
            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_Principal_Fechamento, Valor = Convert.ToDouble(this.PrincipalFechamento ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });
            //PuPar
            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_Fechamento, Valor = Convert.ToDouble(this.PuPar ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });
            //

            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Nominal_Sem_Projecao, Valor = Convert.ToDouble(this.PuNominalSemProjecao ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });

            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Nominal, Valor = Convert.ToDouble(this.PuNominal ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });

            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.Fator_Juros, Valor = Convert.ToDouble(this.FatorJuros ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });

            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.Fator_Correcao, Valor = Convert.ToDouble(this.FatorCorrecao ?? 0), FatorAjuste = 1, Previsao = false,
               IsRebook = false });

            priceList.Add(new PriceHistoricalData { CodAtivo = CodAtivo, Data = dtReferencia, CodPraca = CodPraca, CodFeeder = CodFeeder, CodCampo = (int)CodCampoEnum.PU_Curva_Papel_Abertura, Valor = Convert.ToDouble(this.PuParAbertura ?? 0), FatorAjuste = 1, Previsao = false, IsRebook = false });

            return priceList;
        }
    }
}
