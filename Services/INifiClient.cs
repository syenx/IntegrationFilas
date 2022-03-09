using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services
{
    public interface INifiClient
    {
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, string token);
    }
}
