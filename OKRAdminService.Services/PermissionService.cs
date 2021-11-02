using Microsoft.EntityFrameworkCore;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OKRAdminService.Services
{
    public class PermissionService : BaseService, IPermissionService
    {
        private readonly IRepositoryAsync<PermissionRoleMapping> permissionRoleMappingRepo;
        private readonly IRepositoryAsync<RoleMaster> roleMasterRepo;
        private readonly IRepositoryAsync<PermissionMaster> permissionMasterRepo;
        private readonly IRepositoryAsync<Employee> employeeRepo;
        public PermissionService(IServicesAggregator servicesAggregateService) : base(servicesAggregateService)
        {
            permissionRoleMappingRepo = UnitOfWorkAsync.RepositoryAsync<PermissionRoleMapping>();
            roleMasterRepo = UnitOfWorkAsync.RepositoryAsync<RoleMaster>();
            permissionMasterRepo = UnitOfWorkAsync.RepositoryAsync<PermissionMaster>();
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
        }

        public async Task<bool> EditPermissionToRoleAsync(long roleId, long permissionId, bool isChecked, long loginEmpCode)
        {
            Logger.Information("EditPermissionToRoleAsync called for roleId " + roleId);
            IOperationStatus operationStatus = new OperationStatus();
            var permissionRoleDetails = await permissionRoleMappingRepo.FindOneAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
            if (!(permissionRoleDetails is null))
            {
                permissionRoleDetails.IsActive = isChecked;
                permissionRoleDetails.UpdatedBy = loginEmpCode;
                permissionRoleDetails.UpdatedOn = DateTime.UtcNow;
                permissionRoleMappingRepo.Update(permissionRoleDetails);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
            }
            else
            {
                var permission = new PermissionRoleMapping()
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    IsActive = isChecked,
                    CreatedBy = loginEmpCode,
                    CreatedOn = DateTime.UtcNow
                };
                
                permissionRoleMappingRepo.Add(permission);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
            }
            return operationStatus.Success;
        }

        public async Task<List<PermissionRoleResponseModel>> GetAllRolePermissionAsync()
        {
            Logger.Information("GetAllRolePermissionAsync called");
            List<PermissionRoleResponseModel> permissionRoleList = await (from role in roleMasterRepo.GetQueryable().OrderByDescending(x => x.CreatedOn)
                                       select new PermissionRoleResponseModel
                                       {
                                           RoleId = role.RoleId,
                                           RoleName = role.RoleName,
                                           RoleDescription = role.RoleDescription,
                                           Status = role.IsActive
                                       }).ToListAsync();

            if (permissionRoleList != null && permissionRoleList.Any())
            {
                permissionRoleList.ForEach(x =>
                {
                    var userList = employeeRepo.GetQueryable().Where(o => o.RoleId == x.RoleId && o.IsActive).ToList();
                    x.AssignUsers = Mapper.Map<List<EmployeeInformation>>(userList);
                    x.TotalAssignedUsers = userList.Count;
                    x.Permission = (from permissionrole in permissionRoleMappingRepo.GetQueryable()
                                    join permission in permissionMasterRepo.GetQueryable() on permissionrole.PermissionId equals permission.PermissionId
                                    where x.RoleId == permissionrole.RoleId
                                    select new PermissionDetailModel
                                    {
                                        PermissionId = permissionrole.PermissionId,
                                        Status = permissionrole.IsActive,
                                        PermissionName = permission.Permission
                                    }).ToList();
                });
            }
            return permissionRoleList;
        }

        public async Task<List<UserRolePermission>> GetPermissionsByRoleIdAsync(long roleId)
        {
            List<UserRolePermission> userPermission = new List<UserRolePermission>();
            var modulePermissions = await (from perms in permissionMasterRepo.GetQueryable().Include(x => x.ModuleMaster)
                                           join mpperm in permissionRoleMappingRepo.GetQueryable() on perms.PermissionId equals mpperm.PermissionId into g
                                           from grp in g.DefaultIfEmpty()
                                           where grp.RoleId == roleId && perms.IsActive
                                           select new
                                           {
                                               perms.ModuleId,
                                               perms.ModuleMaster.ModuleName,
                                               grp.PermissionId,
                                               perms.Permission,
                                               grp.IsActive
                                           }).ToListAsync();

            if (modulePermissions != null && modulePermissions.Any())
            {
                var groupList = modulePermissions.GroupBy(x => x.ModuleId).ToList();
                foreach (var keyItem in groupList)
                {
                    var permissionList = keyItem.ToList();
                    if (permissionList != null && permissionList.Any())
                    {
                        UserRolePermission userRolePermission = new UserRolePermission();
                        List<PermissionDetailModel> permissions = new List<PermissionDetailModel>();
                        var firstElement = permissionList.FirstOrDefault();
                        userRolePermission.ModuleId = firstElement.ModuleId;
                        userRolePermission.ModuleName = firstElement.ModuleName;
                        foreach (var item in permissionList)
                        {
                            PermissionDetailModel permissionDetail = new PermissionDetailModel();
                            permissionDetail.PermissionId = item.PermissionId;
                            permissionDetail.PermissionName = item.Permission;
                            permissionDetail.Status = item.IsActive;
                            permissions.Add(permissionDetail);
                        }
                        userRolePermission.Permissions = permissions;
                        userPermission.Add(userRolePermission);
                    }
                }
            }
            return userPermission;
        }

        public async Task<List<PermissionRoleResponseModel>> SearchRoleAsync(string roleName)
        {
            Logger.Information("SearchRoleAsync called for roleName" + roleName);
            List<PermissionRoleResponseModel> permissionRoleResponseModel = new List<PermissionRoleResponseModel>();
            var roleDetails = await roleMasterRepo.GetQueryable().Where(x => x.RoleName.Contains(roleName)).ToListAsync();
            if (roleDetails.Count > 0)
            {
                permissionRoleResponseModel.AddRange(from role in roleDetails
                                                     select new PermissionRoleResponseModel
                                                     {
                                                         RoleId = role.RoleId,
                                                         RoleName = role.RoleName,
                                                         RoleDescription = role.RoleDescription,
                                                         Status = role.IsActive,
                                                         TotalAssignedUsers = employeeRepo.GetQueryable().Where(x => x.RoleId == role.RoleId && x.IsActive).Count(),
                                                         Permission = (from permissionrole in permissionRoleMappingRepo.GetQueryable()
                                                                       where role.RoleId == permissionrole.RoleId
                                                                       join permission in permissionMasterRepo.GetQueryable()
                                                                       on permissionrole.PermissionId equals permission.PermissionId
                                                                       select new PermissionDetailModel
                                                                       {
                                                                           PermissionId = permissionrole.PermissionId,
                                                                           Status = permissionrole.IsActive,
                                                                           PermissionName = permission.Permission
                                                                       }).ToList()
                                                     });
            }
            return permissionRoleResponseModel;
        }

        public async Task<List<PermissionRoleResponseModel>> SortRoleAsync(bool sortOrder)
        {
            Logger.Information("SortRoleAsync called for sortOrder" + sortOrder);
            List<PermissionRoleResponseModel> permissionRoleResponseModel = new List<PermissionRoleResponseModel>();
            var roleDetails = await roleMasterRepo.GetAllAsync();
            permissionRoleResponseModel.AddRange(from role in roleDetails
                                                 select new PermissionRoleResponseModel
                                                 {
                                                     RoleId = role.RoleId,
                                                     RoleName = role.RoleName,
                                                     RoleDescription = role.RoleDescription,
                                                     Status = role.IsActive,
                                                     TotalAssignedUsers = employeeRepo.GetQueryable().Where(x => x.RoleId == role.RoleId && x.IsActive).Count(),
                                                     Permission = (from permissionrole in permissionRoleMappingRepo.GetQueryable()
                                                                   where role.RoleId == permissionrole.RoleId
                                                                   join permission in permissionMasterRepo.GetQueryable()
                                                                   on permissionrole.PermissionId equals permission.PermissionId
                                                                   select new PermissionDetailModel
                                                                   {
                                                                       PermissionId = permissionrole.PermissionId,
                                                                       Status = permissionrole.IsActive,
                                                                       PermissionName = permission.Permission
                                                                   }).ToList()
                                                 });
            if (sortOrder)
            {
                return permissionRoleResponseModel.OrderByDescending(x => x.RoleName).ToList();
            }

            return permissionRoleResponseModel.OrderBy(x => x.RoleName).ToList();
        }
    }
}
