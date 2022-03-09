using System;

namespace EDM.Infohub.Integration.Models
{
    public class PriceHistoricalData
    {
        public int CodAtivo { get; set; }
        public DateTime Data { get; set; }
        public string CodPraca { get; set; }
        public int CodFeeder { get; set; }
        public int CodCampo { get; set; }
        public double Valor { get; set; }
        public double FatorAjuste { get; set; }
        public bool Previsao { get; set; }
        public bool IsRebook { get; set; }
    }
}
