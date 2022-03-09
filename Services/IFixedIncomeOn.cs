using EDMFixedIncomeOnService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDM.Infohub.Integration.Services.Impl
{
    public interface IFixedIncomeOn
    { 
        FIOnInstrumentContract[] GetFIOnInstrumentBySNA(string sna);
        int InsertUpdateInstrument(FIOnInstrumentContract instrument);

        FIOnDomainContract[] GetDomains();

        int GetCge(string cpfCnpj);
        int GetCgeGarantidor(string cpfCnpj);
        int GetCgeFiduciario(string cpfCnpj);
        FIOnInstrumentExceptionDataContract[] GetInstrumentException(int codAtivo);
        void AddFIOnException(FIOnInstrumentExceptionDataContract exception);
        void RemoveFIOnException(int Id);
        IEnumerable<int> SetAssinaturaBpo(List<FIOnAssinaturaBpoContract> assinaturaMdp);
    }
}
