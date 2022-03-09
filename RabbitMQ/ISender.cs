using EDM.Infohub.BPO.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.RabbitMQ.impl
{
    public interface ISender
    {
        void SendObj(string queueName, object obj, bool isDurable);
        void SendBulk<T>(string queueName, IEnumerable<T> message, bool isDurable);
    }
}
