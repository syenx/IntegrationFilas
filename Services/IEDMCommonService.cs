using EDM.Infohub.Integration.Models;
using EDMCommonService;

namespace EDM.Infohub.Integration.Services
{
    public interface IEDMCommonService
    {
        void SalvarPrecosLote(PriceHistoricalData[] priceList);

        int CodCetipToCodAtivo(string codCetip);
        void InserirPendencia(PendenciaAtivoDataContract pendencia, ValorPendenciaAtivoDataContract valorPendencia);
    }
}
