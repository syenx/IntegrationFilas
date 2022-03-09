using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace EDM.Infohub.Integration.GenericStructureSQS.SQS.Interfaces
{
    public interface ISQSSender
    {
        Task Send<T>(object obj, T queueConfigObj) where T : ISQSQueueConfiguration;

        //Task Send<T>(object obj) where T : ISQSQueueConfiguration;

        Task SendBatch<T>(IEnumerable<object> obj, T queueConfigObj, int batchSize) where T : ISQSQueueConfiguration;

        //Task SendBatch<T>(IEnumerable<object> obj, int batchSize) where T : ISQSQueueConfiguration;
    }
}
