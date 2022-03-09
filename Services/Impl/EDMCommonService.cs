using EDM.Infohub.Integration.Models;
using EDM.Infohub.Integration.Security;
using EDM.Infohub.Integration.Services;
using EDMCommonService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.ServiceModel;
using static EDMCommonService.EDMCommonServiceContractClient;

namespace EDM.Infohub.Integration
{
    public class EDMCommonService : IEDMCommonService
    {
        private readonly ISecureGateway _secureGateway;
        private EDMCommonServiceContractClient soapClient;
        private readonly ILogger<EDMCommonService> _logger;
        private readonly IConfiguration _configuration;

        public string COMMON_SERVICE_URI { get; }

        //private readonly ISecurity _Security;
        public EDMCommonService(IConfiguration configuration, ILogger<EDMCommonService> logger, ISecureGateway secureGateway)
        {
            _logger = logger;
            _configuration = configuration;
            COMMON_SERVICE_URI = _configuration["EDMCommonService"];
            _secureGateway = secureGateway;
        }
        public void SalvarPrecosLote(PriceHistoricalData[] priceList)
        {
            var soapClient = new EDMCommonServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMCommonServiceContract, COMMON_SERVICE_URI);
            //soapClient.InnerChannel.OperationTimeout = new TimeSpan(0, 3, 0);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));

            using (new OperationContextScope(soapClient.InnerChannel))
            {
                //_Security.SetSOAPHeaders(OperationContext.Current);
                try
                {
                    var teste = soapClient.SalvarPrecosLoteAsync(ConvertEntityToDataContractList(priceList)).Result;
                    if (teste.Length > 0)
                    {
                        foreach (PriceDataContract t in teste)
                        {
                            _logger.LogError(JsonConvert.SerializeObject(t));
                        }
                    }
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

        private PriceDataContract[] ConvertEntityToDataContractList(PriceHistoricalData[] priceList)
        {
            return priceList == null ? null : priceList.ToList().Select(ConvertEntityToDataContract).ToArray();
        }

        private PriceDataContract ConvertEntityToDataContract(PriceHistoricalData price)
        {
            return new PriceDataContract
            {
                CodAtivo = price.CodAtivo,
                CodPraca = price.CodPraca,
                CodFeeder = Convert.ToByte(price.CodFeeder),
                CodCampo = price.CodCampo,
                Data = price.Data,
                Preco = price.Valor,
                Previsao = price.Previsao,
                FatorAjuste = price.FatorAjuste,
                IsRebook = price.IsRebook
            };
        }

        public int CodCetipToCodAtivo(string codCetip)
        {
            //OnboardingEDMServiceClient soapClient = _provider.GetRequiredService<OnboardingEDMServiceClient>();
            var soapClient = new EDMCommonServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMCommonServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));

            using (new OperationContextScope(soapClient.InnerChannel))
            {
                //_Security.SetSOAPHeaders(OperationContext.Current);
                try
                {
                    var fiOnInstrument = soapClient.GetFIOnIntrumentByCodCetip(codCetip);
                    if (fiOnInstrument == null)
                    {
                        throw new NullReferenceException($"Nao foi encontrado o CodAtivo de {codCetip}");
                    }
                    return fiOnInstrument.CodAtivo ?? 0;
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

        public void InserirPendencia(PendenciaAtivoDataContract pendencia, ValorPendenciaAtivoDataContract valorPendencia)
        {
            //OnboardingEDMServiceClient soapClient = _provider.GetRequiredService<OnboardingEDMServiceClient>();
            var soapClient = new EDMCommonServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMCommonServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));

            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    soapClient.InserirPendencia(pendencia, valorPendencia);
                }
                catch
                {
                    _logger.LogError("Nao foi possivel adicionar pendencia ao papel");
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
