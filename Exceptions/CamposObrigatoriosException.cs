using EDM.Infohub.BPO.RabbitMQ;
using EDMFixedIncomeOnService;
using System;
using System.Collections.Generic;

namespace EDM.Infohub.Integration.Exceptions
{
    [Serializable]
    public class CamposObrigatoriosException : Exception
    {
        public CamposObrigatoriosException() : base() { }
        public CamposObrigatoriosException(string message) : base(message) { }
        public CamposObrigatoriosException(string message, Exception inner) : base(message, inner) { }
        protected CamposObrigatoriosException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
