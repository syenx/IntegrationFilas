using System;
using System.Collections.Generic;

namespace config.rabbitMQ
{
    public interface IReceiver
    {
        void Connect(string connectionName);
        void Receive(string queueName, Action<IDictionary<string,object>> func, bool isDurable);
    }
}
