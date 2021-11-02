using OKRAdminService.EF;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface IRoleService
    {
        Task<RoleRequestModel> CreateRoleAsync(RoleRequestModel roleRequestModel, long loggedInuserId, string jwtToken);
        Task<AssignUserRequest> AssignRoleToUserAsync(AssignUserRequest assignUserRequest, long loggedInuserId);
        Task<RoleRequestModel> EditRoleAsync(RoleRequestModel roleUpdateRequest, long loggedInuserId,string jwtToken);
        Task<RoleMaster> ActiveInactiveRoleAsync(long roleId, bool isActive, long loggedInuserId);
        Task<List<RoleResponseModel>> GetAllRoleAsync();
        Task<UserRoleDetail> GetRolesByUserIdAsync(long userId);
        Task<IOperationStatus> DeleteAssignUserAsync(long roleId, long empId, long loggedInuserId);
        Task<RoleMaster> GetRoleByRoleNameAsync(string roleName);
        Task<RoleMaster> GetRoleNameAsync(string roleName, long roleId);
    }
}
