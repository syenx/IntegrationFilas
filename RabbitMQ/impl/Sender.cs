using config.rabbitMQ;
using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.RabbitMQ.impl
{
    public class Sender : ISender
    {
        private readonly IConfiguration configuration;
        private readonly IRabbitMQConnection _rabbitMQConnection;
        ILogger<Sender> _logger;
        private static string codPraca;
        private static int codFeeder;

        public Sender(IConfiguration configuration, IRabbitMQConnection rabbitMQConnection, ILogger<Sender> logger)
        {
            this.configuration = configuration;
            codPraca = this.configuration["CodigoPraca"];
            codFeeder = Int32.Parse(this.configuration["CodigoFeeder"]);
            _logger = logger;
            _rabbitMQConnection = rabbitMQConnection;
        }

        public void SendObj(string queueName, object obj, bool isDurable)
        {
            var channel = _rabbitMQConnection.QueueDeclare(configuration[$"RabbitMQ:{queueName}:QueueName"], isDurable);
            try
            {
                Send(channel, configuration[$"RabbitMQ:{queueName}:QueueName"], new List<object>() { obj }, isDurable);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no envio da mensagem. Nao foi possivel converter o objeto em JSON. QueueName: {queueName}. Objeto: {obj.ToString()}. Erro: {e.Message}");
                throw;
            }
            finally
            {
                CloseChannel(channel);
            }
        }

        public void SendBulk<T>(string queueName, IEnumerable<T> message, bool isDurable)
        {
            var channel = _rabbitMQConnection.QueueDeclare(configuration[$"RabbitMQ:{queueName}:QueueName"], isDurable);

            try
            {
                foreach (var items in message)
                {
                    try
                    {
                        Send(channel, configuration[$"RabbitMQ:{queueName}:QueueName"], new List<T>() { items }, isDurable);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Erro no envio de uma mensagem. Message: '{JsonConvert.SerializeObject(items)}'. Erro: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no envio das mensagens. Messages: {JsonConvert.SerializeObject(message)}. Erro: {e.Message}");
                throw;
            }
            finally
            {
                CloseChannel(channel);
            }
        }

        private void Send(IModel channel, string queueName, object obj, bool isDurable)
        {
            string message;
            try
            {
                message = JsonConvert.SerializeObject(obj);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no envio da mensagem. Nao foi possivel converter o objeto em JSON. QueueName: {queueName}. Objeto: {obj.ToString()}. Erro: {e.Message}");
                throw;
            }
            try
            {
                IBasicProperties properties = channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>();
                properties.Headers.Add("COD_PRACA", codPraca);
                properties.Headers.Add("COD_FEEDER", codFeeder);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: properties,
                                     body: body);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Erro no envio da mensagem. QueueName: {queueName}. Mensagem: {message}. Erro: {e.Message}");
                throw;
            }
        }
        private void CloseChannel(IModel channel)
        {
            channel.Close();
        }
    }
}
