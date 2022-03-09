using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces
{
    public interface ISQSConfiguration
    {
        Dictionary<string, ISQSQueueConfiguration> QueuesConfig { get; }

    }
}
