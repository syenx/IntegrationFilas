using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration
{
    public static class Utils
    {
        public static DateTime GetSPDateTime()
        {
            var easternTimeZone = DateTimeZoneProviders.Tzdb["America/Sao_Paulo"];
            return Instant.FromDateTimeUtc(DateTime.UtcNow)
                          .InZone(easternTimeZone)
                          .ToDateTimeUnspecified();
        }

        public static JObject JsonErrorMessage(object mensagem, string erro)
        {
            JObject message = new JObject();
            message.Add("erro", erro);
            message.Add("message", JsonConvert.SerializeObject(mensagem));
            return message;
        }

        public static List<string> ListaSnaPrecos(List<PricePricing> precos)
        {
            var ListaSna = new List<string>();
            foreach (var pu in precos)
                ListaSna.Add(pu.CodigoSNA);

            return ListaSna;
        }

        public static List<string> ListaSnaCadastros(List<DadosCaracteristicos> cadastros, bool homologacao)
        {
            var ListaSna = new List<string>();
            foreach (var cadastro in cadastros)
            {
                string codSna = cadastro.CodigoSNA;
                if (homologacao)
                    codSna = CodSnaHomologacao(cadastro.CodigoSNA);
                ListaSna.Add(codSna);
            }
            return ListaSna;
        }

        public static List<string> ListaSnaPuDeEventos(List<DadosPreco> eventos, bool homologacao)
        {
            var ListaSna = new List<string>();
            foreach (var evento in eventos)
            {
                string codSna = evento.CodigoSNA;
                if (homologacao)
                    codSna = CodSnaHomologacao(evento.CodigoSNA);
                ListaSna.Add(codSna);
            }
            return ListaSna;
        }

        public static string CodSnaHomologacao(string codSna)
        {
            codSna = $"L-{codSna}";
            if (codSna.Length > 12)
                codSna = codSna.Substring(0, 12);
            return codSna;
        }

        public static DadosCaracteristicos messageToCadastro(List<DadosCaracteristicos> message, string codSNA)
        {
            return message.Where(c => c.CodigoSNA == codSNA || "L-" + c.CodigoSNA == codSNA || "L-" + c.CodigoSNA[0..^1] == codSNA).ToList().FirstOrDefault();
        }

    }
}
