using AutoMapper;
using EDM.Infohub.Integration.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services.Impl
{
    public class NifiService : INifiService
    {
        private readonly INifiClient _client;
        private readonly IPriceTokenClient _tokenClient;
        private readonly IConfiguration _config;
        private readonly ILogger<NifiService> _logger;
        private readonly IMapper _mapper;
        private IMemoryCache _cacheToken;

        public NifiService(IConfiguration config, ILogger<NifiService> logger, INifiClient client, IMapper mapper, IMemoryCache cache, IPriceTokenClient tokenClient)
        {
            _config = config;
            _client = client;
            _logger = logger;
            _mapper = mapper;
            _cacheToken = cache;
            _tokenClient = tokenClient;
        }

        public void SalvarPrecosLote(PriceHistoricalData[] priceList)
        {
            try
            {
                var token = GetToken();
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(_mapper.Map<List<PriceNifi>>(priceList)));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                content.Headers.Add("priceType", _config["NifiIntegration:priceType"]);
                
                var response = _client.PostAsync(null, content, token);
                response.Result.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }
        internal string GetToken()
        {
            try
            {
                string token;
                if(_cacheToken.TryGetValue("token", out token))
                {
                    return _cacheToken.Get<string>("token");
                }

                _logger.LogInformation("Gerando novo token para integracao de precos com nifi.");

                var bodyAuthenticate = new Dictionary<string, object>
                {
                    { "username", _config["NifiToken:user"] },
                    { "password", _config["NifiToken:password"] }
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(bodyAuthenticate));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = _tokenClient.PostAsync("v1/users/authenticate/", content).Result;
                var responseBody = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
                token = responseBody["token"].ToString();

                _cacheToken.Set("token", token, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(2)));

                return token;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw e;
            }
        }
    }
}