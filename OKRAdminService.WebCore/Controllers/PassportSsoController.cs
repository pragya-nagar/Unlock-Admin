using Microsoft.AspNetCore.Mvc;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace OKRAdminService.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassportSsoController : ApiControllerBase
    {
        private readonly IPassportSsoService _passportSsoService;

        public PassportSsoController(IIdentityService identityService, IPassportSsoService passportSsoService) : base(identityService)
        {
            _passportSsoService = passportSsoService;
        }

        [Route("SsoAuthentication")]
        [HttpPost]
        public async Task<IActionResult> SsoLoginAsync([Required] SsoLoginRequest ssoLoginRequest)
        {
            var payload = new PayloadCustom<UserLoginResponse>();

            if (string.IsNullOrEmpty(ssoLoginRequest.AppId))
                ModelState.AddModelError("appId", "AppId is required");

            if (string.IsNullOrEmpty(ssoLoginRequest.SessionId))
                ModelState.AddModelError("sessionId", "SessionId is required");

            if (ModelState.IsValid)
            {
                var record = await _passportSsoService.SsoLoginAsync(ssoLoginRequest);
                if (string.IsNullOrEmpty(record.TokenId))
                {
                    payload.MessageList.Add("userName", "Invalid Username or Password ");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.Unauthorized;
                }
                else
                {
                    payload.Entity = record;
                    payload.MessageType = Common.MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }


        [Route("ActiveUserSync")]
        [HttpPost]
        public async Task<IActionResult> ActiveUserAsync()
        {
            var payload = new PayloadCustom<List<PassportEmployeeResponse>>();

            payload.Entity = await _passportSsoService.ActiveUserAsync();
            if (payload.Entity != null && payload.Entity.Count > 0)
            {
                payload.MessageList.Add("user", "Users are added/updated successfully in the system.");
                payload.MessageType = Common.MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("user", "There is no active user to add/update in the system.");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }          
            return Ok(payload);
        }

        [Route("InActiveUserSync")]
        [HttpPost]
        public async Task<IActionResult> InActiveUserAsync()
        {
            var payload = new PayloadCustom<List<PassportEmployeeResponse>>();

            payload.Entity = await _passportSsoService.InActiveUserAsync();
            if (payload.Entity != null && payload.Entity.Count > 0)
            {
                payload.MessageList.Add("user", "Users are updated successfully in the system.");
                payload.MessageType = Common.MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("user", "There is no inactive user to update in the system.");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }

        [Route("GetAllPassportUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAllPassportUsersAsync()
        {
            var payload = new PayloadCustom<List<PassportEmployeeResponse>>();

            payload.Entity = await _passportSsoService.GetAllPassportUsersAsync();
            if (payload.Entity != null && payload.Entity.Count > 0)
            {
                payload.MessageType = Common.MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("user", "No user found in the passport system.");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }
    }
}
