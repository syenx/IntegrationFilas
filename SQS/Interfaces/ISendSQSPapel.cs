using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.SQS.Interfaces
{
    public interface ISendSQSPapel
    {
        public void Send(object instance);
        public void SendBatch(IEnumerable<object> instances, int batchSize);
    }
}
