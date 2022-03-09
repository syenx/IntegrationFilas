using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    public class PuDeEventosLuz
    {
        public string CodigoSNA { get; set; }
        public DateTime DataEvento { get; set; }
        public string TipoEvento { get; set; }
        public decimal PuEvento { get; set; }
        public string StatusPagamento { get; set; }
        public DateTime AlteracaoStatusPagamento { get; set; }
    }
}
