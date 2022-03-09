using EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces;
using System.Collections.Generic;

namespace EDM.Infohub.BPO.Models.SQS
{

    public class SQSConfig : ISQSConfiguration
    {
        public Dictionary<string, ISQSQueueConfiguration> QueuesConfig { get; set; }
    }

    public class SQSQueueConfig : ISQSQueueConfiguration
    {
        public string QueueName { get; set; }

        public string QueueURL { get; set; }

        public int MaxNumberOfMessages { get; set; }

        public int WaitTimeSeconds { get; set; }
    }

}
