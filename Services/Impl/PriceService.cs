using EDM.Infohub.Integration.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Price;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;


namespace EDM.Infohub.Integration.Services.Impl
{
    public class PriceService : IPriceService
    {
        private ILogger<EDMCommonService> _logger;
        private IConfiguration _configuration;

        public string COMMON_SERVICE_URI { get; }

        private ISecureGateway _secureGateway;

        public PriceService(IConfiguration configuration, ILogger<EDMCommonService> logger, ISecureGateway secureGateway)
        {
            _logger = logger;
            _configuration = configuration;
            COMMON_SERVICE_URI = _configuration["EDMGetPriceService"];
            _secureGateway = secureGateway;
        }

        public List<Response> GetPrice(List<Request> request)
        {
            var soapClient = new GetPriceClient(GetPriceClient.EndpointConfiguration.GetPriceServiceSOAP, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));

            using (new OperationContextScope(soapClient.InnerChannel))
            {
                //_Security.SetSOAPHeaders(OperationContext.Current);
                try
                {
                    var response = soapClient.GetPrice(request.ToArray());
                    if (response.Length > 0)
                    {
                        foreach (Response t in response)
                        {
                            _logger.LogError(JsonConvert.SerializeObject(t));
                        }
                    }
                    return response.ToList<Response>();
                }
                catch
                {
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

    }


}
