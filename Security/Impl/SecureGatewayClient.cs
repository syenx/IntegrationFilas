using EDM.Infohub.Integration.Security;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Extensions.Logging;
using System.Threading;
using SecureGatewayService;
using Microsoft.Extensions.Caching.Memory;

namespace EDM.Infohub.Integration.Services
{
    internal class SecureGatewayClient : ISecureGateway
    {
        private readonly SecGtwNoCertContractClient _secClient;
        private readonly ILogger<SecureGatewayClient> _logger;
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;

        public SecureGatewayClient(IConfiguration configuration, IMemoryCache cache, ILogger<SecureGatewayClient> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            //_secClient = new SecGtwNoCertContractClient(configuration);
            _secClient = new SecGtwNoCertContractClient(configuration);
            GetToken();
        }
        public string GetToken()
        {
            var token = "";

            try
            {
                if (!_cache.TryGetValue("SecureGateway_Token", out token))
                {

                    var key = _configuration.GetSection("SecureGateway")["AuthenticationKey"].ToString();
                    //Console.WriteLine("criando cache");
                    // Key not in cache, so get data.
                    var response = _secClient.AuthenticateByKeyAsync(key).Result;
                    if(response.Token == null)
                    {
                        throw new Exception($"Token Vazio: {response.MessageResult}");
                    }
                    token = response.Token;
                    //Console.WriteLine($"Message Result:{response.MessageResult}");

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1));
                        // Keep in cache for this time, reset time if accessed.
                        //.SetSlidingExpiration(TimeSpan.FromDays(1));
                    //_logger.LogInformation($"Solicitando token do Secure Gateway: {token}");
                    Console.WriteLine($"Solicitando token do Secure Gateway: {token}");

                    // Save data in cache.
                    _cache.Set("SecureGateway_Token", token, cacheEntryOptions);
                }
                else
                {
                    token = _cache.Get<string>("SecureGateway_Token");                    
                    //Console.WriteLine($"Pegando do cache: {token}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Token não gerado: {e}");
                throw e;
            }

            //Console.WriteLine(token);
            return token;

        }

        public bool ValidateToke(string token)
        {
            throw new NotImplementedException();
        }
    }
}

