using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Models
{
    [Table("PU_DE_EVENTOS")]
    public class PuDeEventos
    {
        [Key]
        public DateTime data_referencia { get; set; }
        public String codigo_instrumento { get; set; }
        public String tipo_evento { get; set; }
        public Double pu_do_evento { get; set; }
        public Double pu_do_evento_open { get; set; }
        public String situacao { get; set; }
        public String mensagem_de_liquidacao { get; set; }
        public DateTime data_movimento { get; set; }
        public String usuario { get; set; }
        public String situacaoInterna { get; set; }
        public Double puLiquidacao { get; set; }
        public Double saldoNominal { get; set; }
        public Double saldoPar { get; set; }
        public String incorporarDiferenca { get; set; }
        public String usuarioConfirmacao { get; set; }
        public DateTime? dataConfirmacao { get; set; }
        public Double rendaTributavel { get; set; }
        public String observacao { get; set; }
        public Double saldoNominalOriginal { get; set; }
        public Double valorAIncorporar { get; set; }
        public Double valorAComplementar { get; set; }
        public Int32 idEvento { get; set; }
        public Int32 eventoPai { get; set; }
        public String statusEventosFilho { get; set; }
        public String decisaoAlteracaoFluxo { get; set; }
        public String tipoLiquidacao { get; set; }
        public Double taxaLiquidacao { get; set; }
        public DateTime dataLiquidacao { get; set; }
        public DateTime? dataPagamento { get; set; }
        public String tipoPremio { get; set; }
    }
}
