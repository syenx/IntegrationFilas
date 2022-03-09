using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces
{
    public interface ISQSReceiver
    {
        Task Receive<T>(Action<string> func, T queueConfigObj) where T : ISQSQueueConfiguration;
    }
}
