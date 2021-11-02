using Microsoft.AspNetCore.Mvc;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace OKRAdminService.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ApiControllerBase
    {

        private readonly IPermissionService _permissionService;
        public PermissionController(IIdentityService identityService, IPermissionService permissionService) : base(identityService)
        {
            _permissionService = permissionService;
        }

        [Route("EditRolePermission")]
        [HttpPut]
        public async Task<IActionResult> EditPermissionToRoleAsync([Required] long roleId, [Required] long permissionId, bool isChecked)
        {
            Logger.Information("PermissionController EditPermissionToRoleAsync called for role Id" + roleId);
            var payloadCustom = new PayloadCustom<bool>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (roleId <= 0)
                ModelState.AddModelError("roleId", "RoleId is not valid");

            if (permissionId <= 0)
                ModelState.AddModelError("permissionId", "PermissionId is not valid");

            if (ModelState.IsValid)
            {
                payloadCustom.IsSuccess = await _permissionService.EditPermissionToRoleAsync(roleId, permissionId, isChecked, token.EmployeeId);
                if (payloadCustom.IsSuccess)
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

        [Route("GetAllRolePermission")]
        [HttpGet]
        public async Task<IActionResult> GetAllRolePermissionAsync()
        {
            Logger.Information("PermissionController GetAllRolePermissionAsync called");
            var payloadCustom = new PayloadCustom<PermissionRoleResponseModel>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payloadCustom.EntityList = await _permissionService.GetAllRolePermissionAsync();
            if (payloadCustom.EntityList != null && payloadCustom.EntityList.Count > 0)
            {
                payloadCustom.MessageType = Common.MessageType.Success.ToString();
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = Response.StatusCode;
            }
            else
            {
                payloadCustom.MessageList.Add("RolePermission", "Roles permissions not found");
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadCustom);
        }

        [Route("SearchRole")]
        [HttpGet]
        public async Task<IActionResult> SearchRoleAsync([Required] string roleName)
        {
            Logger.Information("PermissionController SearchRoleAsync called for roleName" + roleName);
            var payloadCustom = new PayloadCustom<PermissionRoleResponseModel>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payloadCustom.EntityList = await _permissionService.SearchRoleAsync(roleName);
            if (payloadCustom.EntityList != null && payloadCustom.EntityList.Count > 0)
            {
                payloadCustom.MessageType = Common.MessageType.Success.ToString();
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = Response.StatusCode;
            }
            else
            {
                payloadCustom.MessageList.Add("RoleName", $"Roles not found with name -{roleName}");
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadCustom);
        }

        [Route("SortRole")]
        [HttpGet]
        public async Task<IActionResult> SortRoleAsync(bool sortOrder)
        {
            Logger.Information("PermissionController SortRoleAsync called for sortOrder" + sortOrder);
            var payloadCustom = new PayloadCustom<PermissionRoleResponseModel>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payloadCustom.EntityList = await _permissionService.SortRoleAsync(sortOrder);
            if (payloadCustom.EntityList != null && payloadCustom.EntityList.Count > 0)
            {
                payloadCustom.MessageType = Common.MessageType.Success.ToString();
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = Response.StatusCode;
            }
            else
            {
                payloadCustom.MessageList.Add("Message", "Something went wrong");
                payloadCustom.IsSuccess = true;
                payloadCustom.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadCustom);
        }
    }
}
