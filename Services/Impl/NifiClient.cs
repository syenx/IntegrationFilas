using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services.Impl
{
    public class NifiClient : INifiClient
    {
        private readonly HttpClient client;
        private readonly IConfiguration _config;

        public NifiClient(IConfiguration config)
        {
            _config = config;
            client = new HttpClient();
            client.BaseAddress = new Uri(_config["NifiIntegration:endpoint"]);
        }
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, string token)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client.PostAsync(requestUri, content);
        }
    }
}
