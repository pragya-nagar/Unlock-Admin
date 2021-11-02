using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface IPermissionService
    {
        Task<bool> EditPermissionToRoleAsync(long roleId, long permissionId, bool isChecked, long loginEmpCode);
        Task<List<PermissionRoleResponseModel>> GetAllRolePermissionAsync();
        Task<List<PermissionRoleResponseModel>> SearchRoleAsync(string roleName);
        Task<List<PermissionRoleResponseModel>> SortRoleAsync(bool sortOrder);
        Task<List<UserRolePermission>> GetPermissionsByRoleIdAsync(long roleId);
    }
}
