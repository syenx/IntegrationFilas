using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    public class Fluxos
    {
        [JsonProperty(PropertyName = "codigoSNA")]
        public string CodigoSNA { get; set; }
        [JsonProperty(PropertyName = "dataBase")]
        public DateTime DataBase { get; set; }
        [JsonProperty(PropertyName = "dataLiquidacao")]
        public DateTime DataLiquidacao { get; set; }
        [JsonProperty(PropertyName = "tipoEvento")]
        public string TipoEvento { get; set; }
        [JsonProperty(PropertyName = "taxa")]
        public double Taxa { get; set; }
    }
}
