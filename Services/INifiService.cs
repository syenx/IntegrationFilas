using EDM.Infohub.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services
{
    public interface INifiService
    {
        void SalvarPrecosLote(PriceHistoricalData[] priceList);
    }
}
