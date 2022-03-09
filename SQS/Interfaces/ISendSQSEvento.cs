using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.SQS.Interfaces
{
    public interface ISendSQSEvento
    {
        public void Send(object instance);
        public void SendBatch(IEnumerable<object> instances);
    }
}
