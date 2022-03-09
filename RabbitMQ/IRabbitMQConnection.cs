using RabbitMQ.Client;

namespace config.rabbitMQ
{
    public interface IRabbitMQConnection
    {
        IConnection ConConnect(string connectionName);
        (IConnection, IModel) Connect(string connectionName, string queueConfiguration);
        IModel QueueDeclare(string queueName, bool isDurable);
        IModel QueueDeclareComplete(string queueConfiguration);
    }
}
