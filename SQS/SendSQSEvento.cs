using Amazon.SQS;
using EDM.Infohub.Integration.GenericStructureSQS.SQS;
using EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces;
using EDM.Infohub.Integration.SQS.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace EDM.Infohub.Integration.SQS
{
    public class SendSQSEvento : ISendSQSEvento
    {
        private readonly ILogger<SendSQSEvento> _logger;
        private readonly SQSSender _eventoSender;
        private readonly ISQSQueueConfiguration _SQSEventoQueueConfig;

        //EventBroker _broker;

        public SendSQSEvento(IAmazonSQS sqsClient, ILogger<SendSQSEvento> logger, ISQSConfiguration optionsSQSConfig)//, EventBroker eventBroker)
        {
            _eventoSender = new SQSSender(sqsClient);
            _logger = logger;
            _SQSEventoQueueConfig = optionsSQSConfig.QueuesConfig["evento"];
            //_broker = eventBroker;
            //_broker.Commands += BrokerOnCommands;
        }

        //private void BrokerOnCommands(object sender, EventArgs e)
        //{
        //    _logger.LogInformation("Chamou via evento");
        //}

        public void Send(object evento)
        {
            try
            {
                _eventoSender.Send(evento, _SQSEventoQueueConfig).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro Send SQS Evento: " + ex.Message);
                throw ex;
            }
        }

        public void SendBatch(IEnumerable<object> instances)
        {
            try
            {
                foreach (object evento in instances)
                {
                    _eventoSender.Send(evento, _SQSEventoQueueConfig).Wait();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro Send SQS Evento: " + ex.Message);
                throw ex;
            }
        }
    }
}
