using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace config.rabbitMQ.impl
{
    public class RabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<RabbitMQConnection> _logger;

        private IConnection connection;

        // private RetryPolicy _retryPolicy;


        public RabbitMQConnection(IConfiguration configuration, ILogger<RabbitMQConnection> logger)
        {
            this.configuration = configuration;
            _logger = logger;

            //_retryPolicy = Policy
            //    .Handle<Exception>()
            //    .WaitAndRetry(5, retryAttempt => {
            //        var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            //        _logger.LogWarning($"Conexão com o Rabbit não realizada, esperando {timeToWait.TotalSeconds} segundos");
            //        return timeToWait;
            //    }
            //    );
        }

        public IConnection ConConnect(string connectionName)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = configuration[$"RabbitMQ:{connectionName}:Hostname"],
                    UserName = configuration[$"RabbitMQ:{connectionName}:UserName"],
                    Password = configuration[$"RabbitMQ:{connectionName}:Password"],
                };

                var port = configuration[$"RabbitMQ:{connectionName}:Port"];
                if (port != null)
                    factory.Port = int.Parse(port);

                //var connection = _retryPolicy.Execute(() => factory.CreateConnection());
                this.connection = factory.CreateConnection();
                _logger.LogInformation($"Conexao criada. connectionName: '{connectionName}'");
                return this.connection;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Nao foi possivel criar a conexao com rabbitMQ. connectionName: '{connectionName}'. Erro: {e.Message}");
                throw;
            }
        }

        public (IConnection, IModel) Connect(string connectionName, string queueConfiguration)
        {
            var connection = ConConnect(connectionName);
            var channel = QueueDeclare(queueConfiguration, false);
            return (connection, channel);
        }

        public IModel QueueDeclareComplete(string queueConfiguration)
        {
            IModel channel;

            Dictionary<string, object> args = new Dictionary<String, Object>();
            args.Add("x-dead-letter-exchange", configuration[$"RabbitMQ:{queueConfiguration}:DeadLetterExchange"]);
            args.Add("x-dead-letter-routing-key", configuration[$"RabbitMQ:{queueConfiguration}:RoutingKey"]);

            try
            {

                channel = this.connection.CreateModel();

                //DEAD LETTER QUEUE
                channel.ExchangeDeclare(configuration[$"RabbitMQ:{queueConfiguration}:DeadLetterExchange"], ExchangeType.Direct);
                channel.QueueDeclare(
                    queue: configuration[$"RabbitMQ:{queueConfiguration}:QueueName"] + "-deadLetter",
                    durable: Convert.ToBoolean(configuration[$"RabbitMQ:{queueConfiguration}:Durable"]),
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                    );
                channel.QueueBind(
                    configuration[$"RabbitMQ:{queueConfiguration}:QueueName"] + "-deadLetter",
                    configuration[$"RabbitMQ:{queueConfiguration}:DeadLetterExchange"],
                    configuration[$"RabbitMQ:{queueConfiguration}:RoutingKey"],
                    null);

                //QUEUE
                channel.ExchangeDeclare(configuration[$"RabbitMQ:{queueConfiguration}:Exchange"], ExchangeType.Direct, true, false);
                channel.QueueDeclare(
                    queue: configuration[$"RabbitMQ:{queueConfiguration}:QueueName"],
                    durable: Convert.ToBoolean(configuration[$"RabbitMQ:{queueConfiguration}:Durable"]),
                    exclusive: false,
                    autoDelete: false,
                    arguments: args
                );

                channel.QueueBind(
                    configuration[$"RabbitMQ:{queueConfiguration}:QueueName"],
                    configuration[$"RabbitMQ:{queueConfiguration}:Exchange"],
                    configuration[$"RabbitMQ:{queueConfiguration}:RoutingKey"],
                    null);

                _logger.LogInformation($"Fila declarada. queueName: '{configuration[$"RabbitMQ:{queueConfiguration}:QueueName"]}'");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Nao foi possivel declarar queue. queueName: { configuration[$"RabbitMQ:{queueConfiguration}:QueueName"]}. Erro: {e.Message}");
                throw;
            }
            return channel;
        }

        public IModel QueueDeclare(string queueName, bool isDurable)
        {
            IModel channel;

            try
            {
                channel = this.connection.CreateModel();
                channel.QueueDeclare(
                    queue: queueName,
                    durable: isDurable,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _logger.LogInformation($"Fila declarada. queueName: '{queueName}'");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Nao foi possivel declarar queue. queueName: {queueName}. Erro: {e.Message}");
                throw;
            }
            return channel;
        }
    }
}
