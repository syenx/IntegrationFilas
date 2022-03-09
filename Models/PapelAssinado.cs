using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    public class PapelAssinado
    {
        public string Papel { get; set; }
        public DateTime DataAssinatura { get; set; }
        public string ImpactaPreco { get; set; }
        public string ImpactaCadastro { get; set; }
        public string ImpactaEvento { get; set; }
        public string ImpactaHistorico { get; set; }
    }
}
