using Price;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services
{
    public interface IPriceService
    {
        public List<Response> GetPrice(List<Request> request);
    }
}
