using Microsoft.EntityFrameworkCore;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace OKRAdminService.Services
{
    public class RoleService : BaseService, IRoleService
    {
        private readonly IRepositoryAsync<RoleMaster> roleMasterRepo;
        private readonly IRepositoryAsync<Employee> employeeRepo;
        private readonly IRepositoryAsync<PermissionMaster> permissionMasterRepo;
        private readonly IRepositoryAsync<PermissionRoleMapping> permissionRoleMappingRepo;
        private readonly INotificationsEmailsService notificationsService;
        private readonly IDistributedCache _distributedCache;

        public RoleService(IServicesAggregator servicesAggregateService, INotificationsEmailsService notificationsServices, IDistributedCache distributedCache) : base(servicesAggregateService)
        {
            roleMasterRepo = UnitOfWorkAsync.RepositoryAsync<RoleMaster>();
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
            permissionMasterRepo = UnitOfWorkAsync.RepositoryAsync<PermissionMaster>();
            permissionRoleMappingRepo = UnitOfWorkAsync.RepositoryAsync<PermissionRoleMapping>();
            notificationsService = notificationsServices;
            _distributedCache = distributedCache;
        }

        public async Task<RoleRequestModel> CreateRoleAsync(RoleRequestModel roleRequestModel, long loggedInuserId, string jwtToken)
        {
            Logger.Information("CreateRoleAsync called for add Role Request" + roleRequestModel);
            RoleMaster roleMaster = new RoleMaster();
            roleMaster.RoleName = roleRequestModel.RoleName;
            roleMaster.RoleDescription = roleRequestModel.RoleDescription;
            roleMaster.IsActive = roleRequestModel.Status;
            roleMaster.CreatedBy = loggedInuserId;
            roleMaster.CreatedOn = DateTime.UtcNow;
            roleMasterRepo.Add(roleMaster);
            var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();

            if (operationStatus.Success && roleMaster.RoleId > 0)
            {

                foreach (var users in roleRequestModel.AssignUsers)
                {
                    var employeeDetails = await employeeRepo.FindOneAsync(x => x.EmployeeId == users.EmployeeId);
                    if (!(employeeDetails is null))
                    {
                        employeeDetails.RoleId = roleMaster.RoleId;
                        employeeDetails.UpdatedBy = loggedInuserId;
                        employeeDetails.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(employeeDetails);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeeDetails.OrganisationId);

                    }
                    else
                    {
                        Logger.Error($"Role could not assign to to user for {users.EmployeeId}");
                    }
                }

                await Task.Run(async () =>
                 {
                     await notificationsService.CreateRoleMailAndNotificationsAsync(roleRequestModel, CreateEditCodes.CR.ToString(), jwtToken).ConfigureAwait(false);
                 }).ConfigureAwait(false);

                var permissionDetails = await permissionMasterRepo.GetAllAsync();
                foreach (var permission in permissionDetails)
                {
                    PermissionRoleMapping permissionRoleMapping = new PermissionRoleMapping();
                    permissionRoleMapping.RoleId = roleMaster.RoleId;
                    permissionRoleMapping.PermissionId = permission.PermissionId;
                    permissionRoleMapping.IsActive = false;
                    permissionRoleMapping.CreatedBy = loggedInuserId;
                    permissionRoleMapping.CreatedOn = DateTime.UtcNow;
                    permissionRoleMappingRepo.Add(permissionRoleMapping);
                }

                await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }

            return roleRequestModel;
        }

        public async Task<AssignUserRequest> AssignRoleToUserAsync(AssignUserRequest assignUserRequest, long loggedInuserId)
        {
            Logger.Information("AssignRoleToUserAsync called for assignUserRequest" + assignUserRequest);
            var roleDetails = await roleMasterRepo.FindOneAsync(x => x.RoleId == assignUserRequest.RoleId);
            foreach (var users in assignUserRequest.AssignUsers)
            {
                var employeeDetails = await employeeRepo.FindOneAsync(x => x.EmployeeId == users.EmployeeId);
                if (!(roleDetails is null) && !(employeeDetails is null))
                {
                    employeeDetails.RoleId = roleDetails.RoleId;
                    employeeDetails.UpdatedBy = loggedInuserId;
                    employeeDetails.UpdatedOn = DateTime.UtcNow;
                    employeeRepo.Update(employeeDetails);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeeDetails.OrganisationId);

                }
            }
            await UnitOfWorkAsync.SaveChangesAsync();
            await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            return assignUserRequest;
        }

        public async Task<RoleRequestModel> EditRoleAsync(RoleRequestModel roleUpdateRequest, long loggedInuserId, string jwtToken)
        {
            Logger.Information("EditRoleAsync called for roleId " + roleUpdateRequest.RoleId);
            var roleDetails = await roleMasterRepo.FindOneAsync(x => x.RoleId == roleUpdateRequest.RoleId);
            if (!(roleDetails is null))
            {
                roleDetails.RoleName = roleUpdateRequest.RoleName;
                roleDetails.RoleDescription = roleUpdateRequest.RoleDescription;
                roleDetails.IsActive = roleUpdateRequest.Status;
                roleDetails.UpdatedBy = loggedInuserId;
                roleDetails.UpdatedOn = DateTime.UtcNow;
                roleMasterRepo.Update(roleDetails);
                var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();

                if (operationStatus.Success && roleDetails.RoleId > 0)
                {
                    await Task.Run(async () =>
                     {
                         await notificationsService.CreateRoleMailAndNotificationsAsync(roleUpdateRequest, CreateEditCodes.ER.ToString(), jwtToken).ConfigureAwait(false);
                     }).ConfigureAwait(false);
                }

                if (operationStatus.Success && roleUpdateRequest.AssignUsers != null && roleUpdateRequest.AssignUsers.Any())
                {
                    foreach (var emp in roleUpdateRequest.AssignUsers)
                    {
                        var user = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == emp.EmployeeId && x.IsActive);
                        if (user != null && user.RoleId != roleDetails.RoleId)
                        {
                            user.RoleId = roleDetails.RoleId;
                            user.UpdatedBy = loggedInuserId;
                            user.UpdatedOn = DateTime.UtcNow;
                            employeeRepo.Update(user);
                            
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + user.OrganisationId);
                            
                        }

                    }
                }
                await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }
            else
            {
                Logger.Error("Error occurred in EditRoleAsync called for role Id" + roleUpdateRequest.RoleId);
            }
            return roleUpdateRequest;
        }

        public async Task<RoleMaster> ActiveInactiveRoleAsync(long roleId, bool isActive, long loggedInuserId)
        {
            Logger.Information("ActiveInactiveRoleAsync called for roleId " + roleId);
            var roleDetails = await roleMasterRepo.FindOneAsync(x => x.RoleId == roleId);
            if (!(roleDetails is null))
            {
                roleDetails.IsActive = isActive;
                roleDetails.UpdatedBy = loggedInuserId;
                roleDetails.UpdatedOn = DateTime.UtcNow;
                roleMasterRepo.Update(roleDetails);
                var operationStatus = await UnitOfWorkAsync.SaveChangesAsync();

                if (operationStatus.Success && !roleDetails.IsActive)
                {
                    var defaultRoleDetails = await GetRoleByRoleNameAsync(AppConstants.DefaultUserRole);
                    var employeeDetails = await employeeRepo.FindAsync(x => x.RoleId == roleId);
                    if (!(defaultRoleDetails is null) && !(employeeDetails is null))
                    {
                        foreach (var users in employeeDetails)
                        {
                            users.RoleId = defaultRoleDetails.RoleId;
                            users.UpdatedBy = loggedInuserId;
                            users.UpdatedOn = DateTime.UtcNow;
                            employeeRepo.Update(users);
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + users.OrganisationId);
                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                        }
                        await UnitOfWorkAsync.SaveChangesAsync();                        
                    }
                }
            }
            else
            {
                Logger.Error("Error occurred in ActiveInactiveRoleAsync called for role Id" + roleId);
            }
            return roleDetails;
        }

        public async Task<List<RoleResponseModel>> GetAllRoleAsync()
        {
            Logger.Information("GetAllRoleAsync called");
            List<RoleResponseModel> roleResponseModel = new List<RoleResponseModel>();
            var roleDetails = await roleMasterRepo.GetAllAsync();
            roleResponseModel.AddRange(from role in roleDetails
                                       select new RoleResponseModel
                                       {
                                           RoleId = role.RoleId,
                                           RoleName = role.RoleName,
                                           RoleDescription = role.RoleDescription,
                                           Status = role.IsActive,
                                           TotalAssignedUsers = employeeRepo.GetQueryable().Where(x => x.RoleId == role.RoleId && x.IsActive).Count()
                                       });
            return roleResponseModel;
        }

        public async Task<UserRoleDetail> GetRolesByUserIdAsync(long userId)
        {
            Logger.Information($"GetRolesByUserIdAsync called for userId {userId}");
            var userRoles = await (from emp in employeeRepo.GetQueryable()
                                   join role in roleMasterRepo.GetQueryable() on emp.RoleId equals role.RoleId
                                   where emp.EmployeeId == userId && role.IsActive
                                   select new UserRoleDetail
                                   {
                                       EmployeeId = emp.EmployeeId,
                                       RoleId = role.RoleId,
                                       RoleName = role.RoleName
                                   }).ToListAsync();
            return userRoles.FirstOrDefault();
        }

        public async Task<IOperationStatus> DeleteAssignUserAsync(long roleId, long empId, long loggedInuserId)
        {
            Logger.Information("DeleteAssignUserAsync called for roleId " + roleId);
            IOperationStatus operationStatus = new OperationStatus();
            var employeedetails = await employeeRepo.FindOneAsync(x => x.RoleId == roleId && x.EmployeeId == empId);
            var roleDetails = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName.Equals(AppConstants.DefaultUserRole));
            if (!(employeedetails is null))
            {
                employeedetails.RoleId = roleDetails.RoleId;
                employeedetails.UpdatedBy = loggedInuserId;
                employeedetails.UpdatedOn = DateTime.UtcNow;
                employeeRepo.Update(employeedetails);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeedetails.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }
            return operationStatus;
        }

        public async Task<RoleMaster> GetRoleByRoleNameAsync(string roleName)
        {
            var roles = await roleMasterRepo.FindOneAsync(x => x.RoleName == roleName);
            return roles;
        }
        public async Task<RoleMaster> GetRoleNameAsync(string roleName, long roleId)
        {
            var roles = await roleMasterRepo.GetQueryable().FirstOrDefaultAsync(x => x.RoleName == roleName && x.RoleId != roleId);
            return roles;
        }
    }
}

