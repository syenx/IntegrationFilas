using config.rabbitMQ;
using EDM.Infohub.Integration.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EDM.Infohub.BPO.RabbitMQ
{
    public class RabbitReceiver
    {
        private readonly IReceiver _receiver;

        private readonly ILogger<RabbitReceiver> _logger;

        private MessageProcessor _processor;

        public RabbitReceiver(IReceiver receiver, ILogger<RabbitReceiver> logger, MessageProcessor processor)
        {
            _receiver = receiver;
            _logger = logger;
            _processor = processor;

            //_serviceProvider = serviceProvider;
        }

        public void StartRabbitService()
        {
            Action<IDictionary<string,object>> messageTarget;
            Action<IDictionary<string, object>> messageTargetCadastro;
            Action<IDictionary<string, object>> messageTargetAssinatura;
            Action<IDictionary<string, object>> messageTargetPuDeEventos;
            Action<IDictionary<string, object>> messageTargetPuHistorico;

            messageTarget = TreatIncomingMessage;
            messageTargetCadastro = TreatCadastro;
            messageTargetAssinatura = TreatAssinatura;
            messageTargetPuDeEventos = TreatPuDeEventos;
            messageTargetPuHistorico = TreatPuHistorico;

            //_rabbitmq.Connect("ConnectionConfiguration", "QueueConfiguration");
            _receiver.Connect("ConnectionConfiguration");
            _receiver.Receive("QueueConfiguration", messageTarget, true);
            _receiver.Receive("DadosCaracteristicosQueue", messageTargetCadastro, false);
            _receiver.Receive("AssinaturasQueue", messageTargetAssinatura, true);
            _receiver.Receive("PuDeEventosQueue", messageTargetPuDeEventos, true);
            _receiver.Receive("PrecoHistoricoQueue", messageTargetPuHistorico, true);

        }


        private void TreatIncomingMessage(IDictionary<string, object> message)
        {
            _logger.LogInformation($"Mensagem lida: {message["Message"]}");

            var messageObj = JsonConvert.DeserializeObject<List<DadosPreco>>((string)message["Message"]);

            _processor.Process(messageObj, message);

        }

        private void TreatCadastro(IDictionary<string, object> message)
        {
            _logger.LogInformation($"Mensagem lida: {message["Message"]}");

            var messageObj = JsonConvert.DeserializeObject<List<DadosCaracteristicos>>((string)message["Message"]);

            _processor.ProcessCadastro(messageObj, message);

        }

        private void TreatAssinatura(IDictionary<string, object> message)
        {
            _logger.LogInformation($"Mensagem lida: {message["Message"]}");

            var messageObj = JsonConvert.DeserializeObject<AssinaturaMdp>((string)message["Message"]);

            _processor.ProcessAssinatura(messageObj, message);
        }

        private void TreatPuDeEventos(IDictionary<string, object> message)
        {
            _logger.LogInformation($"Mensagem lida: {message["Message"]}");

            var messageObj = JsonConvert.DeserializeObject<List<DadosPreco>>((string)message["Message"]);

            _processor.ProcessPuDeEventos(messageObj, message);

        }

        private void TreatPuHistorico(IDictionary<string, object> message)
        {
            _logger.LogInformation($"Mensagem lida: {message["Message"]}");

            var messageObj = JsonConvert.DeserializeObject<List<DadosPreco>>((string)message["Message"]);

            _processor.ProcessPUHistorico(messageObj, message);

        }
    }
}
