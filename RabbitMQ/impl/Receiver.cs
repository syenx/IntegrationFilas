using EDM.Infohub.Integration.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace config.rabbitMQ.impl
{
    public class Receiver : IReceiver
    {
        private readonly IRabbitMQConnection rabbitMQConnection;
        private readonly ILogger<Receiver> _logger;
        private readonly IConfiguration _configuration;

        public Receiver(IRabbitMQConnection rabbitMQConnection, ILogger<Receiver> logger, IConfiguration configuration)
        {
            this.rabbitMQConnection = rabbitMQConnection;
            _logger = logger;
            _configuration = configuration;
        }

        public void Connect(string connectionName)
        {
            rabbitMQConnection.ConConnect(connectionName);
        }

        public void Receive(string queueConfiguration, Action<IDictionary<string, object>> func, bool isDurable)
        {
            var channel = rabbitMQConnection.QueueDeclare(_configuration[$"RabbitMQ:{queueConfiguration}:QueueName"], isDurable);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                //var message = Encoding.UTF8.GetString(body);

                var message = ea.BasicProperties.Headers;
                message.Add("Message", Encoding.UTF8.GetString(body));
                try
                {
                    _logger.LogInformation($"Recebendo na fila '{queueConfiguration}' a mensagem: {message}");

                    func.Invoke(message);
                    _logger.LogInformation($"Dando ACK ");
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Erro no recebimento da mensagem. QueueName: {queueConfiguration}. Mensagem: {ea.Body.ToString()}. Erro: {e.Message}");
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue:true);
                    throw;
                }
            };
            channel.BasicQos(0, 5, false);
            channel.BasicConsume(queue: _configuration[$"RabbitMQ:{queueConfiguration}:QueueName"],
                                 autoAck: false,
                                 consumer: consumer);

        }
    }
}
