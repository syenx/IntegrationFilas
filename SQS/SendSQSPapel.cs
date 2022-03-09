using Amazon.SQS;
using EDM.Infohub.Integration.GenericStructureSQS.SQS;
using EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces;
using EDM.Infohub.Integration.SQS.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace EDM.Infohub.Integration.SQS
{
    public class SendSQSPapel : ISendSQSPapel
    {
        private readonly ILogger<SendSQSPapel> _logger;
        private readonly SQSSender _papelSender;
        private readonly ISQSQueueConfiguration _SQSPapelQueueConfig;

        public SendSQSPapel(IAmazonSQS sqsClient, ILogger<SendSQSPapel> logger, ISQSConfiguration optionsSQSConfig)
        {
            _papelSender = new SQSSender(sqsClient);
            _logger = logger;
            _SQSPapelQueueConfig = optionsSQSConfig.QueuesConfig["papel"];
        }
        public void Send(object papel)
        {
            try
            {
                _papelSender.Send(new List<object>() { papel }, _SQSPapelQueueConfig).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro Send SQS Papel: " + ex.Message);
                throw ex;
            }
        }

        public void SendBatch(IEnumerable<object> papeis, int batchSize)
        {
            try
            {
                _papelSender.SendBatch(papeis, _SQSPapelQueueConfig, batchSize).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro Send Batch SQS Papel: " + ex.Message);
                throw ex;
            }
        }
    }
}
