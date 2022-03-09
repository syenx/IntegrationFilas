using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDM.Infohub.BPO.Models.SQS
{

    public class RastreamentoPapel
    {
        [JsonProperty(PropertyName = "id_requisicao")]
        public Guid IdRequisicao  { get; set; }
        [JsonProperty(PropertyName = "cd_sna")]
        public string Papel { get; set; }
        [JsonProperty(PropertyName = "dh_inicio_evento")]
        public DateTime DataInicioEvento{ get; set; }
        [JsonProperty(PropertyName = "dh_final_evento")]
        public DateTime? DataFimEvento { get; set; }
        [JsonProperty(PropertyName = "en_tipo")]
        public string TipoLog { get; set; }
        [JsonProperty(PropertyName = "en_status")]
        public string StatusMensagem { get; set; }
        [JsonProperty(PropertyName = "en_status_processamento")]
        public string StatusPapel { get; set; }
        [JsonProperty(PropertyName = "tx_erro")]
        public string MensagemErro { get; set; }
        [JsonProperty(PropertyName = "nm_login_usuario")]
        public string Usuario { get; set; }
    }

    [Table("log.tb_rastreamento")]
    public class RastreamentoPapelDAO
    {
        [Key]
        public virtual int? pk_tb_rastreamento { get; set; }
        public byte[] id_requisicao { get; set; }
        public string cd_sna { get; set; }
        public DateTime dh_inicio_evento { get; set; }
        public DateTime dh_final_evento { get; set; }
        public long dh_rank { get; set; }
        public string en_tipo { get; set; }
        public string en_status { get; set; }
        public string en_status_processamento { get; set; }
        public string tx_erro { get; set; }
        public string nm_login_usuario { get; set; }
    }

    public enum TipoLogEnum
    {
        [NpgsqlTypes.PgName("PRECO")]
        PRECO = 1,
        [NpgsqlTypes.PgName("CADASTRO")]
        CADASTRO = 2,
        [NpgsqlTypes.PgName("FLUXO")]
        FLUXO = 3,
        [NpgsqlTypes.PgName("EVENTO")]
        EVENTO = 4,
        [NpgsqlTypes.PgName("PRECO_HISTORICO")]
        PRECO_HISTORICO = 5
    }

    public enum StatusMensagemEnum
    {
        [NpgsqlTypes.PgName("ENVIADO_LUZ")]
        ENVIADO_LUZ = 1,
        [NpgsqlTypes.PgName("RECEBIDO_LUZ")]
        RECEBIDO_LUZ = 2,
        [NpgsqlTypes.PgName("PERSISTIDO_BPO")]
        PERSISTIDO_BPO = 3,
        [NpgsqlTypes.PgName("ENVIADO_MDP")]
        ENVIADO_MDP = 4,
        [NpgsqlTypes.PgName("PROCESSADO_MDP")]
        PROCESSADO_MDP = 5,
        [NpgsqlTypes.PgName("ERRO_MDP")]
        ERRO_MDP = 6,
        [NpgsqlTypes.PgName("FINALIZADO")]
        FINALIZADO = 7,
    }

    public enum StatusProcessamentoEnum
    {
        [NpgsqlTypes.PgName("INICIADO")]
        INICIADO = 1,
        [NpgsqlTypes.PgName("PROCESSANDO")]
        PROCESSANDO = 2,
        [NpgsqlTypes.PgName("SUCESSO")]
        SUCESSO = 3,
        [NpgsqlTypes.PgName("ERRO")]
        ERRO = 4,
    }

}
