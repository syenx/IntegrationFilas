using EDM.Infohub.BPO.RabbitMQ;
using EDM.Infohub.Integration.Models;
using EDM.Infohub.Integration.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services
{
    public interface IEDMLuzService
    {
        Task<List<Fluxos>> FluxoPapel(DateTime data, string papel);
        Task<List<Fluxos>> FluxoPapel(DateTime data, string papel, string idRequisicaoEvento, string dataRequisicaoRastreamento, string usuarioRastreamento, bool papelHomologacao);
        Task<DadosCaracteristicos> CadastroPapel(string papel);
    }
}
