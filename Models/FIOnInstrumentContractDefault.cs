using EDMFixedIncomeOnService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    public class FIOnInstrumentContractDefault
    {
        public FIOnInstrumentContract papelPadrao()
        {
            return new FIOnInstrumentContract()
            {
                BaseAnualAlternativo = 360,
                Bullet = false,
                CodIndice = null,
                Coobrigacao = false,
                CurrencyRateFrequency = null,
                CurrencyRateType = null,
                Cusip = "",
                ModeloPrazoAlternativo = 1,
                ModeloTaxaAlternativo = 4,
                Sedol = "",
                TipoInflacaoVariavel = 0,
                TipoRisco = "",
            };
        }
    }
}
