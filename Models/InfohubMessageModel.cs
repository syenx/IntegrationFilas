using System;

namespace EDM.Infohub.Integration.Models
{
    public class InfohubMessageModel
    {
        public String guid { get; set; }
        /// <summary>Datahora do evento na mensagem do InfoHub</summary>
        public DateTime infoHubTime { get; set; }

        /// <summary>Data hora de recebimento da mensagem pelo OBR</summary>
        public DateTime receivingTime { get; set; }

        /// <summary>Tipo da mensagem</summary>
        public string type { get; set; }

        /// <summary>Código da conta</summary>
        public string institutionAccountCode { get; set; }

        /// <summary>Mensagen do InfoHub serializada</summary>
        public string rawMessage { get; set; }

        /// <summary>Conta monitoradora</summary>
        public string monitoringAccountCode { get; set; }

        /// <summary>Conta monitorada</summary>
        public string monitoredAccountCode { get; set; }

        /// <summary>Código do Instrumento</summary>
        public string instrumentCode { get; set; }

        //<summary> Last Update Time </summary>
        public DateTime lastUpdateTime { get; set; }

        /// <summary>Id da Msg do InfoHub</summary>
        public long infoHubMessageId { get; set; }
        /// <summary>Origem da mensagem</summary>
        public string Source { get; set; }
    }
}
