using Microsoft.AspNetCore.Mvc;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels.Response;
using Serilog;
using System.Linq;
using System.Net;

namespace OKRAdminService.WebCore.Controllers
{
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        private readonly IIdentityService _identityService;
        protected ILogger Logger { get; set; }
        public ApiControllerBase(IIdentityService identityService)
        {
            _identityService = identityService;
            Logger = Log.Logger;
        }
        protected string LoggedInUserEmail => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

        protected string UserToken => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "token")?.Value;

        protected bool IsTokenActive => (!string.IsNullOrEmpty(LoggedInUserEmail) && !string.IsNullOrEmpty(UserToken));
        protected string TenantId => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value;
        protected IdentityResponse GetTokenDetails()
        {
            IdentityResponse response = new IdentityResponse();
            if (IsTokenActive)
            {
                var identity = _identityService.GetUser(LoggedInUserEmail);
                if (identity != null)
                {
                    response.IsTokenActive = IsTokenActive;
                    response.UserToken = UserToken;
                    response.EmailId = identity.EmailId;
                    response.FirstName = identity.FirstName;
                    response.LastName = identity.LastName;
                    response.EmployeeId = identity.EmployeeId;
                    response.EmployeeCode = identity.EmployeeCode;
                }
            }
            return response;
        }

        public PayloadCustom<T> GetPayloadStatus<T>(PayloadCustom<T> payload)
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    if (!payload.MessageList.ContainsKey(state.Key))
                    {
                        payload.MessageList.Add(state.Key, error.ErrorMessage);
                    }
                }
            }
            payload.IsSuccess = false;
            payload.Status = (int)HttpStatusCode.BadRequest;
            return payload;
        }
    }
}