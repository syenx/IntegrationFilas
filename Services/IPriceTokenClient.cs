using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services
{
    public interface IPriceTokenClient
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
    }
}
