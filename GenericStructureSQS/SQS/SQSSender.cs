using Amazon.SQS;
using Amazon.SQS.Model;
using EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.GenericStructureSQS.SQS
{
    public class SQSSender : ISQSSender
    {
        private readonly IAmazonSQS _sqsClient;

        public SQSSender(IAmazonSQS sqsClient)
        {
            _sqsClient = sqsClient;
        }

        public async Task Send<T>(object obj, T queueConfigObj) where T : ISQSQueueConfiguration
        {
            try
            {
                var sendMessageRequest = new SendMessageRequest();
                sendMessageRequest.QueueUrl = queueConfigObj.QueueURL;

                if (obj.GetType() != typeof(string))
                    sendMessageRequest.MessageBody = JsonConvert.SerializeObject(obj);
                else
                    sendMessageRequest.MessageBody = obj.ToString();

                var sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
                if (sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Falha no envio de objeto para fila SQS");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SendBatch<T>(IEnumerable<object> obj, T queueConfigObj, int batchSize) where T : ISQSQueueConfiguration
        {
            //batchSize é importante por conta do tamanho limite de mensagem no SQS = 256 kB
            //limite o tamanho da lista serializada de acordo com o tamanho médio de um objeto de maneira a respeitar o limite de tamanho
            try
            {
                var batches = GetBatches(obj, batchSize);
                foreach (var batch in batches)
                {
                    var sendMessageRequest = new SendMessageRequest();
                    sendMessageRequest.QueueUrl = queueConfigObj.QueueURL;

                    if (batch.GetType() != typeof(string))
                        sendMessageRequest.MessageBody = JsonConvert.SerializeObject(batch);
                    else
                        sendMessageRequest.MessageBody = batch.ToString();

                    var sendMessageResponse = await _sqsClient.SendMessageAsync(sendMessageRequest);
                    if (sendMessageResponse.HttpStatusCode != HttpStatusCode.OK)
                        throw new Exception("Falha no envio de lista de objetos para fila SQS");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<IEnumerable<object>> GetBatches(IEnumerable<object> obj, int batchSize)
        {
            return obj.Select((item, inx) => new { item, inx })
                    .GroupBy(x => x.inx / batchSize)
                    .Select(g => g.Select(x => x.item));
        }
    }
}
