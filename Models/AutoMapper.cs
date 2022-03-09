using AutoMapper;
using EDM.Infohub.BPO.RabbitMQ;
using EDMFixedIncomeOnService;

namespace EDM.Infohub.Integration.Models
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<DadosPreco, PricePricing>();


            CreateMap<PriceHistoricalData, PriceNifi>()
                 .ForMember(dest =>
                     dest.CodAtivo,
                     opt => opt.MapFrom(src => src.CodAtivo))
                 .ForMember(dest =>
                     dest.Data,
                     opt => opt.MapFrom(src => src.Data))
                 .ForMember(dest =>
                     dest.CodPraca,
                     opt => opt.MapFrom(src => src.CodPraca))
                 .ForMember(dest =>
                     dest.CodFeeder,
                     opt => opt.MapFrom(src => src.CodFeeder))
                 .ForMember(dest =>
                     dest.CodCampo,
                     opt => opt.MapFrom(src => src.CodCampo))
                 .ForMember(dest =>
                     dest.Preco,
                     opt => opt.MapFrom(src => src.Valor))
                 .ForMember(dest =>
                     dest.FatorAjuste,
                     opt => opt.MapFrom(src => src.FatorAjuste))
                 .ForMember(dest =>
                     dest.Previsao,
                     opt => opt.MapFrom(src => src.Previsao))
                 .ForMember(dest =>
                     dest.IsRebook,
                     opt => opt.MapFrom(src => src.IsRebook));

            CreateMap<AssinaturaMdp, FIOnAssinaturaBpoContract>()
                 .ForMember(dest =>
                    dest.CodAtivo,
                    opt => opt.MapFrom(src => 0))
                .ForMember(dest =>
                    dest.CodCetip,
                    opt => opt.MapFrom(src => src.Papel))
                .ForMember(dest =>
                    dest.EsAssinado,
                    opt => opt.MapFrom(src => src.Assinado))
                .ForMember(dest =>
                    dest.ImpactaPreco,
                    opt => opt.MapFrom(src => src.ImpactaPreco))
                .ForMember(dest =>
                    dest.ImpactaCadastro,
                    opt => opt.MapFrom(src => src.ImpactaCadastro))
                .ForMember(dest =>
                    dest.ImpactaPuEvento,
                    opt => opt.MapFrom(src => src.ImpactaEvento))
                .ForMember(dest =>
                    dest.ImpactaPuHistorico,
                    opt => opt.MapFrom(src => src.ImpactaHistorico));

        }
    }
}
