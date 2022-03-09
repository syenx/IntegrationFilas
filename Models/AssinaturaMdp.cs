using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    public class AssinaturaMdp
    {
        public string Papel { get; set; }
        public bool ImpactaPreco { get; set; }
        public bool ImpactaCadastro { get; set; }
        public bool ImpactaEvento { get; set; }
        public bool ImpactaHistorico { get; set; }
        public bool Assinado { get; set; }
    }
}
