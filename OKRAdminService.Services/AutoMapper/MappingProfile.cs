using AutoMapper;
using OKRAdminService.EF;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;

namespace OKRAdminService.Services.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<OrganisationCycle, OrganisationCycleDetails>()
              .ForMember(dest => dest.CycleDurationId, opts => opts.MapFrom(src => src.CycleDurationId))
              .ForMember(dest => dest.CycleDuration, opts => opts.MapFrom(src => src.CycleDurationMaster.CycleDuration))
              .ForMember(dest => dest.OrganisationName, opts => opts.MapFrom(src => src.Organisation.OrganisationName))
              .ForMember(dest => dest.OrganisationId, opts => opts.MapFrom(src => src.Organisation.OrganisationId))
              .ReverseMap();

            CreateMap<OrganizationObjective, OrganizationObjectivesDetails>()
               .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
               .ForMember(dest => dest.OrgnizationId, opts => opts.MapFrom(src => src.OrganisationId))
               .ForMember(dest => dest.ObjectiveName, opts => opts.MapFrom(src => src.ObjectivesMaster.ObjectiveName))
               .ForMember(dest => dest.IsActive, opts => opts.MapFrom(src => src.IsActive))
               .ReverseMap();

            CreateMap<RoleMaster, RoleMasterDetails>().ReverseMap();
            CreateMap<OrganisationCycle, OrganisationCycleResponse>()
                .ForMember(dest => dest.OrganisationId, opts => opts.MapFrom(src => src.OrganisationId))
                .ForMember(dest => dest.OrganisationName, opts => opts.MapFrom(src => src.Organisation.OrganisationName))
                .ForMember(dest => dest.Symbol, opts => opts.MapFrom(src => src.CycleDurationSymbol.Symbol))
                .ForMember(dest => dest.CycleYear, opts => opts.MapFrom(src => src.CycleYear))
                .ForMember(dest => dest.CycleStartDate, opts => opts.MapFrom(src => src.CycleStartDate))
                .ForMember(dest => dest.CycleEndDate, opts => opts.MapFrom(src => src.CycleEndDate))
                .ForMember(dest => dest.OrganisationCycleId, opts => opts.MapFrom(src => src.OrganisationCycleId))
                .ForMember(dest => dest.CycleDuration, opts => opts.MapFrom(src => src.CycleDurationMaster.CycleDuration))
                .ReverseMap();

            CreateMap<OrganisationCycle, CycleDurationMasterDetails>()
                .ForMember(dest => dest.CycleDurationId, opts => opts.MapFrom(src => src.CycleDurationId))
                .ForMember(dest => dest.CycleDuration, opts => opts.MapFrom(src => src.CycleDurationMaster.CycleDuration))
                .ForMember(dest => dest.IsActive, opts => opts.MapFrom(src => src.IsActive))
                .ReverseMap();
            CreateMap<OkrStatusMaster, OkrStatusMasterDetails>().ReverseMap();

            CreateMap<OrganisationCycle, QuarterDetails>()
                .ForMember(dest => dest.OrganisationCycleId, opts => opts.MapFrom(src => src.OrganisationCycleId))
                .ForMember(dest => dest.Symbol, opts => opts.MapFrom(src => src.CycleDurationSymbol.Symbol))
                .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => src.CycleStartDate))
                .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => src.CycleEndDate))
                .ReverseMap();

            CreateMap<PermissionRoleMapping, PermissionDetailModel>()
              .ForMember(dest => dest.PermissionId, opts => opts.MapFrom(src => src.PermissionId))
              .ForMember(dest => dest.PermissionName, opts => opts.MapFrom(src => src.PermissionMaster.Permission))
              .ForMember(dest => dest.Status, opts => opts.MapFrom(src => src.IsActive))
              .ReverseMap();
            CreateMap<Organisation, ActiveOrganisations>();
            CreateMap<PermissionMaster, PermissionMasterDetails>();

            CreateMap<Organisation, OrganisationSearch>()
              .ForMember(dest => dest.OrganisationId, opts => opts.MapFrom(src => src.OrganisationId))
              .ForMember(dest => dest.OrganisationName, opts => opts.MapFrom(src => src.OrganisationName))
              .ForMember(dest => dest.OrganisationLeader, opts => opts.MapFrom(src => src.OrganisationHead))
              .ReverseMap();
            CreateMap<CycleDurationMaster, CycleDurationDetails>();

            CreateMap<Employee, EmployeeInformation>().ReverseMap();
            CreateMap<EmployeeContactDetail, UserContactDetail>().ReverseMap();
            CreateMap<AssignmentTypeMaster, AssignmentTypeResponse>().ReverseMap();
            CreateMap<MetricMaster, MetricMasterResponse>().ReverseMap();
            CreateMap<MetricDataMaster, MetricDataMasterResponse>().ReverseMap();
            CreateMap<GoalStatusMaster, GoalStatusResponse>().ReverseMap();
            CreateMap<KrStatusMaster, KrStatusResponse>().ReverseMap();
            CreateMap<GoalTypeMaster, GoalTypeResponse>().ReverseMap();
            CreateMap<OkrTypeFilter, OkrTypeResponse>().ReverseMap();
            CreateMap<DirectReporteesFilter, DirectReporteesResponse>().ReverseMap();

            CreateMap<Employee, DirectReportsDetails>();
            CreateMap<ColorCodeMaster, ColorCodesResponse>();
            CreateMap<Organisation, EmployeeOrganizationDetails>();
            CreateMap<Employee, Identity>().ReverseMap();
        }
    }
}
