using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Transactions;
using EDM.Infohub.Integration.Exceptions;
using EDM.Infohub.Integration.Security;
using EDMFixedIncomeOnService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static EDMFixedIncomeOnService.EDMFixedIncomeOnServiceContractClient;

namespace EDM.Infohub.Integration.Services.Impl
{
    public class EDMFixedIncomeOn : IFixedIncomeOn
    {

        private readonly ISecureGateway _secureGateway;
//        private EDMFixedIncomeOnServiceContractClient soapClient;
        private readonly ILogger<EDMCommonService> _logger;
        private readonly IConfiguration _configuration;
        public string COMMON_SERVICE_URI { get; }

        public EDMFixedIncomeOn(IConfiguration configuration, ILogger<EDMCommonService> logger, ISecureGateway secureGateway)
        {
            _logger = logger;
            _configuration = configuration;
            COMMON_SERVICE_URI = _configuration["EDMFixedIncomeOnService"];
            _secureGateway = secureGateway;
        }


        public FIOnInstrumentContract[] GetFIOnInstrumentBySNA(string sna)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            //soapClient.InnerChannel.OperationTimeout = new TimeSpan(0, 3, 0);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));

            using (new OperationContextScope(soapClient.InnerChannel))
            {
                //_Security.SetSOAPHeaders(OperationContext.Current);
                try
                {
                    var instrumento = soapClient.GetFIOnInstrumentBySNA(sna);
                    //var teste = soapClient.SalvarPrecosLoteAsync(ConvertEntityToDataContractList(priceList)).Result;
                    //if (teste.Length > 0)
                    //{
                    //    foreach (PriceDataContract t in teste)
                    //    {
                    //        _logger.LogError(JsonConvert.SerializeObject(t));
                    //    }
                    //}
                    return instrumento;
                }catch(FaultException e)
                {
                    _logger.LogInformation($"O ativo {sna} nao existe em nossa base");
                    return new List<FIOnInstrumentContract>().ToArray();
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel recuperar o ativo {sna} do Global");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public int InsertUpdateInstrument(FIOnInstrumentContract instrument)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    return soapClient.InsertUpdateInstrument(instrument);
                }
                catch(FaultException e)
                {
                    _logger.LogError($"Nao foi possivel dar upsert no ativo {instrument.CodCetip} por meio do EDM Services");
                    if (e.Message.Contains("was deadlocked on lock resources with another process and has been chosen as the deadlock victim"))
                        throw new TransactionException(e.Message);
                    else
                        throw new CamposObrigatoriosException(e.Message);
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel dar upsert no ativo {instrument.CodCetip} por meio do EDM Services");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public FIOnDomainContract[] GetDomains()
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    return soapClient.GetDomain("");
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel recuperar os dominios do EDM Services");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public int GetCge(string cpfCnpj)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    return soapClient.GetCgebyCpfCnpj(cpfCnpj) ?? 0;
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel recuperar o CGE Emissor para o CNPJ {cpfCnpj}");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public int GetCgeGarantidor(string cpfCnpj)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    return soapClient.GetCgebyCpfCnpjGarantidor(cpfCnpj) ?? 0;
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel recuperar o CGE Garantidor para o CNPJ {cpfCnpj}");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public int GetCgeFiduciario(string cpfCnpj)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    return soapClient.GetCgebyCpfCnpjFiduciario(cpfCnpj) ?? 0;
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel recuperar o CGE Fiduciario para o CNPJ {cpfCnpj}");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public FIOnInstrumentExceptionDataContract[] GetInstrumentException(int codAtivo)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    return soapClient.GetInstrumentException(codAtivo);
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel recuperar as excecoes do ativo {codAtivo}");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public void AddFIOnException(FIOnInstrumentExceptionDataContract exception)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    soapClient.AddFIOnException(exception);
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel adicionar a excecao de id {exception.IdExcecao} no ativo {exception.CodAtivo}");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public void RemoveFIOnException(int Id)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {
                try
                {
                    soapClient.RemoveFIOnException(Id);
                }
                catch
                {
                    _logger.LogError($"Nao foi possivel remover a excecao {Id}");
                    soapClient.Abort();
                    throw;
                }
                finally
                {
                    soapClient.CloseAsync();
                }
            }
        }

        public IEnumerable<int> SetAssinaturaBpo(List<FIOnAssinaturaBpoContract> assinaturas)
        {
            var soapClient = new EDMFixedIncomeOnServiceContractClient(EndpointConfiguration.WSHttpBinding_IEDMFixedIncomeOnServiceContract, COMMON_SERVICE_URI);
            string tokenToInsert = _secureGateway.GetToken();
            soapClient.ChannelFactory.Endpoint.EndpointBehaviors.Add(new BtgSecurityBehavior(tokenToInsert));
            using (new OperationContextScope(soapClient.InnerChannel))
            {              
                try
                {
                    return soapClient.SetAssinaturaBpo(assinaturas.ToArray());
                }
                catch
                {
                    _logger.LogError($"Nao foi adicionar as assinaturas: {assinaturas.ToString()}");
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
