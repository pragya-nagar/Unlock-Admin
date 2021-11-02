using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OKRAdminService.Services
{
    public class OrganisationService : BaseService, IOrganisationService
    {
        private readonly IRepositoryAsync<Organisation> organisationRepo;
        private readonly IRepositoryAsync<OrganisationCycle> organisationCycleRepo;
        private readonly IRepositoryAsync<OrganizationObjective> organisationObjectiveRepo;
        private readonly IRepositoryAsync<Employee> employeeRepo;
        private readonly IRepositoryAsync<ObjectivesMaster> objectiveRepo;
        private readonly IRepositoryAsync<CycleDurationSymbol> durationSymbolRepo;
        private readonly IRepositoryAsync<CycleDurationMaster> cycleDurationMasterRepo;
        private readonly INotificationsEmailsService notificationsService;
        private readonly IRepositoryAsync<ColorCodeMaster> colorCodeRepo;
        private readonly IKeyVaultService keyVaultService;
        private readonly IDistributedCache _distributedCache;

        public OrganisationService(IServicesAggregator servicesAggregateService, INotificationsEmailsService notificationsServices, IKeyVaultService keyVault, IDistributedCache distributedCache) : base(servicesAggregateService)
        {
            organisationRepo = UnitOfWorkAsync.RepositoryAsync<Organisation>();
            organisationCycleRepo = UnitOfWorkAsync.RepositoryAsync<OrganisationCycle>();
            organisationObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<OrganizationObjective>();
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
            objectiveRepo = UnitOfWorkAsync.RepositoryAsync<ObjectivesMaster>();
            durationSymbolRepo = UnitOfWorkAsync.RepositoryAsync<CycleDurationSymbol>();
            cycleDurationMasterRepo = UnitOfWorkAsync.RepositoryAsync<CycleDurationMaster>();
            colorCodeRepo = UnitOfWorkAsync.RepositoryAsync<ColorCodeMaster>();
            notificationsService = notificationsServices;
            keyVaultService = keyVault;
            _distributedCache = distributedCache;
        }

        public async Task<IOperationStatus> AddOrganisationAsync(OrganisationRequest request, long loggedInUserId, string jwtToken)
        {
            var org = new Organisation();
            org.OrganisationName = request.OrganisationName;
            org.OrganisationHead = request.OrganisationLeader;
            org.ImagePath = request.ImagePath;
            org.IsActive = true;
            org.IsDeleted = false;
            org.ParentId = AppConstants.DefaultParentId;
            org.LogoName = request.LogoName;
            org.CreatedBy = loggedInUserId;
            org.CreatedOn = DateTime.UtcNow;
            var colorCodes = await GetOrganizationColorCodesAsync();
            var usedColorCodes = from item in organisationRepo.GetQueryable() where item.IsActive select item.ColorCode;
            var colorCode = colorCodes.FirstOrDefault(p => usedColorCodes.All(p2 => p2 != p.ColorCode));
            if (colorCode != null)
            {
                org.ColorCode = colorCode.ColorCode;
                org.BackGroundColorCode = colorCode.BackGroundColorCode;
            }

            organisationRepo.Add(org);
            var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);

            if (operationStatus.Success)
            {
                await Task.Run(async () =>
                {
                    await notificationsService.AddOrganisationsNotificationsAndEmailsAsync(request, org, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
                await GenerateOrganisationCycleAsync(request.CycleDuration, org.OrganisationId, request.CycleStartDate, loggedInUserId);
                var objectives = await objectiveRepo.GetAllAsync();
                if (objectives != null && objectives.Any())
                {
                    foreach (var item in objectives)
                    {
                        var objectiveType = new OrganizationObjective();
                        objectiveType.OrganisationId = org.OrganisationId;
                        objectiveType.ObjectiveId = item.ObjectiveId;
                        objectiveType.IsActive = item.ObjectiveName.Equals(AppConstants.PrivateObjective) ? request.IsPrivate : true;
                        objectiveType.IsDiscarded = false;
                        objectiveType.CreatedBy = loggedInUserId;
                        objectiveType.CreatedOn = DateTime.UtcNow;
                        organisationObjectiveRepo.Add(objectiveType);
                    }
                    operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                }
            }
            return operationStatus;
        }

        public async Task<OrganisationObjectives> GetObjectivesByOrgIdAsync(long organisationId)
        {
            OrganisationObjectives organisationDetails = new OrganisationObjectives();
            var parentId = await GetParentOrganisationIdAsync(organisationId);
            var objectiveList = await (from p in organisationObjectiveRepo.GetQueryable()
                                       join s in objectiveRepo.GetQueryable() on p.ObjectiveId equals s.ObjectiveId
                                       where p.OrganisationId == parentId && p.IsDiscarded == false
                                       select new ObjectiveDetails
                                       {
                                           ObjectiveId = p.ObjectiveId,
                                           ObjectiveName = s.ObjectiveName,
                                           IsActive = p.IsActive
                                       }).ToListAsync();
            organisationDetails.OrganisationId = organisationId;
            organisationDetails.ObjectiveDetails = objectiveList;
            return organisationDetails;
        }

        public async Task<OrganisationCycleResponse> GetCurrentCycleAsync(long organisationId)
        {
            OrganisationCycleResponse cycleResponse = new OrganisationCycleResponse();
            long ParentOrgId = await GetParentOrganisationIdAsync(organisationId);
            var currentCycle = await organisationCycleRepo.GetQueryable().Include(x => x.Organisation).Include(x => x.CycleDurationMaster).Include(x => x.CycleDurationSymbol).OrderBy(x => x.OrganisationCycleId).Where(x => x.OrganisationId == ParentOrgId && x.IsDiscarded == false && x.IsActive).ToListAsync();
            if (currentCycle.Count > 0)
            {
                var currentAailable = currentCycle.Any(x => x.CycleStartDate.Date <= DateTime.UtcNow.Date && x.CycleEndDate?.Date >= DateTime.UtcNow.Date);
                var organisationCycle = currentAailable ? currentCycle.FirstOrDefault(x => x.CycleStartDate.Date <= DateTime.UtcNow.Date && x.CycleEndDate?.Date >= DateTime.UtcNow.Date) : currentCycle.First();
                if (organisationCycle != null)
                    cycleResponse = Mapper.Map<OrganisationCycleResponse>(organisationCycle);
            }
            return cycleResponse;
        }
        public async Task<OrganisationDetail> GetOrganisationByIdAsync(long organisationId)
        {

            var organisationDetail = await (from org in organisationRepo.GetQueryable()
                                            join emp in employeeRepo.GetQueryable() on org.OrganisationHead equals emp.EmployeeId into g
                                            from orgdtl in g.DefaultIfEmpty()
                                            where org.OrganisationId == organisationId && org.IsActive
                                            select new OrganisationDetail
                                            {
                                                OrganisationId = org.OrganisationId,
                                                OrganisationName = org.OrganisationName,
                                                OrganisationLogo = org.ImagePath,
                                                Description = org.Description,
                                                ParentOrganisationId = Convert.ToInt64(org.ParentId),
                                                LeaderName = orgdtl != null ? orgdtl.FirstName + " " + orgdtl.LastName : string.Empty,
                                                OrganisationLeader = orgdtl.EmployeeId,
                                                LeaderProfileImage = orgdtl.ImagePath,
                                                LogoName = org.LogoName
                                            }).FirstOrDefaultAsync();


            if (organisationDetail != null)
            {
                var nextParentOrg = organisationDetail.ParentOrganisationId > 0 ? await GetOrganisationAsync(organisationDetail.ParentOrganisationId) : null;
                if (nextParentOrg != null)
                {
                    organisationDetail.ParentName = nextParentOrg.OrganisationName;
                }

                var parentId = await GetParentOrganisationIdAsync(organisationId);
                organisationDetail.HeadParentId = (organisationId == parentId) ? AppConstants.DefaultParentId : parentId;
                var cycle = await organisationCycleRepo.GetQueryable().Include(x => x.CycleDurationMaster).Where(x => x.OrganisationId == parentId && x.IsActive).OrderBy(x => x.OrganisationCycleId).FirstOrDefaultAsync();
                if (!(cycle is null))
                {
                    organisationDetail.CycleStartDate = cycle.CycleStartDate.Date;
                    organisationDetail.CycleDurationId = cycle.CycleDurationId;
                    organisationDetail.CycleDuration = cycle.CycleDurationMaster.CycleDuration;
                }

                var objective = await organisationObjectiveRepo.GetQueryable().Include(x => x.ObjectivesMaster).Where(x => x.OrganisationId == parentId && x.IsActive).ToListAsync();
                if (objective != null && objective.Any())
                {
                    var objectDetail = objective.FirstOrDefault(x => x.ObjectivesMaster.ObjectiveName.Equals(AppConstants.PrivateObjective));
                    organisationDetail.IsPrivate = objectDetail?.IsActive ?? false;
                }

                var employees = await employeeRepo.GetQueryable().Where(x => x.OrganisationId == organisationId && x.IsActive).ToListAsync();
                if (employees != null && employees.Any())
                {
                    organisationDetail.TotalEmployees = employees.Count;
                    organisationDetail.EmployeeList = Mapper.Map<List<EmployeeInformation>>(employees);
                }
            }
            return organisationDetail;
        }

        public async Task<OrganisationCycleDetails> GetOrganisationCycleDetailsAsync(long organisationId)
        {
            Logger.Information("Called GetOrganisationCycleDetails for organisation id -" + organisationId);
            OrganisationCycleDetails cycleDetails = new OrganisationCycleDetails();
            int MinCycleYear = 0;
            int MaxCycleYear = 0;
            OrganisationCycle organisationCycle = null;
            long parentId = await GetParentOrganisationIdAsync(organisationId);
            var currentCycles = await organisationCycleRepo.GetQueryable().Include(x => x.Organisation).Include(x => x.CycleDurationMaster).OrderBy(x => x.OrganisationCycleId).Where(x => x.OrganisationId == parentId && x.IsDiscarded == false && x.IsActive).ToListAsync();
            if (currentCycles != null && currentCycles.Any())
            {
                var currentAailable = currentCycles.Any(x => x.CycleStartDate.Date <= DateTime.UtcNow.Date && x.CycleEndDate?.Date >= DateTime.UtcNow.Date);
                organisationCycle = currentAailable ? currentCycles.FirstOrDefault(x => x.CycleStartDate.Date <= DateTime.UtcNow.Date && x.CycleEndDate?.Date >= DateTime.UtcNow.Date) : currentCycles.First();
                cycleDetails = Mapper.Map<OrganisationCycleDetails>(organisationCycle);
                cycleDetails.CycleStart = currentAailable ? CycleStart.Active.ToString() : CycleStart.InActive.ToString();
                MinCycleYear = (int)organisationCycle.CycleYear - 2;
                MaxCycleYear = (int)organisationCycle.CycleYear;
            }
            else
            {
                cycleDetails.CycleStart = CycleStart.NA.ToString();
                MinCycleYear = DateTime.UtcNow.Year - 2;
                MaxCycleYear = DateTime.UtcNow.Year;
                Logger.Information($"Current cycle is not availabele for organisation Id {organisationId}");
            }
            Logger.Information($"MinCycleYear- {MinCycleYear} and MaxCycleYear- {MaxCycleYear}");
            List<CycleDetails> cyclesList = new List<CycleDetails>();
            for (int year = MinCycleYear; year <= MaxCycleYear; year++)
            {
                var cycles = await organisationCycleRepo.GetQueryable().Include(x => x.CycleDurationSymbol).Where(x => x.OrganisationId == parentId && x.CycleYear == year && x.IsDiscarded == false).OrderByDescending(x => x.OrganisationCycleId).ToListAsync();
                CycleDetails cycle = new CycleDetails();
                List<QuarterDetails> quarterList = new List<QuarterDetails>();
                if (organisationCycle != null && cycles != null && cycles.Any())
                {
                    var firstCycle = cycles.First();
                    cycle.Year = Convert.ToString(firstCycle.CycleYear);
                    cycle.IsCurrentYear = (firstCycle.CycleYear == organisationCycle.CycleYear);
                    foreach (var cycleItem in cycles)
                    {
                        var quarterDetails = Mapper.Map<QuarterDetails>(cycleItem);
                        quarterDetails.IsCurrentQuarter = (cycleItem.CycleStartDate.Date <= DateTime.UtcNow.Date && cycleItem.CycleEndDate?.Date >= DateTime.UtcNow.Date);
                        quarterList.Add(quarterDetails);
                    }
                    cycle.QuarterDetails = quarterList;
                    cyclesList.Add(cycle);
                }
            }
            cycleDetails.CycleDetails = cyclesList;
            Logger.Information("Completed GetOrganisationCycleDetails for organisation id -" + organisationId);
            return cycleDetails;
        }

        public async Task<List<OrganisationSearch>> SearchOrganisationAsync(string organisationName)
        {
            var list = new List<OrganisationSearch>();
            var orgList = await organisationRepo.GetQueryable().Where(x => x.OrganisationName.Contains(organisationName)).ToListAsync();
            if (orgList != null && orgList.Any())
                list = Mapper.Map<List<OrganisationSearch>>(orgList);
            return list;
        }

        /// <summary>
        /// Update the organisation
        /// </summary>
        /// <param name="request"></param>
        /// <param name="loggedInUserId"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task<IOperationStatus> UpdateOrganisationAsync(OrganisationRequest request, long loggedInUserId, string jwtToken)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var organisation = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == request.OrganisationId && x.IsActive && x.IsDeleted == false);

            if (organisation != null)
            {
                if (!string.IsNullOrWhiteSpace(request.OrganisationName))
                    organisation.OrganisationName = request.OrganisationName;

                organisation.OrganisationHead = request.OrganisationLeader;
                organisation.ImagePath = request.ImagePath;
                organisation.LogoName = request.LogoName;
                organisation.UpdatedBy = loggedInUserId;
                organisation.UpdatedOn = DateTime.UtcNow;
                var oldOrganisationLeader = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == request.OrganisationId && x.IsActive && x.IsDeleted == false);
                organisationRepo.Update(organisation);

                var organisationCycles = await organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == request.OrganisationId && x.IsActive && x.IsDiscarded == false && x.IsProcessed == false).OrderBy(x => x.CycleStartDate).ToListAsync();
                if (organisationCycles != null && organisationCycles.Any())
                {
                    var firstCurrentCycle = organisationCycles.First();
                    if (request.CycleDuration > 0 && request.CycleDuration != firstCurrentCycle.CycleDurationId)
                    {
                        foreach (var cycle in organisationCycles)
                        {
                            cycle.IsDiscarded = true;
                            cycle.IsActive = false;
                            cycle.IsProcessed = false;
                            cycle.UpdatedBy = loggedInUserId;
                            cycle.UpdatedOn = DateTime.UtcNow;
                            organisationCycleRepo.Update(cycle);
                        }

                        await GenerateCycleWithExistingAsync(request, loggedInUserId);
                    }
                    else if (firstCurrentCycle.CycleStartDate.Date != request.CycleStartDate.Date)
                    {
                        var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
                        ////var startedDate = request.CycleStartDate;
                        if (firstCurrentCycle.CycleDurationId == durationMap[CycleDurations.Quarterly])
                        {
                            foreach (var item in organisationCycles)
                            {
                                ////var startDate = Convert.ToDateTime(request.CycleStartDate);
                                ////item.CycleStartDate = startDate;
                                ////item.CycleEndDate = item.CycleStartDate.AddMonths(3).AddDays(-1);
                                ////request.CycleStartDate = Convert.ToDateTime(item.CycleEndDate.Value.AddDays(1));
                                item.OrganisationId = request.OrganisationId;
                                item.CycleDurationId = request.CycleDuration;
                                item.IsActive = true;
                                item.IsDiscarded = false;
                                item.IsProcessed = false;
                                item.UpdatedBy = loggedInUserId;
                                item.UpdatedOn = DateTime.UtcNow;
                                ////item.CycleYear = startedDate.Year;
                                organisationCycleRepo.Update(item);
                            }
                        }
                        else if (firstCurrentCycle.CycleDurationId == durationMap[CycleDurations.HalfYearly])
                        {
                            foreach (var item in organisationCycles)
                            {
                                ////var startDate = Convert.ToDateTime(request.CycleStartDate);
                                ////item.CycleStartDate = startDate;
                                ////item.CycleEndDate = item.CycleStartDate.AddMonths(6).AddDays(-1);
                                ////request.CycleStartDate = Convert.ToDateTime(item.CycleEndDate.Value.AddDays(1));
                                item.OrganisationId = request.OrganisationId;
                                item.CycleDurationId = request.CycleDuration;
                                item.IsActive = true;
                                item.IsDiscarded = false;
                                item.IsProcessed = false;
                                item.UpdatedBy = loggedInUserId;
                                item.UpdatedOn = DateTime.UtcNow;
                                ////item.CycleYear = startedDate.Year;
                                organisationCycleRepo.Update(item);
                            }
                        }
                        else if (firstCurrentCycle.CycleDurationId == durationMap[CycleDurations.Annually])
                        {
                            foreach (var item in organisationCycles)
                            {
                                ////var startDate = Convert.ToDateTime(request.CycleStartDate);
                                ////item.CycleStartDate = startDate;
                                ////item.CycleEndDate = item.CycleStartDate.AddYears(1).AddDays(-1);
                                ////request.CycleStartDate = Convert.ToDateTime(item.CycleEndDate.Value.AddDays(1));
                                item.OrganisationId = request.OrganisationId;
                                item.CycleDurationId = request.CycleDuration;
                                item.IsActive = true;
                                item.IsDiscarded = false;
                                item.IsProcessed = false;
                                item.UpdatedBy = loggedInUserId;
                                item.UpdatedOn = DateTime.UtcNow;
                                ////item.CycleYear = startedDate.Year;
                                organisationCycleRepo.Update(item);
                            }
                        }
                        else if (firstCurrentCycle.CycleDurationId == durationMap[CycleDurations.ThreeYears])
                        {
                            foreach (var item in organisationCycles)
                            {
                                ////var startDate = Convert.ToDateTime(request.CycleStartDate);
                                ////item.CycleStartDate = startDate;
                                ////item.CycleEndDate = item.CycleStartDate.AddYears(3).AddDays(-1);
                                ////request.CycleStartDate = Convert.ToDateTime(item.CycleEndDate.Value.AddDays(1));
                                item.OrganisationId = request.OrganisationId;
                                item.CycleDurationId = request.CycleDuration;
                                item.IsActive = true;
                                item.IsDiscarded = false;
                                item.IsProcessed = false;
                                item.UpdatedBy = loggedInUserId;
                                item.UpdatedOn = DateTime.UtcNow;
                                ////item.CycleYear = startedDate.Year;
                                organisationCycleRepo.Update(item);
                            }
                        }
                    }

                }
                else
                {
                    await GenerateCycleWithExistingAsync(request, loggedInUserId);
                }

                var objectiveMap = await objectiveRepo.GetQueryable().ToDictionaryAsync(x => x.ObjectiveName.ToEnum<ObjectiveTypes>(), x => x.ObjectiveId);
                var oldObjectiveDetails = await organisationObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == request.OrganisationId && x.ObjectiveId == objectiveMap[ObjectiveTypes.Private]);
                var orgObjectives = await organisationObjectiveRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == request.OrganisationId && x.ObjectiveId == objectiveMap[ObjectiveTypes.Private]);
                if (orgObjectives != null)
                {
                    orgObjectives.UpdatedBy = loggedInUserId;
                    orgObjectives.UpdatedOn = DateTime.UtcNow;
                    orgObjectives.IsActive = request.IsPrivate;
                    organisationObjectiveRepo.Update(orgObjectives);
                }
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.OrganizationCycleDetails + request.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + request.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);

                ////Update old leader team OKRs to new leader
                if (request.OrganisationLeader > 0 && oldOrganisationLeader.OrganisationHead != request.OrganisationLeader)
                {
                    var employees = await employeeRepo.GetQueryable().Where(x => x.OrganisationId == request.OrganisationId && x.IsActive).ToListAsync();
                    foreach (var emp in employees)
                    {
                        if (emp.EmployeeId != request.OrganisationLeader)
                        {
                            emp.ReportingTo = request.OrganisationLeader;
                            emp.UpdatedBy = loggedInUserId;
                            emp.UpdatedOn = DateTime.UtcNow;
                            employeeRepo.Update(emp);
                            await UnitOfWorkAsync.SaveChangesAsync();
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + emp.OrganisationId);
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                        }

                    }

                    var currentCycle = await GetCurrentCycleAsync(request.OrganisationId);
                    UpdateTeamLeaderOkrRequest updateTeamLeaderOkrRequest = new UpdateTeamLeaderOkrRequest()
                    {
                        OldLeader = Convert.ToInt64(oldOrganisationLeader.OrganisationHead),
                        NewLeader = request.OrganisationLeader,
                        TeamId = request.OrganisationId,
                        CycleId = currentCycle.OrganisationCycleId
                    };

                    await UpdateTeamLeaderOkr(updateTeamLeaderOkrRequest, jwtToken);
                }
                if (operationStatus.Success)
                {
                    await Task.Run(async () =>
                    {
                        await notificationsService.UpdateOrganisationNotificationsAndEmailsAsync(request, organisation, oldOrganisationLeader, Convert.ToInt64(organisation.UpdatedBy), organisationCycles.First(), oldObjectiveDetails, jwtToken).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }

            }

            return operationStatus;
        }

        public async Task<List<ActiveOrganisations>> GetAllOrganisationsAsync()
        {
            List<ActiveOrganisations> orgList = new List<ActiveOrganisations>();
            using (var command = AdminDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC Sp_GetAllOrganisations ";
                command.CommandType = CommandType.Text;
                AdminDBContext.Database.OpenConnection();
                var dataReader = await command.ExecuteReaderAsync();

                while (dataReader.Read())
                {
                    ActiveOrganisations item = new ActiveOrganisations();
                    item.OrganisationId = Convert.ToInt64(dataReader["OrganisationId"].ToString());
                    item.OrganisationName = Convert.ToString(dataReader["OrganisationName"].ToString());
                    item.OrganisationHead = Convert.ToString(dataReader["OrganisationHead"].ToString());
                    item.HeadCode = Convert.ToString(dataReader["HeadCode"].ToString());
                    item.EmployeeCount = Convert.ToInt32(dataReader["EmployeeCount"].ToString());
                    item.Designation = Convert.ToString(dataReader["Designation"].ToString());
                    item.HeadImage = Convert.ToString(dataReader["HeadImage"].ToString());
                    item.OrgLogo = Convert.ToString(dataReader["OrgLogo"].ToString());
                    item.ParentId = Convert.ToInt64(dataReader["ParentId"].ToString());
                    item.Level = Convert.ToInt32(dataReader["Level"].ToString());
                    item.IsActive = Convert.ToBoolean(dataReader["IsActive"].ToString());

                    orgList.Add(item);
                }

                AdminDBContext.Database.CloseConnection();
            }
            return orgList.ToList();
        }

        public async Task<IOperationStatus> UndoChangesForOrganisationAsync(long organisationId)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var undoChanges = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == organisationId);
            undoChanges.IsActive = false;
            undoChanges.IsDeleted = true;

            organisationRepo.Update(undoChanges);
            await UnitOfWorkAsync.SaveChangesAsync();

            var undoCycleChanges = organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == organisationId);
            foreach (var item in undoCycleChanges)
            {
                item.IsActive = false;
                organisationCycleRepo.Update(item);
                await UnitOfWorkAsync.SaveChangesAsync();
            }

            var undoObjectives = organisationObjectiveRepo.GetQueryable().Where(x => x.OrganisationId == organisationId);
            foreach (var item in undoObjectives)
            {
                item.IsActive = false;
                organisationObjectiveRepo.Update(item);
                await UnitOfWorkAsync.SaveChangesAsync();
            }
            return operationStatus;
        }

        public async Task<IOperationStatus> AddChildOrganisationAsync(ChildRequest request, long loggedInUserId, string jwtToken)
        {
            Organisation organisation = new Organisation();
            organisation.OrganisationName = request.ChildOrganisationName;
            organisation.Description = request.Description;
            organisation.OrganisationHead = request.LeaderId;
            organisation.IsActive = true;
            organisation.LogoName = request.LogoName;
            organisation.ImagePath = request.LogoImage;
            organisation.ParentId = request.ParentOrganisationId;
            organisation.CreatedBy = loggedInUserId;
            organisation.CreatedOn = DateTime.UtcNow;
            organisation.IsDeleted = false;

            var colorCodes = await GetOrganizationColorCodesAsync();
            var usedColorCodes = from item in organisationRepo.GetQueryable() where item.IsActive select item.ColorCode;
            var colorCode = colorCodes.FirstOrDefault(p => usedColorCodes.All(p2 => p2 != p.ColorCode));
            if (colorCode != null)
            {
                organisation.ColorCode = colorCode.ColorCode;
                organisation.BackGroundColorCode = colorCode.BackGroundColorCode;
            }

            organisationRepo.Add(organisation);
            var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();

            if (operationStatus.Success && request.LeaderId > 0)
            {
                await Task.Run(async () =>
                {
                    await notificationsService.AddChildOrganisationEmailAndNotificationsAsync(request, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            if (request.EmployeeList != null && request.EmployeeList.Any())
            {
                foreach (var employeeId in request.EmployeeList)
                {
                    var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive);
                    if (employee != null && employee.EmployeeId > 0)
                    {
                        employee.OrganisationId = organisation.OrganisationId;
                        employee.UpdatedBy = loggedInUserId;
                        employee.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(employee);
                    }
                }
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + organisation.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }
            return operationStatus;
        }

        public async Task<IOperationStatus> UpdateChildOrganisation(ChildRequest request, long loggedInUserId, string jwtToken)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var oldOrganisationLeader = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == request.ChildOrganisationId && x.IsActive);

            var organisation = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == request.ChildOrganisationId && x.IsActive && x.IsDeleted == false);
            if (organisation != null)
            {
                organisation.OrganisationName = request.ChildOrganisationName;
                organisation.Description = request.Description;
                organisation.OrganisationHead = request.LeaderId;
                organisation.IsActive = true;
                organisation.LogoName = request.LogoName;
                organisation.ImagePath = request.LogoImage;
                organisation.ParentId = request.ParentOrganisationId;
                organisation.UpdatedBy = loggedInUserId;
                organisation.UpdatedOn = DateTime.UtcNow;
                organisation.IsDeleted = false;
                organisationRepo.Update(organisation);

                if (request.EmployeeList != null && request.EmployeeList.Any())
                {
                    foreach (var employeeId in request.EmployeeList)
                    {
                        var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive);
                        if (employee != null && employee.EmployeeId > 0)
                        {
                            employee.OrganisationId = organisation.OrganisationId;
                            if (request.LeaderId > 0 && oldOrganisationLeader.OrganisationHead != request.LeaderId && employee.EmployeeId != request.LeaderId)
                            {
                                employee.ReportingTo = request.LeaderId;
                            }
                            employee.UpdatedBy = loggedInUserId;
                            employee.UpdatedOn = DateTime.UtcNow;
                            employeeRepo.Update(employee);
                        }
                    }
                }
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + organisation.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                var currentCycle = await GetCurrentCycleAsync(request.ChildOrganisationId);
                ////Update old leader team OKRs to new leader
                if (request.LeaderId != 0 && oldOrganisationLeader.OrganisationHead != request.LeaderId)
                {
                    UpdateTeamLeaderOkrRequest updateTeamLeaderOkrRequest = new UpdateTeamLeaderOkrRequest()
                    {
                        OldLeader = Convert.ToInt64(oldOrganisationLeader.OrganisationHead),
                        NewLeader = request.LeaderId,
                        TeamId = request.ChildOrganisationId,
                        CycleId = currentCycle.OrganisationCycleId
                    };

                    await UpdateTeamLeaderOkr(updateTeamLeaderOkrRequest, jwtToken);
                }

                ///Mail to admins and organisation leader
                if (operationStatus.Success)
                {
                    var parentId = await GetParentOrganisationIdAsync(oldOrganisationLeader.OrganisationId);
                    await Task.Run(async () =>
                    {
                        await notificationsService.UpdateChildOrganisationMailAndNotificationsAsync(request, organisation, parentId, oldOrganisationLeader, Convert.ToInt64(organisation.UpdatedBy), currentCycle, jwtToken).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                }
            }

            return operationStatus;
        }

        public async Task<IOperationStatus> AddChildOrganisationToParentOrganisationAsync(long organisationId, long ChildId, long loggedInUserId)
        {
            var childDetails = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == ChildId && x.IsActive && x.IsDeleted == false);
            var objectives = organisationObjectiveRepo.GetQueryable().Where(x => x.OrganisationId == ChildId);
            var childCycle = organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == ChildId && x.IsActive && x.IsDiscarded == false);
            if (childDetails != null)
            {
                childDetails.ParentId = organisationId;
                childDetails.UpdatedBy = loggedInUserId;
                childDetails.UpdatedOn = DateTime.UtcNow;
                organisationRepo.Update(childDetails);

                if (childCycle != null)
                {
                    foreach (var item in childCycle)
                    {
                        item.IsDiscarded = true;
                        item.IsActive = false;
                        item.UpdatedBy = loggedInUserId;
                        item.UpdatedOn = DateTime.UtcNow;
                        organisationCycleRepo.Update(item);
                    }
                }
                if (objectives != null)
                {
                    foreach (var item in objectives)
                    {
                        item.IsDiscarded = true;
                        item.IsActive = false;
                        item.UpdatedBy = loggedInUserId;
                        item.UpdatedOn = DateTime.UtcNow;
                        organisationObjectiveRepo.Update(item);
                    }
                }
                return await UnitOfWorkAsync.SaveChangesAsync();
            }
            return new OperationStatus();
        }


        /// <summary>
        /// This is the id of the one that is going to be detached
        /// </summary>
        /// <param name="organisationId"></param>
        /// <returns></returns>
        public async Task<IOperationStatus> DetachChildOrganisationFromParentOrganisationAsync(long organisationId, long loggedInUserId)
        {
            var detachedChild = await GetOrganisationAsync(organisationId);
            if (detachedChild != null)
            {
                var parentId = await GetParentOrganisationIdAsync(organisationId);
                detachedChild.ParentId = AppConstants.DefaultParentId;
                detachedChild.UpdatedBy = loggedInUserId;
                detachedChild.UpdatedOn = DateTime.UtcNow;
                organisationRepo.Update(detachedChild);
                var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();

                if (operationStatus.Success)
                {
                    var cycleDurationId = organisationCycleRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == parentId && x.IsActive).CycleDurationId;
                    await GenerateOrganisationCycleAsync(cycleDurationId, detachedChild.OrganisationId, DateTime.UtcNow, loggedInUserId);

                    var objective = organisationObjectiveRepo.GetQueryable().Where(x => x.OrganisationId == parentId);
                    foreach (var item in objective)
                    {
                        var objectives = new OrganizationObjective();
                        objectives.OrganisationId = detachedChild.OrganisationId;
                        objectives.ObjectiveId = item.ObjectiveId;
                        objectives.IsActive = true;
                        objectives.IsDiscarded = false;
                        objectives.CreatedBy = item.CreatedBy;
                        objectives.CreatedOn = DateTime.UtcNow;
                        organisationObjectiveRepo.Add(objectives);
                    }
                    operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                }
                return operationStatus;
            }
            else
            {
                return new OperationStatus();
            }
        }
        public async Task<IOperationStatus> AddParentToParentOrganisationAsync(ParentRequest parent, long loggedInuserId)
        {
            var organisation = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == parent.OrganisationId);
            if (organisation.ParentId <= 0)
            {
                Organisation organisations = new Organisation();
                organisations.OrganisationName = parent.ParentName;
                organisations.OrganisationHead = parent.LeaderId;
                organisations.ImagePath = parent.ImagePath;
                organisations.IsActive = true;
                organisations.IsDeleted = false;
                organisation.LogoName = parent.LogoName;
                organisations.CreatedBy = loggedInuserId;
                organisations.CreatedOn = DateTime.UtcNow;
                organisations.ParentId = AppConstants.DefaultParentId;
                organisationRepo.Add(organisations);
                var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                if (operationStatus.Success)
                {
                    organisation.ParentId = organisations.OrganisationId;
                    organisation.UpdatedBy = loggedInuserId;
                    organisation.UpdatedOn = DateTime.UtcNow;
                    organisationRepo.Update(organisation);

                    var organisationCycle = organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == organisation.OrganisationId && x.IsActive && x.IsDiscarded == false);
                    if (organisationCycle != null)
                    {
                        foreach (var item in organisationCycle)
                        {
                            item.IsDiscarded = true;
                            item.IsActive = false;
                            item.UpdatedBy = loggedInuserId;
                            item.UpdatedOn = DateTime.UtcNow;
                            organisationCycleRepo.Update(item);
                        }
                    }
                    var objective = organisationObjectiveRepo.GetQueryable().Where(x => x.OrganisationId == organisation.OrganisationId);
                    if (objective != null)
                    {
                        foreach (var item in objective)
                        {
                            item.IsDiscarded = true;
                            item.IsActive = false;
                            item.UpdatedBy = loggedInuserId;
                            item.UpdatedOn = DateTime.UtcNow;
                            organisationObjectiveRepo.Update(item);
                        }
                    }
                    await GenerateOrganisationCycleAsync(parent.CycleDurationId, organisations.OrganisationId, Convert.ToDateTime(parent.CycleStartDate), loggedInuserId);
                    var objectives = await objectiveRepo.GetAllAsync();
                    if (objectives != null && objectives.Any())
                    {
                        foreach (var item in objectives)
                        {
                            var objectiveType = new OrganizationObjective();
                            objectiveType.OrganisationId = organisations.OrganisationId;
                            objectiveType.ObjectiveId = item.ObjectiveId;
                            objectiveType.IsActive = item.ObjectiveName.Equals(AppConstants.PrivateObjective) ? parent.IsPrivate : true;
                            objectiveType.IsDiscarded = false;
                            objectiveType.CreatedBy = loggedInuserId;
                            objectiveType.CreatedOn = DateTime.UtcNow;
                            organisationObjectiveRepo.Add(objectiveType);
                        }
                    }
                }
                return await UnitOfWorkAsync.SaveChangesAsync();
            }
            return new OperationStatus();
        }

        public async Task<IOperationStatus> AddChildAsParentToParentOrganisationAsync(long oldParentId, long newParentId, long loggedInUserId)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var Oldorganisation = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == oldParentId);
            var oldOrganisationCycle = organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == oldParentId);
            var oldOrganizationObjective = organisationObjectiveRepo.GetQueryable().Where(x => x.OrganisationId == oldParentId);

            if (Oldorganisation.ParentId <= 0)
            {
                Oldorganisation.ParentId = newParentId;
                Oldorganisation.UpdatedBy = loggedInUserId;
                Oldorganisation.UpdatedOn = DateTime.UtcNow;
                organisationRepo.Update(Oldorganisation);

                foreach (var item in oldOrganisationCycle)
                {
                    item.IsDiscarded = true;
                    item.IsActive = false;
                    item.UpdatedBy = loggedInUserId;
                    item.UpdatedOn = DateTime.UtcNow;
                    organisationCycleRepo.Update(item);
                }

                foreach (var item in oldOrganizationObjective)
                {
                    item.IsDiscarded = true;
                    item.IsActive = false;
                    item.UpdatedBy = loggedInUserId;
                    item.UpdatedOn = DateTime.UtcNow;
                    organisationObjectiveRepo.Update(item);
                }
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
            }
            return operationStatus;
        }

        public async Task<IOperationStatus> DeleteOrganisationAsync(long organisationId, long loggedInUserId)
        {
            var organisation = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == organisationId && x.IsActive && x.IsDeleted == false);
            if (organisation != null)
            {
                organisation.IsDeleted = true;
                organisation.IsActive = false;
                organisation.UpdatedBy = loggedInUserId;
                organisation.UpdatedOn = DateTime.UtcNow;
                organisationRepo.Update(organisation);

                var users = await employeeRepo.GetQueryable().Where(x => x.OrganisationId == organisationId && x.IsActive).ToListAsync();
                if (organisation.ParentId > 0 && users != null && users.Any())
                {
                    foreach (var user in users)
                    {
                        user.OrganisationId = Convert.ToInt64(organisation.ParentId);
                        user.UpdatedBy = loggedInUserId;
                        user.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(user);
                    }
                }
                else if (organisation.ParentId <= 0 && users != null && users.Any())
                {
                    foreach (var user in users)
                    {
                        user.IsActive = false;
                        user.UpdatedBy = loggedInUserId;
                        user.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(user);
                    }
                }

                var organisationCycle = organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == organisationId && x.IsDiscarded == false && x.IsActive);
                if (organisationCycle != null && organisationCycle.Any())
                {
                    foreach (var item in organisationCycle)
                    {
                        item.IsDiscarded = true;
                        item.IsActive = false;
                        item.UpdatedBy = loggedInUserId;
                        item.UpdatedOn = DateTime.UtcNow;
                        organisationCycleRepo.Update(item);
                    }
                }

                var organisationObjective = organisationObjectiveRepo.GetQueryable().Where(x => x.OrganisationId == organisationId && x.IsDiscarded == false && x.IsActive);
                if (organisationObjective != null && organisationObjective.Any())
                {
                    foreach (var item in organisationObjective)
                    {
                        item.IsDiscarded = true;
                        item.IsActive = false;
                        item.UpdatedBy = loggedInUserId;
                        item.UpdatedOn = DateTime.UtcNow;
                        organisationObjectiveRepo.Update(item);
                    }
                }
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + organisation.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                return await UnitOfWorkAsync.SaveChangesAsync();

            }
            return new OperationStatus();
        }

        public async Task<List<ImportOkrCycle>> GetImportOkrCycleAsync(long organisationId, long currentCycleId, int cycleYear)
        {
            var parentOrgId = GetParentOrganisationIdAsync(organisationId).Result;
            List<ImportOkrCycle> result = new List<ImportOkrCycle>();
            using (var command = AdminDBContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC sp_ImportOkrResult " + currentCycleId + "," + parentOrgId + "," + cycleYear;
                command.CommandType = CommandType.Text;
                AdminDBContext.Database.OpenConnection();
                var dataReader = await command.ExecuteReaderAsync();

                while (dataReader.Read())
                {
                    ImportOkrCycle item = new ImportOkrCycle();
                    item.OrganizationCycleId = Convert.ToInt64(dataReader["OrganisationCycleId"].ToString());
                    item.CycleYear = Convert.ToInt32(dataReader["CycleYear"].ToString());
                    item.Symbol = dataReader["Symbol"].ToString().Trim();
                    if (item.OrganizationCycleId > 0)
                    {
                        item.CycleStartDate = Convert.ToDateTime(Convert.ToString(dataReader["CycleStartDate"])).Date;
                        item.CycleEndDate = Convert.ToDateTime(Convert.ToString(dataReader["CycleEndDate"])).Date;
                    }
                    result.Add(item);
                }

                AdminDBContext.Database.CloseConnection();
            }
            return result;

        }

        public async Task<Organisation> GetOrganisationAsync(long orgId)
        {
            return await organisationRepo.FindOneAsync(x => x.OrganisationId == orgId && x.IsActive && x.IsDeleted == false);
        }
        public async Task<Organisation> GetOrganisationByNameAsync(string organisationName)
        {
            organisationName = organisationName.ToLower();
            return await organisationRepo.FindOneAsync(x => x.OrganisationName.ToLower().Equals(organisationName) && x.IsActive && x.IsDeleted == false);
        }
        public async Task<Organisation> GetOrganisationByNameAsync(string organisationName, long orgId)
        {
            organisationName = Convert.ToString(organisationName).ToLower();
            return await organisationRepo.FindOneAsync(x => x.OrganisationName.ToLower().Equals(organisationName) && x.IsActive && x.IsDeleted == false && x.OrganisationId != orgId);
        }
        public async Task<IOperationStatus> GenerateOrganisationCycleAsync(long cycleDurationId, long organisationId, DateTime StartDate, long loggedInUserId)
        {
            var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
            DateTime cycleStartDate = StartDate;
            if (cycleDurationId == durationMap[CycleDurations.Quarterly])
            {
                foreach (var item in durationSymbolRepo.GetQueryable().Where(x => x.CycleDurationId == cycleDurationId))
                {
                    var orgCycle = new OrganisationCycle();
                    orgCycle.CycleStartDate = cycleStartDate;
                    orgCycle.CycleEndDate = orgCycle.CycleStartDate.AddMonths(3).AddDays(-1);
                    cycleStartDate = Convert.ToDateTime(orgCycle.CycleEndDate.Value.AddDays(1));
                    orgCycle.OrganisationId = organisationId;
                    orgCycle.CycleDurationId = cycleDurationId;
                    orgCycle.IsActive = true;
                    orgCycle.IsDiscarded = false;
                    orgCycle.IsProcessed = false;
                    orgCycle.CreatedBy = loggedInUserId;
                    orgCycle.CreatedOn = DateTime.UtcNow;
                    orgCycle.SymbolId = item.Id;
                    orgCycle.CycleYear = StartDate.Year;
                    organisationCycleRepo.Add(orgCycle);
                }
            }
            else if (cycleDurationId == durationMap[CycleDurations.HalfYearly])
            {
                foreach (var item in durationSymbolRepo.GetQueryable().Where(x => x.CycleDurationId == cycleDurationId))
                {
                    var orgCycle = new OrganisationCycle();
                    orgCycle.CycleStartDate = cycleStartDate;
                    orgCycle.CycleEndDate = orgCycle.CycleStartDate.AddMonths(6).AddDays(-1);
                    cycleStartDate = Convert.ToDateTime(orgCycle.CycleEndDate.Value.AddDays(1));
                    orgCycle.OrganisationId = organisationId;
                    orgCycle.CycleDurationId = cycleDurationId;
                    orgCycle.IsActive = true;
                    orgCycle.IsDiscarded = false;
                    orgCycle.IsProcessed = false;
                    orgCycle.CreatedBy = loggedInUserId;
                    orgCycle.CreatedOn = DateTime.UtcNow;
                    orgCycle.SymbolId = item.Id;
                    orgCycle.CycleYear = StartDate.Year;
                    organisationCycleRepo.Add(orgCycle);
                }
            }
            else if (cycleDurationId == durationMap[CycleDurations.Annually])
            {
                foreach (var item in durationSymbolRepo.GetQueryable().Where(x => x.CycleDurationId == cycleDurationId))
                {
                    var orgCycle = new OrganisationCycle();
                    orgCycle.CycleStartDate = cycleStartDate;
                    orgCycle.CycleEndDate = orgCycle.CycleStartDate.AddYears(1).AddDays(-1);
                    cycleStartDate = Convert.ToDateTime(orgCycle.CycleEndDate.Value.AddDays(1));
                    orgCycle.OrganisationId = organisationId;
                    orgCycle.CycleDurationId = cycleDurationId;
                    orgCycle.IsActive = true;
                    orgCycle.IsDiscarded = false;
                    orgCycle.IsProcessed = false;
                    orgCycle.CreatedBy = loggedInUserId;
                    orgCycle.CreatedOn = DateTime.UtcNow;
                    orgCycle.SymbolId = item.Id;
                    orgCycle.CycleYear = StartDate.Year;
                    organisationCycleRepo.Add(orgCycle);
                }
            }
            else if (cycleDurationId == durationMap[CycleDurations.ThreeYears])
            {
                foreach (var item in durationSymbolRepo.GetQueryable().Where(x => x.CycleDurationId == cycleDurationId))
                {
                    var orgCycle = new OrganisationCycle();
                    orgCycle.CycleStartDate = cycleStartDate;
                    orgCycle.CycleEndDate = orgCycle.CycleStartDate.AddYears(3).AddDays(-1);
                    cycleStartDate = Convert.ToDateTime(orgCycle.CycleEndDate.Value.AddDays(1));
                    orgCycle.OrganisationId = organisationId;
                    orgCycle.CycleDurationId = cycleDurationId;
                    orgCycle.IsActive = true;
                    orgCycle.IsDiscarded = false;
                    orgCycle.IsProcessed = false;
                    orgCycle.CreatedBy = loggedInUserId;
                    orgCycle.CreatedOn = DateTime.UtcNow;
                    orgCycle.SymbolId = item.Id;
                    orgCycle.CycleYear = StartDate.Year;
                    organisationCycleRepo.Add(orgCycle);
                }
            }
            return await UnitOfWorkAsync.SaveChangesAsync();
        }

        public async Task GenerateCycleWithExistingAsync(OrganisationRequest request, long loggedInUserId)
        {
            var existCycles = await organisationCycleRepo.GetQueryable().Where(x => x.OrganisationId == request.OrganisationId && x.CycleDurationId == request.CycleDuration && x.CycleYear == request.CycleStartDate.Year && !x.IsActive && x.IsDiscarded == true && x.IsProcessed == false).OrderBy(x => x.CycleStartDate).ToListAsync();
            if (existCycles != null && existCycles.Any())
            {
                int updateExist = 0;
                var datePairs = await GetStartEndDatePairs(request.CycleDuration, request.CycleStartDate);
                existCycles.ForEach(cycle =>
                {
                    foreach (var pair in datePairs)
                    {
                        if (cycle.CycleStartDate.Date.Equals(pair.Key) && cycle.CycleEndDate.Value.Date.Equals(pair.Value))
                        {
                            cycle.IsActive = true;
                            cycle.IsProcessed = false;
                            cycle.IsDiscarded = false;
                            organisationCycleRepo.Update(cycle);
                            updateExist += 1;
                        }
                    }
                });
                if (updateExist == 0)
                    await GenerateOrganisationCycleAsync(request.CycleDuration, request.OrganisationId, Convert.ToDateTime(request.CycleStartDate), loggedInUserId);
            }
            else
            {
                await GenerateOrganisationCycleAsync(request.CycleDuration, request.OrganisationId, Convert.ToDateTime(request.CycleStartDate), loggedInUserId);
            }
        }

        private async Task<Dictionary<DateTime, DateTime>> GetStartEndDatePairs(long durationId, DateTime statDate)
        {
            Dictionary<DateTime, DateTime> datePairs = new Dictionary<DateTime, DateTime>();
            var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
            if (durationId == durationMap[CycleDurations.Quarterly])
            {
                for (int i = 0; i < 4; i++)
                {
                    var cycleStartDate = statDate;
                    var cycleEndDate = cycleStartDate.AddMonths(3).AddDays(-1);
                    statDate = Convert.ToDateTime(cycleEndDate.AddDays(1));
                    datePairs.Add(cycleStartDate.Date, cycleEndDate.Date);
                }
            }
            else if (durationId == durationMap[CycleDurations.HalfYearly])
            {
                for (int i = 0; i < 2; i++)
                {
                    var cycleStartDate = statDate;
                    var cycleEndDate = cycleStartDate.AddMonths(6).AddDays(-1);
                    statDate = Convert.ToDateTime(cycleEndDate.AddDays(1));
                    datePairs.Add(cycleStartDate.Date, cycleEndDate.Date);
                }
            }
            else if (durationId == durationMap[CycleDurations.Annually])
            {
                var cycleStartDate = statDate;
                var cycleEndDate = cycleStartDate.AddYears(1).AddDays(-1);
                statDate = Convert.ToDateTime(cycleEndDate.AddDays(1));
                datePairs.Add(cycleStartDate.Date, cycleEndDate.Date);
            }
            else if (durationId == durationMap[CycleDurations.ThreeYears])
            {
                var cycleStartDate = statDate;
                var cycleEndDate = cycleStartDate.AddYears(3).AddDays(-1);
                statDate = Convert.ToDateTime(cycleEndDate.AddDays(1));
                datePairs.Add(cycleStartDate.Date, cycleEndDate.Date);
            }
            return datePairs;
        }

        public async Task<bool> DoesCycleFallsInFutureDate(OrganisationRequest organisationRequest)
        {
            var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
            DateTime cycleEndDate = (organisationRequest.CycleDuration == durationMap[CycleDurations.ThreeYears]) ? organisationRequest.CycleStartDate.AddYears(3).AddDays(-1) : organisationRequest.CycleStartDate.AddYears(1).AddDays(-1);
            return (cycleEndDate.Date >= DateTime.UtcNow.Date);
        }
        public async Task<string> UploadLogoOnAzure(IFormFile file)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var result = string.Empty;

            if (keyVault != null)
            {
                string imageGuid = Guid.NewGuid().ToString();


                string strFolderName = Configuration.GetValue<string>("AzureBlob:OrganisationLogoFolderName");
                string fileExt = Path.GetExtension(file.FileName);
                string azureLocation = strFolderName + "/" + imageGuid + fileExt;

                var account = new CloudStorageAccount(new StorageCredentials(keyVault.BlobAccountName, keyVault.BlobAccountKey), true);
                var cloudBlobClient = account.CreateCloudBlobClient();

                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(keyVault.BlobContainerName);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    await cloudBlobContainer.SetPermissionsAsync(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        }
                        );
                }

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(azureLocation);
                cloudBlockBlob.Properties.ContentType = file.ContentType;

                await cloudBlockBlob.UploadFromStreamAsync(file.OpenReadStream());

                result = keyVault.BlobCdnUrl + keyVault.BlobContainerName + "/" + azureLocation;
            }
            return result;
        }

        public async Task<bool> HaveChildOrganisationsAsync(long organisationId)
        {
            var childs = await organisationRepo.GetQueryable().Where(x => x.ParentId == organisationId && x.IsActive).ToListAsync();
            return (childs != null && childs.Any()) ? true : false;
        }

        public async Task<long> GetParentOrganisationIdAsync(long organisationId)
        {
            var parentId = new long();
            do
            {
                var organisation = await GetOrganisationAsync(organisationId);
                if (organisation == null)
                    break;
                organisationId = Convert.ToInt64(organisation?.ParentId);
                parentId = Convert.ToInt64(organisation.OrganisationId);

            }
            while (organisationId != 0);
            return parentId;
        }

        public async Task<List<TeamDetails>> GetUsersTeamDetailsAsync(long loggedInUser, int goalType, long empId, bool isCoach)
        {
            var teamDetailsList = new List<TeamDetails>();
            var headOrganizations = new List<Organisation>();
            if (!isCoach)
            {
                headOrganizations = empId > 0 ? await organisationRepo.GetQueryable().Where(x => x.OrganisationHead == empId && x.IsActive && x.IsDeleted == false).ToListAsync()
                : await organisationRepo.GetQueryable().Where(x => x.OrganisationHead == loggedInUser && x.IsActive && x.IsDeleted == false).ToListAsync();
            }
            else
            {
                var loggedInUserDetails = empId > 0 ? await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == empId && x.IsActive)
                    : await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == loggedInUser && x.IsActive);
                var loggedInUserParentId = await GetParentOrganisationIdAsync(loggedInUserDetails.OrganisationId);
                var activeOrganisations = await organisationRepo.GetQueryable().Where(x => x.IsActive && x.IsDeleted == false && x.OrganisationHead != loggedInUserDetails.EmployeeId && x.OrganisationHead > 0).ToListAsync();

                headOrganizations = (from org in activeOrganisations
                                     select new Organisation()
                                     {
                                         ParentId = GetParentOrganisationIdAsync(org.OrganisationId).Result,
                                         OrganisationId = org.OrganisationId,
                                         OrganisationName = org.OrganisationName,
                                         OrganisationHead = org.OrganisationHead,
                                         ImagePath = org.ImagePath,
                                         ColorCode = org.ColorCode,
                                         BackGroundColorCode = org.BackGroundColorCode
                                     }).Where(x => x.ParentId == loggedInUserParentId || x.OrganisationId == loggedInUserParentId).ToList();
            }

            foreach (var headOrg in headOrganizations)
            {
                var orgDetails = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == headOrg.OrganisationId && x.IsActive && x.IsDeleted == false);
                var parentOrg = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == orgDetails.ParentId && x.IsActive && x.IsDeleted == false);
                var headOrgEmployeesList = await employeeRepo.GetQueryable().Where(x => x.OrganisationId == headOrg.OrganisationId && x.IsActive).ToListAsync();
                var teamEmployeeList = new List<TeamEmployeeDetails>();
                if (goalType == 2)
                {
                    foreach (var employee in headOrgEmployeesList)
                    {
                        var teamEmployees = new TeamEmployeeDetails
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeCode = employee.EmployeeCode,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Designation = employee.Designation,
                            ImagePath = employee.ImagePath,
                            OrganisationId = employee.OrganisationId
                        };

                        teamEmployeeList.Add(teamEmployees);
                    }
                }

                var leaderDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == headOrg.OrganisationHead && x.IsActive);

                var teamList = new TeamDetails
                {
                    OrganisationId = headOrg.OrganisationId,
                    OrganisationName = headOrg.OrganisationName,
                    OrganisationHead = headOrg.OrganisationHead,
                    ImagePath = headOrg.ImagePath,
                    MembersCount = headOrgEmployeesList.Count,
                    ParentName = parentOrg == null ? " " : parentOrg.OrganisationName,
                    TeamEmployees = teamEmployeeList,
                    ColorCode = headOrg.ColorCode,
                    BackGroundColorCode = headOrg.BackGroundColorCode,
                    ParentTeamColorCode = parentOrg == null ? " " : parentOrg.ColorCode,
                    ParentTeamBackGroundColorCode = parentOrg == null ? " " : parentOrg.BackGroundColorCode,
                    LeaderFirstName = leaderDetails == null ? "" : leaderDetails.FirstName,
                    LeaderLastName = leaderDetails == null ? "" : leaderDetails.LastName,
                    LeaderDesignation = leaderDetails == null ? "" : leaderDetails.Designation,
                    LeaderImagePath = leaderDetails == null ? "" : leaderDetails.ImagePath
                };

                teamDetailsList.Add(teamList);
            }

            return teamDetailsList;
        }

        public async Task<SubTeamDetails> GetTeamDetailsByIdAsync(long teamId)
        {
            var organizationDetails = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == teamId && x.IsActive && x.IsDeleted == false);
            var teamDetails = new SubTeamDetails();
            if (organizationDetails != null)
            {
                var teamEmployees = await employeeRepo.GetQueryable().Where(x => x.OrganisationId == organizationDetails.OrganisationId && x.IsActive).ToListAsync();
                var teamEmployeeList = new List<TeamEmployeeDetails>();
                foreach (var emp in teamEmployees)
                {
                    var employees = new TeamEmployeeDetails
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeCode = emp.EmployeeCode,
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Designation = emp.Designation,
                        ImagePath = emp.ImagePath,
                        OrganisationId = emp.OrganisationId
                    };

                    teamEmployeeList.Add(employees);
                }

                var leaderDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == organizationDetails.OrganisationHead && x.IsActive);

                teamDetails.OrganisationId = organizationDetails.OrganisationId;
                teamDetails.OrganisationName = organizationDetails.OrganisationName;
                teamDetails.OrganisationHead = organizationDetails.OrganisationHead;
                teamDetails.ImagePath = organizationDetails.ImagePath;
                teamDetails.ParentId = organizationDetails.ParentId;
                teamDetails.MembersCount = teamEmployees.Count;
                teamDetails.TeamEmployees = teamEmployeeList;
                teamDetails.ColorCode = organizationDetails.ColorCode;
                teamDetails.BackGroundColorCode = organizationDetails.BackGroundColorCode;
                teamDetails.LeaderFirstName = leaderDetails == null ? "" : leaderDetails.FirstName;
                teamDetails.LeaderLastName = leaderDetails == null ? "" : leaderDetails.LastName;
                teamDetails.LeaderDesignation = leaderDetails == null ? "" : leaderDetails.Designation;
            }

            return teamDetails;
        }

        public async Task<List<SubTeamDetails>> GetTeamDetailsAsync()
        {
            var allTeamDetails = new List<SubTeamDetails>();
            var organizationDetails = await organisationRepo.GetQueryable().Where(x => x.IsActive && x.IsDeleted == false).ToListAsync();
            if (organizationDetails == null) return allTeamDetails;
            {
                foreach (var org in organizationDetails)
                {
                    var teamDetails = new SubTeamDetails();
                    var teamEmployees = await employeeRepo.GetQueryable().Where(x => x.OrganisationId == org.OrganisationId && x.IsActive).ToListAsync();
                    var leaderDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == org.OrganisationHead && x.IsActive);
                    var teamEmployeeList = teamEmployees.Select(emp => new TeamEmployeeDetails
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeCode = emp.EmployeeCode,
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Designation = emp.Designation,
                        ImagePath = emp.ImagePath,
                        OrganisationId = emp.OrganisationId
                    }).ToList();
                    teamDetails.OrganisationId = org.OrganisationId;
                    teamDetails.OrganisationName = org.OrganisationName;
                    teamDetails.OrganisationHead = org.OrganisationHead;
                    teamDetails.ImagePath = org.ImagePath;
                    teamDetails.ParentId = org.ParentId;
                    teamDetails.MembersCount = teamEmployees.Count;
                    teamDetails.TeamEmployees = teamEmployeeList;
                    teamDetails.ColorCode = org.ColorCode;
                    teamDetails.BackGroundColorCode = org.BackGroundColorCode;
                    teamDetails.LeaderFirstName = leaderDetails == null ? "" : leaderDetails.FirstName;
                    teamDetails.LeaderLastName = leaderDetails == null ? "" : leaderDetails.LastName;
                    teamDetails.LeaderDesignation = leaderDetails == null ? "" : leaderDetails.Designation;
                    allTeamDetails.Add(teamDetails);
                }
            }
            return allTeamDetails;
        }


        public async Task<List<DirectReportsDetails>> GetDirectReportsByIdAsync(long employeeId)
        {
            var employeeDetails = await employeeRepo.GetQueryable().Where(x => x.ReportingTo == employeeId && x.IsActive).ToListAsync();
            var directReportsDetails = new List<DirectReportsDetails>();
            if (employeeDetails != null)
            {
                foreach (var emp in employeeDetails)
                {
                    var directReports = Mapper.Map<Employee, DirectReportsDetails>(emp);
                    var organizationDetails = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == emp.OrganisationId);
                    if (organizationDetails != null)
                    {
                        directReports.OrganizationName = organizationDetails.OrganisationName;
                        directReports.ColorCode = organizationDetails.ColorCode;
                        directReports.BackGroundColorCode = organizationDetails.BackGroundColorCode;
                        directReportsDetails.Add(directReports);
                    }
                }
            }
            return directReportsDetails;
        }

        public async Task<List<ColorCodesResponse>> GetOrganizationColorCodesAsync()
        {
            var colorCodeDetails = await colorCodeRepo.GetQueryable().Where(x => x.IsActive).ToListAsync();
            var colorCodes = Mapper.Map<List<ColorCodesResponse>>(colorCodeDetails);
            return colorCodes;
        }

        /// <summary>
        /// Update the organisation color
        /// </summary>       
        /// <returns></returns>
        public async Task<IOperationStatus> UpdateOrganisationColorAsync(long loggedInUserId, string jwtToken)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var organisation = organisationRepo.GetQueryable().Where(x => x.ColorCode == null && x.IsActive && x.IsDeleted == false).ToList();
            var colorCode = colorCodeRepo.GetQueryable().Where(x => x.IsActive).ToList();
            foreach (var item in organisation)
            {
                var organisationColor = organisationRepo.GetQueryable().Where(x => x.ColorCode != null && x.IsActive && x.IsDeleted == false).Select(x => x.ColorCode).ToList();
                foreach (var color in colorCode)
                {
                    if (!organisationColor.Contains(color.ColorCode))
                    {
                        item.BackGroundColorCode = color.BackGroundColorCode;
                        item.ColorCode = color.ColorCode;
                        item.UpdatedBy = loggedInUserId;
                        item.UpdatedOn = DateTime.UtcNow;
                        organisationRepo.Update(item);
                        await UnitOfWorkAsync.SaveChangesAsync();
                        break;
                    }
                }

            }
            return operationStatus;
        }

        public async Task<EmployeeOrganizationDetails> GetOrganizationDetailsByEmployeeId(long employeeId)
        {
            var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive);
            var organizationDetails = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == employeeDetails.OrganisationId && x.IsActive);
            var organization = Mapper.Map<Organisation, EmployeeOrganizationDetails>(organizationDetails);
            return organization;
        }

        public async Task<LicenseDetail> GetLicenceDetail(string jwtToken)
        {
            var tenantMaster = await GetTenantMaster(TenantId, jwtToken);
            LicenseDetail licenseDetail = new LicenseDetail();
            if (tenantMaster.Entity != null)
            {
                licenseDetail.ActiveUser = employeeRepo.GetQueryable().Count(x => x.IsActive);
                licenseDetail.PurchaseLicense = tenantMaster.Entity.PurchaseLicense;
                licenseDetail.BufferLicense = tenantMaster.Entity.BufferLicense;
                licenseDetail.AvailableLicense = (licenseDetail.PurchaseLicense) - licenseDetail.ActiveUser;
                licenseDetail.IsAddUserAllow =((licenseDetail.PurchaseLicense + licenseDetail.BufferLicense) - licenseDetail.ActiveUser) > 0;
                licenseDetail.Note = GetLicenceNote(licenseDetail);
            }

            return licenseDetail;
        }

        private string GetLicenceNote(LicenseDetail licenseDetail)
        {
            string note = string.Empty;
            
            if (!licenseDetail.IsAddUserAllow)
            {
                note = "You've consumed all of your available licenses, please upgrade your account to add more users";
            }
            else if (licenseDetail.AvailableLicense <= 5 && licenseDetail.AvailableLicense > 0)
            {
                note = "Only " + licenseDetail.AvailableLicense + "available licenses on your instance.";
            }
            else if (licenseDetail.AvailableLicense <= 0)
            {
                note = "You've consumed all your licenses, however we have offered you few additional licenses.";
            }
            return note;
        }
    }
}

