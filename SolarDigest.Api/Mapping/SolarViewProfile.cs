using AutoMapper;
using SolarDigest.Api.Data;
using SolarDigest.Api.Functions.Responses;
using SolarDigest.Api.Models;
using SolarDigest.Api.Models.SolarEdgeData;
using SolarDigest.Api.Services.SolarEdge.Response;
using SolarDigest.Models;

namespace SolarDigest.Api.Mapping
{
    public sealed class SolarViewProfile : Profile
    {
        public SolarViewProfile()
        {
            // SolarEdge raw DTO data to SolarView models (nullable to non-nullable meter values)
            CreateMap<PowerDataDto, SolarData>()
                .ForMember(dest => dest.MeterValues, opt => opt.MapFrom(src => src.PowerDetails));

            CreateMap<EnergyDataDto, SolarData>()
                .ForMember(dest => dest.MeterValues, opt => opt.MapFrom(src => src.EnergyDetails));

            CreateMap<PowerDetailsDto, MeterValues>();
            CreateMap<EnergyDetailsDto, MeterValues>();
            CreateMap<MeterDto, Meter>();

            CreateMap<MeterValueDto, MeterValue>()
                .ForMember(dest => dest.Value, opt => opt.NullSubstitute(0.0d));

            CreateMap<SiteDetails, Site>();
            CreateMap<SiteDetails, SiteEntity>();
            CreateMap<SiteEntity, Site>().ReverseMap();
            CreateMap<Site, GetSiteResponse>();

            CreateMap<MeterPowerEntity, MeterPower>();
            CreateMap<MeterPowerMonthEntity, MeterPowerMonth>().ReverseMap();
            CreateMap<MeterPowerYearEntity, MeterPowerYear>().ReverseMap();

            CreateMap<PowerUpdateHistoryEntity, PowerUpdateHistory>();
        }
    }
}