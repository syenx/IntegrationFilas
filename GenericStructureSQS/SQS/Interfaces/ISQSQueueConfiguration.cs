using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces
{
    public interface ISQSQueueConfiguration
    {
        string QueueName { get; }
        string QueueURL { get; }
        int MaxNumberOfMessages { get; }
        int WaitTimeSeconds { get; }
        
    }
}
