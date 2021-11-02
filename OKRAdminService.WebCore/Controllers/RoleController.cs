using Microsoft.AspNetCore.Mvc;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace OKRAdminService.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ApiControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IIdentityService identityService, IRoleService roleService) : base(identityService)
        {
            _roleService = roleService;
        }

        [Route("AddRole")]
        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync([Required] RoleRequestModel roleRequestModel)
        {
            Logger.Information("RoleController CreateRoleAsync called for add Role Request" + roleRequestModel);
            var payloadCustom = new PayloadCustom<RoleRequestModel>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (string.IsNullOrEmpty(roleRequestModel.RoleName))
                ModelState.AddModelError("roleName", "RoleName is required");
            else if (!string.IsNullOrEmpty(roleRequestModel.RoleName))
            {
                var roleExist = await _roleService.GetRoleByRoleNameAsync(roleRequestModel.RoleName);
                if (roleExist != null)
                    ModelState.AddModelError("roleName", "RoleName already exists");
            }

            if (ModelState.IsValid)
            {
                payloadCustom.Entity = await _roleService.CreateRoleAsync(roleRequestModel, token.EmployeeId, token.UserToken);
                if (payloadCustom.Entity != null)
                {
                    payloadCustom.MessageType = Common.MessageType.Success.ToString();
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = Response.StatusCode;
                }
                else
                {
                    payloadCustom.MessageType = Common.MessageType.Info.ToString();
                    payloadCustom.IsSuccess = false;
                    payloadCustom.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadCustom = GetPayloadStatus(payloadCustom);
            }

            return Ok(payloadCustom);
        }

        [Route("AssignUsers")]
        [HttpPost]
        public async Task<IActionResult> AssignRoleToUserAsync([Required] AssignUserRequest assignUserRequest)
        {
            Logger.Information("RoleController AssignRoleToUserAsync called for assignUserRequest" + assignUserRequest);
            var payloadCustom = new PayloadCustom<AssignUserRequest>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (assignUserRequest.RoleId <= 0)
                ModelState.AddModelError("RoleId", "Requested role is not valid");

            if (assignUserRequest.AssignUsers.Count == 0)
                ModelState.AddModelError("AssignUsers", "Users are required to assign the role");

            if (ModelState.IsValid)
            {
                payloadCustom.Entity = await _roleService.AssignRoleToUserAsync(assignUserRequest, token.EmployeeId);
                if (payloadCustom.Entity != null)
                {
                    payloadCustom.MessageType = Common.MessageType.Success.ToString();
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = Response.StatusCode;
                }
                else
                {
                    payloadCustom.MessageList.Add("Message", "Something went wrong");
                    payloadCustom.MessageType = Common.MessageType.Info.ToString();
                    payloadCustom.IsSuccess = false;
                    payloadCustom.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadCustom = GetPayloadStatus(payloadCustom);
            }

            return Ok(payloadCustom);
        }

        [Route("EditRole")]
        [HttpPut]
        public async Task<IActionResult> EditRoleAsync([Required] RoleRequestModel roleUpdateRequest)
        {
            Logger.Information("RoleController EditRoleAsync called for roleUpdateRequest" + roleUpdateRequest);
            var payloadCustom = new PayloadCustom<RoleRequestModel>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (roleUpdateRequest.RoleId <= 0)
                ModelState.AddModelError("roleId", "Role id is not valid");

            if (string.IsNullOrEmpty(roleUpdateRequest.RoleName))
                ModelState.AddModelError("roleName", "Role name is required");
            else if (!string.IsNullOrEmpty(roleUpdateRequest.RoleName))
            {
                var roleExist = await _roleService.GetRoleNameAsync(roleUpdateRequest.RoleName, roleUpdateRequest.RoleId);
                if (roleExist != null)
                    ModelState.AddModelError("roleName", "Role already exists with same name");
            }

            if (ModelState.IsValid)
            {
                payloadCustom.Entity = await _roleService.EditRoleAsync(roleUpdateRequest, token.EmployeeId, token.UserToken);
                if (payloadCustom.Entity != null)
                {
                    payloadCustom.MessageType = Common.MessageType.Success.ToString();
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadCustom = GetPayloadStatus(payloadCustom);
            }
            return Ok(payloadCustom);
        }

        [Route("ActiveInactiveRole")]
        [HttpPut]
        public async Task<IActionResult> ActiveInactiveRoleAsync([Required] long roleId, bool isActive)
        {
            Logger.Information("RoleController ActiveInactiveRoleAsync called for role Id" + roleId);
            var payloadCustom = new PayloadCustom<RoleMaster>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (!isActive)
            {
                var defaultRoleDetails = await _roleService.GetRoleByRoleNameAsync(AppConstants.DefaultUserRole);
                if (roleId == defaultRoleDetails.RoleId)
                    ModelState.AddModelError("roleId", "You can't deactivate this Role");
            }

            if (roleId <= 0)
                ModelState.AddModelError("roleId", "Role id is not valid");

            if (ModelState.IsValid)
            {
                payloadCustom.Entity = await _roleService.ActiveInactiveRoleAsync(roleId, isActive, token.EmployeeId);
                if (payloadCustom.Entity != null)
                {
                    payloadCustom.MessageType = Common.MessageType.Success.ToString();
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadCustom = GetPayloadStatus(payloadCustom);
            }

            return Ok(payloadCustom);
        }

        [Route("GetAllRole")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoleAsync()
        {
            Logger.Information("RoleController GetAllRoleAsync called");
            var payloadCustom = new PayloadCustom<RoleResponseModel>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payloadCustom.EntityList = await _roleService.GetAllRoleAsync();
            if (payloadCustom.EntityList != null && payloadCustom.EntityList.Count > 0)
            {
                payloadCustom.MessageType = Common.MessageType.Success.ToString();
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = Response.StatusCode;
            }
            else
            {
                payloadCustom.MessageList.Add("Role", "No role exits");
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadCustom);
        }

        [Route("GetRolesByUserId")]
        [HttpGet]
        public async Task<IActionResult> GetRoleByUserIdAsync([Required] long userId)
        {
            Logger.Information("RoleController GetRoleByUserIdAsync called");
            var payloadCustom = new PayloadCustom<UserRoleDetail>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (userId <= 0)
                ModelState.AddModelError("userId", "User id is not valid");

            if (ModelState.IsValid)
            {
                payloadCustom.Entity = await _roleService.GetRolesByUserIdAsync(userId);
                if (payloadCustom.Entity != null)
                {
                    payloadCustom.MessageType = Common.MessageType.Success.ToString();
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = Response.StatusCode;
                }
                else
                {
                    payloadCustom.MessageList.Add("Role", "Role does not exist or must be inactive ");
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payloadCustom = GetPayloadStatus(payloadCustom);
            }

            return Ok(payloadCustom);
        }

        [Route("DeleteAssignUser")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAssignUserAsync([Required] long roleId, [Required] long empId)
        {
            Logger.Information("RoleController DeleteAssignUserAsync called");
            var payloadCustom = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (roleId <= 0)
                ModelState.AddModelError("roleId", "Requested role id is not valid");

            if (empId <= 0)
                ModelState.AddModelError("empId", "Requested employee id is not valid");

            if (roleId == 3)
                ModelState.AddModelError("roleId", "Default role can't be remove from user");

            if (ModelState.IsValid)
            {
                payloadCustom.Entity = await _roleService.DeleteAssignUserAsync(roleId, empId, token.EmployeeId);
                if (payloadCustom.Entity.Success)
                {
                    payloadCustom.MessageType = Common.MessageType.Success.ToString();
                    payloadCustom.IsSuccess = true;
                    payloadCustom.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadCustom = GetPayloadStatus(payloadCustom);
            }

            return Ok(payloadCustom);
        }
    }
}
