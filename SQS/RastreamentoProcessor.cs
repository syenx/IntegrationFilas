using EDM.Infohub.BPO.Models.SQS;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.SQS.Interfaces
{
    public class RastreamentoProcessor
    {
        private ISendSQSEvento _evento;
        private ISendSQSPapel _papel;
        private ILogger<RastreamentoProcessor> _logger;

        public RastreamentoProcessor(ISendSQSEvento evento, ISendSQSPapel papel, ILogger<RastreamentoProcessor> logger)
        {
            _evento = evento;
            _papel = papel;
            _logger = logger;
        }

        public void Evento(Guid idRequisicao, TipoRequisicaoEnum tipoRequisicao, DateTime dataInicio, DateTime? dataFim, string metodo, StatusProcessamentoEnum status, JObject jsonEvento, string usuario)
        {
            var evento = new RastreamentoEvento()
            {
                IdRequisicao = idRequisicao,
                TipoRequisicao = tipoRequisicao.ToString(),
                DataInicioEvento = dataInicio,
                DataFimEvento = dataFim,
                Metodo = metodo,
                StatusEvento = status.ToString(),
                JsonEvento = jsonEvento,
                Usuario = usuario
            };
            _logger.LogInformation($"Enviando status {status} do evento para fila SQS.");
            _evento.Send(evento);
        }

        public void Papel(Guid idRequisicao, string codSna, DateTime dataInicio, DateTime? dataFim, TipoLogEnum tipoLog, StatusMensagemEnum statusMensagem, StatusProcessamentoEnum statusPapel, string mensageErro, string usuario)
        {
            var papel = new RastreamentoPapel()
            {
                IdRequisicao = idRequisicao,
                Papel = codSna,
                DataInicioEvento = dataInicio,
                DataFimEvento = dataFim,
                TipoLog = tipoLog.ToString(),
                StatusMensagem = statusMensagem.ToString(),
                StatusPapel = statusPapel.ToString(),
                MensagemErro = mensageErro,
                Usuario = usuario
            };
            _logger.LogInformation($"Enviando status {statusPapel} do papel {codSna} para fila SQS.");
            _papel.Send(papel);
        }

        public void Papeis(Guid idRequisicao, List<string> papeis, DateTime dataInicio, DateTime? dataFim, TipoLogEnum tipoLog, StatusMensagemEnum statusMensagem, StatusProcessamentoEnum statusPapel, string mensageErro, string usuario)
        {
            var listaRastreamento = new List<RastreamentoPapel>();
            foreach(var codSna in papeis)
            {
                listaRastreamento.Add(new RastreamentoPapel()
                {
                    IdRequisicao = idRequisicao,
                    Papel = codSna,
                    DataInicioEvento = dataInicio,
                    DataFimEvento = dataFim,
                    TipoLog = tipoLog.ToString(),
                    StatusMensagem = statusMensagem.ToString(),
                    StatusPapel = statusPapel.ToString(),
                    MensagemErro = mensageErro,
                    Usuario = usuario
                });
            }
            _logger.LogInformation($"Enviando status {statusPapel} dos papeis para fila SQS.");
            _papel.SendBatch(listaRastreamento, 50);
        }
    }
}
