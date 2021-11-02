using Microsoft.EntityFrameworkCore;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OKRAdminService.Services
{
    public class MasterService : BaseService, IMasterService
    {

        public readonly IRepositoryAsync<CycleDurationMaster> cycleDurationMasteRepo;
        public readonly IRepositoryAsync<OrganizationObjective> organisationObjectiveRepo;
        public readonly IRepositoryAsync<RoleMaster> roleMasterRepo;
        public readonly IRepositoryAsync<OrganisationCycle> organisationCycleRepo;
        public readonly IRepositoryAsync<ObjectivesMaster> objectivesRepo;
        public readonly IRepositoryAsync<OkrStatusMaster> okrStatusMasterRepo;
        public readonly IRepositoryAsync<PermissionMaster> permissionMasterRepo;
        public readonly IRepositoryAsync<AssignmentTypeMaster> assignmentTypeMasterRepo;
        public readonly IRepositoryAsync<MetricMaster> metricMasterRepo;
        public readonly IRepositoryAsync<MetricDataMaster> metricDataMasterRepo;
        public readonly IRepositoryAsync<KrStatusMaster> krStatusMasterRepo;
        public readonly IRepositoryAsync<GoalStatusMaster> goalStatusMasterRepo;
        public readonly IRepositoryAsync<GoalTypeMaster> goalTypeMasterRepo;
        public readonly IRepositoryAsync<OkrTypeFilter> okrTypeFilterRepo;
        public readonly IRepositoryAsync<DirectReporteesFilter> directReporteesFilterRepo;

        private readonly IOrganisationService organisationService;

        public MasterService(IServicesAggregator servicesAggregateService, IOrganisationService service) : base(servicesAggregateService)
        {
            cycleDurationMasteRepo = UnitOfWorkAsync.RepositoryAsync<CycleDurationMaster>();
            organisationObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<OrganizationObjective>();
            roleMasterRepo = UnitOfWorkAsync.RepositoryAsync<RoleMaster>();
            organisationCycleRepo = UnitOfWorkAsync.RepositoryAsync<OrganisationCycle>();
            objectivesRepo = UnitOfWorkAsync.RepositoryAsync<ObjectivesMaster>();
            okrStatusMasterRepo = UnitOfWorkAsync.RepositoryAsync<OkrStatusMaster>();
            permissionMasterRepo = UnitOfWorkAsync.RepositoryAsync<PermissionMaster>();
            assignmentTypeMasterRepo = UnitOfWorkAsync.RepositoryAsync<AssignmentTypeMaster>();
            metricMasterRepo = UnitOfWorkAsync.RepositoryAsync<MetricMaster>();
            metricDataMasterRepo = UnitOfWorkAsync.RepositoryAsync<MetricDataMaster>();

            krStatusMasterRepo = UnitOfWorkAsync.RepositoryAsync<KrStatusMaster>();
            goalStatusMasterRepo = UnitOfWorkAsync.RepositoryAsync<GoalStatusMaster>();
            goalTypeMasterRepo = UnitOfWorkAsync.RepositoryAsync<GoalTypeMaster>();
            okrTypeFilterRepo = UnitOfWorkAsync.RepositoryAsync<OkrTypeFilter>();
            directReporteesFilterRepo = UnitOfWorkAsync.RepositoryAsync<DirectReporteesFilter>();

            organisationService = service;
        }

        public async Task<MasterResponse> GetAllMasterDetailsAsync()
        {
            MasterResponse masterResponse = new MasterResponse();
            var objectiveList = await organisationObjectiveRepo.GetQueryable().Include(x => x.ObjectivesMaster).ToListAsync();
            masterResponse.OrganizationObjectivesDetails = Mapper.Map<List<OrganizationObjectivesDetails>>(objectiveList);
            var permissions = await permissionMasterRepo.GetQueryable().Where(x => x.IsActive).ToListAsync();
            masterResponse.PermissionMasters = Mapper.Map<List<PermissionMasterDetails>>(permissions);

            var cycleDuration = await cycleDurationMasteRepo.GetQueryable().ToListAsync();
            masterResponse.CycleDurationDetails = Mapper.Map<List<CycleDurationDetails>>(cycleDuration);

            var roleDetails = await roleMasterRepo.GetQueryable().Where(x => x.IsActive).ToListAsync();
            masterResponse.RoleMasterDetails = Mapper.Map<List<RoleMasterDetails>>(roleDetails);
            var organisationCycles = await organisationCycleRepo.GetQueryable().Include(x => x.CycleDurationSymbol).Include(x => x.CycleDurationMaster).Include(x => x.Organisation).OrderBy(x => x.CycleDurationMaster.CycleDurationId).ToListAsync();
            var groupList = organisationCycles.GroupBy(c => new { c.CycleDurationId, c.SymbolId });
            List<CycleDurationMasterDetails> cycleDurationsList = new List<CycleDurationMasterDetails>();
            foreach (var cycle in groupList)
            {
                var cyleList = cycle.ToList();
                CycleDurationMasterDetails durationMasterDetails = new CycleDurationMasterDetails();
                var firstElement = cyleList.FirstOrDefault();
                durationMasterDetails.CycleDuration = firstElement.CycleDurationMaster.CycleDuration;
                durationMasterDetails.CycleDurationId = firstElement.CycleDurationId;
                durationMasterDetails.IsActive = firstElement.IsActive;
                var mappedObject = Mapper.Map<List<OrganisationCycleResponse>>(cyleList);
                durationMasterDetails.OrganisationCycleDetails = (mappedObject);
                cycleDurationsList.Add(durationMasterDetails);
            }
            masterResponse.CycleDurationMasterDetails = cycleDurationsList;
            return masterResponse;
        }

        public async Task<OkrStatusMasterDetails> GetOkrFiltersMasterAsync(long organisationId)
        {
            var okrStatusDetails = from a in okrStatusMasterRepo.GetQueryable()
                                   select (new OkrStatusDetails()
                                   {
                                       Id = a.Id,
                                       StatusName = a.StatusName,
                                       Description = a.Description,
                                       Code = a.Code,
                                       Color = a.Color
                                   });
            var parentId = await organisationService.GetParentOrganisationIdAsync(organisationId);
            var organisationObjective = await organisationService.GetObjectivesByOrgIdAsync(parentId);
             var okrStatus = from p in okrStatusMasterRepo.GetQueryable()
                            select (new OkrStatusMasterDetails()
                            {
                                OkrStatusDetails = okrStatusDetails.ToList(),
                                ObjectiveDetails = organisationObjective.ObjectiveDetails
                            });
            return await okrStatus.FirstOrDefaultAsync();

        }

        public async Task<List<AssignmentTypeResponse>> GetAssignmentTypeMasterAsync()
        {
            List<AssignmentTypeResponse> assignmentTypeResponse = new List<AssignmentTypeResponse>();
            var assignmentTypeMaster = await assignmentTypeMasterRepo.GetQueryable().Where(x => x.IsActive).ToListAsync();
            if (assignmentTypeMaster != null && assignmentTypeMaster.Any())
            {
                assignmentTypeResponse = Mapper.Map<List<AssignmentTypeResponse>>(assignmentTypeMaster);
            }

            return assignmentTypeResponse;
        }

        public async Task<List<MetricMasterResponse>> GetAllMetricMasterAsync()
        {
            List<MetricMasterResponse> metricMasterResponse = new List<MetricMasterResponse>();
            var metricMasterList = await metricMasterRepo.GetQueryable().Where(x => x.IsActive).ToListAsync();
            if (metricMasterList != null && metricMasterList.Any())
            {
                foreach (var metricMaster in metricMasterList)
                {
                    var metricDetails = Mapper.Map<MetricMasterResponse>(metricMaster);

                    var metricDataMasterList = await metricDataMasterRepo.GetQueryable().Where(x => x.IsActive && x.MetricId == metricMaster.MetricId).ToListAsync();

                    if (metricDataMasterList != null && metricDataMasterList.Any())
                    {
                        metricDetails.MetricDataMaster = Mapper.Map<List<MetricDataMasterResponse>>(metricDataMasterList);
                    }

                    metricMasterResponse.Add(metricDetails);
                }
            }

            return metricMasterResponse;
        }

        public async Task<GetAllOkrMaster> GetAllOkrMaster()
        {
            GetAllOkrMaster getAllOkrMaster = new GetAllOkrMaster();

            getAllOkrMaster.AssignmentTypes = await GetAssignmentTypeMasterAsync();
            getAllOkrMaster.MetricMasters = await GetAllMetricMasterAsync();
            var goalStatus =  await goalStatusMasterRepo.GetQueryable().ToListAsync();
            getAllOkrMaster.GoalStatus = Mapper.Map<List<GoalStatusResponse>>(goalStatus);

            var KrStatus = await krStatusMasterRepo.GetQueryable().ToListAsync();
            getAllOkrMaster.KrStatus = Mapper.Map<List<KrStatusResponse>>(KrStatus);


            var goalType = await goalTypeMasterRepo.GetQueryable().ToListAsync();
            getAllOkrMaster.GoalTypes = Mapper.Map<List<GoalTypeResponse>>(goalType);

            var okrType = await okrTypeFilterRepo.GetQueryable().ToListAsync();
            getAllOkrMaster.okrTypes = Mapper.Map<List<OkrTypeResponse>>(okrType);

            var directReportees = await directReporteesFilterRepo.GetQueryable().ToListAsync();
            getAllOkrMaster.directReportees = Mapper.Map<List<DirectReporteesResponse>>(directReportees);

            return getAllOkrMaster;


        }
    }
}
