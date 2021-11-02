using Microsoft.AspNetCore.Mvc;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace OKRAdminService.WebCore.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrganisationController : ApiControllerBase
    {
        private readonly IOrganisationService _organizationService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public OrganisationController(IIdentityService identityService,IOrganisationService organisationService, IUserService userServices, IRoleService roleServices, IPermissionService permissionServices) : base(identityService)
        {
            _organizationService = organisationService;
            _userService = userServices;
            _roleService = roleServices;
            _permissionService = permissionServices;
        }

        [HttpPost]
        [Route("AddOrganisation")]
        public async Task<IActionResult> CreateOrganisationsAsync([Required][FromBody] OrganisationRequest request)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (string.IsNullOrEmpty(request.OrganisationName))
                ModelState.AddModelError("OrganisationName", "Organisation name can't be blank");
            else
            {
                var isOrganisationExist = await _organizationService.GetOrganisationByNameAsync(request.OrganisationName);
                if (isOrganisationExist != null)
                    ModelState.AddModelError("OrganisationName", "Organisation with the same name already exist in our database.");
            }

            if (request.CycleDuration < 1)
                ModelState.AddModelError("CycleDuration", "Organisation CycleDuration cannot be 0");

            if (request.OrganisationLeader > 0)
            {
                var userExist = await _userService.GetUserByEmployeeIdAsync(request.OrganisationLeader);
                if (userExist == null)
                    ModelState.AddModelError("OrganisationLeader", "Organisation leader does not exist in our database.");
            }
            if (request.CycleStartDate != null)
            {
                bool cycleOk = await _organizationService.DoesCycleFallsInFutureDate(request);
                if (!cycleOk)
                    ModelState.AddModelError("CycleStartDate", "Selected cycle timeline already passed, Please select appropriate cycle start date.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.AddOrganisationAsync(request, Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Error", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpGet]
        [Route("GetOrganisationById")]
        public async Task<IActionResult> GetOrganisationByIdAsync([Required] long organisationId)
        {
            var payload = new PayloadCustom<OrganisationDetail>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (organisationId <= 0)
                ModelState.AddModelError("organisationId", "Organisation id not valid");
            else if (organisationId > 0)
            {
                var org = await _organizationService.GetOrganisationAsync(organisationId);
                if (org == null)
                    ModelState.AddModelError("Organisation", "Organisation does not exist or inactive.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.GetOrganisationByIdAsync(organisationId);
                if (payload.Entity != null)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = Response.StatusCode;
                }
                else
                {
                    payload.MessageList.Add("Organisation", $"Organisation does not exist with organisation Id- {organisationId}");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpGet]
        [Route("GetObjectives")]
        public async Task<IActionResult> GetOrganisationObjectives([Required] long organisationId)
        {
            var payload = new PayloadCustom<OrganisationObjectives>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payload.Entity = await _organizationService.GetObjectivesByOrgIdAsync(organisationId);
            if (payload.Entity != null)
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = Response.StatusCode;
            }
            else
            {
                payload.MessageList.Add("Organisation", $"Organisation does not exist with organisation Id- {organisationId}");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }


        [HttpGet]
        [Route("SearchOrganisation")]
        public async Task<IActionResult> SearchOrganisationAsync([Required] string organisationName)
        {
            var payload = new PayloadCustom<OrganisationSearch>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payload.EntityList = await _organizationService.SearchOrganisationAsync(organisationName);
            if (payload.EntityList != null)
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("Organisation", $"Organisation does not exist with organisation name-  {organisationName}");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }

        [HttpGet]
        [Route("GetCurrentCycle")]
        public async Task<IActionResult> GetCurrentOrganisationCycleAsync([Required] long organisationId)
        {
            var payload = new PayloadCustom<OrganisationCycleResponse>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payload.Entity = await _organizationService.GetCurrentCycleAsync(organisationId);
            if (payload.Entity != null)
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = Response.StatusCode;
            }
            else
            {
                payload.MessageList.Add("OrganisationCycle", $"Organisation cycle does not exist for organisation id- {organisationId}");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }

        [HttpPut]
        [Route("EditOrganisation")]
        public async Task<IActionResult> UpdateOrganisationAsync([Required][FromBody] OrganisationRequest request)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (string.IsNullOrEmpty(request.OrganisationName))
                ModelState.AddModelError("OrganisationName", "Organisation name can't be blank");
            if (request.OrganisationId <= 0)
                ModelState.AddModelError("OrganisationId", "Organisation id is not valid");

            if (!string.IsNullOrEmpty(request.OrganisationName) && request.OrganisationId > 0)
            {
                var org = await _organizationService.GetOrganisationByNameAsync(request.OrganisationName, request.OrganisationId);
                if (org != null)
                    ModelState.AddModelError("OrganisationName", "Organisation with the same name already exist in our database.");
            }
            if (request.OrganisationLeader > 0)
            {
                var userExist = await _userService.GetUserByEmployeeIdAsync(request.OrganisationLeader);
                if (userExist == null)
                    ModelState.AddModelError("OrganisationLeader", "Organisation leader does not exist in our database.");
            }
            if (request.CycleStartDate != null)
            {
                bool cycleOk = await _organizationService.DoesCycleFallsInFutureDate(request);
                if (!cycleOk)
                    ModelState.AddModelError("CycleStartDate", "Selected cycle timeline already passed, Please select appropriate cycle start date.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.UpdateOrganisationAsync(request, Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("GetOrganisationCycleDetails")]
        public async Task<IActionResult> GetOrganisationCycleDetailsAsync([Required] long organisationId)
        {
            var payload = new PayloadCustom<OrganisationCycleDetails>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (organisationId <= 0)
                ModelState.AddModelError("OrganisationId", "Organisation id is not valid");
            else
            {
                var org = await _organizationService.GetOrganisationAsync(organisationId);
                if (org == null)
                    ModelState.AddModelError("Organisation", "Organisation does not exist in our database or inactive.");

            }
            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.GetOrganisationCycleDetailsAsync(organisationId);
                if (payload.Entity != null)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("Organisation", $"Organisation cycle does not exist for organisation id- {organisationId}");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpGet]
        [Route("GetImportOkrCycle")]
        public async Task<IActionResult> GetImportOkrCycleAsync([Required] long organisationId, [Required] long currentCycleId, [Required] int cycleYear)
        {
            var payload = new PayloadCustom<ImportOkrCycle>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payload.EntityList = await _organizationService.GetImportOkrCycleAsync(organisationId, currentCycleId, cycleYear);
            if (payload.EntityList != null)
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("Organisation", $"No cycle found against organisationId {organisationId}, cycleId {cycleYear} and year {cycleYear}");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("GetAllActiveOrganisations")]
        public async Task<IActionResult> GetAllOrganisationsAsync()
        {
            var payload = new PayloadCustom<ActiveOrganisations>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payload.EntityList = await _organizationService.GetAllOrganisationsAsync();
            if (payload.EntityList != null && payload.EntityList.Any())
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("Organisation", $"No organisation found");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payload);
        }

        [HttpPut]
        [Route("UndoChangesForOrganisation")]
        public async Task<IActionResult> UndoChangesForOrganisationAsync([Required] long organisationId)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);


            payload.Entity = await _organizationService.UndoChangesForOrganisationAsync(organisationId);
            if (payload.Entity.Success)
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("Message", $"Something went wrong");
                payload.IsSuccess = false;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }

        [HttpPost]
        [Route("AddChildOrganisation")]
        public async Task<IActionResult> AddChildOrganisationAsync([Required] ChildRequest request)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (request.ParentOrganisationId <= 0)
                ModelState.AddModelError("ParentOrganisationId", "Parent organisation id is invalid");

            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(request.ParentOrganisationId);
                if (orgExist == null)
                    ModelState.AddModelError("Organisation", "Parent organisation does not exist in our database.");
            }
            if (string.IsNullOrEmpty(request.ChildOrganisationName))
                ModelState.AddModelError("ChildName", "Organisation name can't be blank");
            else
            {
                var isOrganisationExist = await _organizationService.GetOrganisationByNameAsync(request.ChildOrganisationName);
                if (isOrganisationExist != null)
                    ModelState.AddModelError("ChildName", "Organisation with the same name already exists in our database.");
            }

            if (request.LeaderId > 0)
            {
                var userExist = await _userService.GetUserByEmployeeIdAsync(request.LeaderId);
                if (userExist == null)
                    ModelState.AddModelError("OrganisationLeader", "Organisation leader does not exist in our database.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.AddChildOrganisationAsync(request, Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpPut]
        [Route("EditChildOrganisation")]
        public async Task<IActionResult> EditChildOrganisationAsync([Required] ChildRequest request)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (request.ParentOrganisationId <= 0)
                ModelState.AddModelError("ParentOrganisationId", "Parent organisation id is invalid");

            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(request.ParentOrganisationId);
                if (orgExist == null)
                    ModelState.AddModelError("Organisation", "Parent organisation does not exist in our database.");
            }
            if (string.IsNullOrEmpty(request.ChildOrganisationName))
                ModelState.AddModelError("ChildName", "Organisation name can't be blank");
            else
            {
                var org = await _organizationService.GetOrganisationByNameAsync(request.ChildOrganisationName, request.ChildOrganisationId);
                if (org != null)
                    ModelState.AddModelError("ChildName", "Organisation with the same name already exists in our database.");

            }

            if (request.LeaderId > 0)
            {
                var userExist = await _userService.GetUserByEmployeeIdAsync(request.LeaderId);
                if (userExist == null)
                    ModelState.AddModelError("OrganisationLeader", "Organisation leader does not exist in our database.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.UpdateChildOrganisation(request, Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpPost]
        [Route("DetachChildFromParent")]
        public async Task<IActionResult> DetachChildOrganisationFromParentOrganisationAsync([Required] long organisationId)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (organisationId <= 0)
                ModelState.AddModelError("OrganisationId", "Organisation id is invalid");
            else if (organisationId > 0)
            {
                var orgaisation = await _organizationService.GetOrganisationAsync(organisationId);
                if (orgaisation == null)
                    ModelState.AddModelError("Organisation", "Organisation does not exist in our database.");
            }
            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.DetachChildOrganisationFromParentOrganisationAsync(organisationId, Convert.ToInt64(token.EmployeeId));
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpPost]
        [Route("AddParentToParent")]
        public async Task<IActionResult> AddParentToParentOrganisationAsync([Required] ParentRequest request)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (request.OrganisationId <= 0)
                ModelState.AddModelError("OrganisationId", "Organisation id is invalid");
            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(request.OrganisationId);
                if (orgExist == null)
                    ModelState.AddModelError("Organisation", "Organisation does not exist in our database.");
            }
            if (string.IsNullOrEmpty(request.ParentName))
                ModelState.AddModelError("ParentName", "ParentName can't be blank");

            if (request.LeaderId > 0)
            {
                var userExist = await _userService.GetUserByEmployeeIdAsync(request.LeaderId);
                if (userExist == null)
                    ModelState.AddModelError("OrganisationLeader", "Organisation leader does not exist in our database.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.AddParentToParentOrganisationAsync(request, token.EmployeeId);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.InternalServerError;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }


        [HttpPost]
        [Route("AddChildToParent")]
        public async Task<IActionResult> AddChildOrganisationToParentOrganisationAsync([Required] long organisationId, [Required] long ChildId)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (organisationId <= 0)
                ModelState.AddModelError("OrganisationId", "Organisation id is invalid");
            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(organisationId);
                if (orgExist == null)
                    ModelState.AddModelError("Organisation", "Parent organisation does not exist in our database.");
            }
            if (ChildId <= 0)
                ModelState.AddModelError("ChildId", "Organisation Id is invalid");
            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(ChildId);
                if (orgExist == null)
                    ModelState.AddModelError("ChildOrganisation", "Child organisation does not exist in our database.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.AddChildOrganisationToParentOrganisationAsync(organisationId, ChildId, token.EmployeeId);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpPost]
        [Route("AddChildAsParentToParent")]
        public async Task<IActionResult> AddChildAsParentToParentOrganisationAsync([Required] long oldParentId, [Required] long newParentId)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (oldParentId <= 0)
                ModelState.AddModelError("OrganisationId", "Old organisation id is invalid");
            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(oldParentId);
                if (orgExist == null)
                    ModelState.AddModelError("Organisation", "Old organisation does not exist in our database.");
            }
            if (newParentId <= 0)
                ModelState.AddModelError("newParentId", "New organisation id is invalid");
            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(newParentId);
                if (orgExist == null)
                    ModelState.AddModelError("NewOrganisation", "New organisation does not exist in our database.");
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.AddChildAsParentToParentOrganisationAsync(oldParentId, newParentId, token.EmployeeId);
                if (payload.Entity != null && payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.InternalServerError;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [HttpDelete]
        [Route("DeleteOrganisation")]
        public async Task<IActionResult> DeleteOrganisation([Required] long organisationId)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (organisationId <= 0)
                ModelState.AddModelError("OrganisationId", "Organisation id is invalid");
            else
            {
                var orgExist = await _organizationService.GetOrganisationAsync(organisationId);
                if (orgExist == null)
                    ModelState.AddModelError("Organisation", "Organisation does not exist in our database.");
                else
                {
                    bool IschaildAvailabe = await _organizationService.HaveChildOrganisationsAsync(organisationId);
                    if (IschaildAvailabe)
                        ModelState.AddModelError("ChildExists", "You have child organization(s) under this organisation, You can't delete this organization. First delete the child");
                }
            }
            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.DeleteOrganisationAsync(organisationId, token.EmployeeId);
                if (payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                }
                payload.Status = (int)HttpStatusCode.OK;
                return Ok(payload);
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);


        }

        [HttpPost]
        [Route("UploadLogo")]
        public async Task<IActionResult> UploadLogoOnAzure([Required] IFormFile file)
        {
            var payload = new PayloadCustom<string>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            string filExtension = System.IO.Path.GetExtension(file.FileName);
            if (filExtension.ToLower() == ".exe" || filExtension.ToLower() == ".com" || filExtension.ToLower() == ".bat"
                || filExtension.ToLower() == ".msi" || filExtension.ToLower() == ".dll" || filExtension.ToLower() == ".class" || filExtension.ToLower() == ".jar")
            {
                ModelState.AddModelError("file", "Please upload only jpg or png file format.");
            }
            else if (file.Length == 0)
            {
                ModelState.AddModelError("file", "Please upload file.");
            }

            if (ModelState.IsValid)
            {
                if (filExtension.ToLower() == ".jpg" || filExtension.ToLower() == ".png"
                   || filExtension.ToLower() == ".jpeg" || filExtension.ToLower() == ".svg")
                {
                    payload.Entity = await _organizationService.UploadLogoOnAzure(file);
                    payload.MessageType = MessageType.Success.ToString();
                    payload.MessageList.Add("file", "File is successfully uploaded to S3");
                    payload.IsSuccess = true;
                    payload.Status = Response.StatusCode;
                }
                else
                {
                    payload.IsSuccess = false;
                    payload.MessageList.Add("file", "Please upload only jpg or png or jpeg or svg file format.");
                    payload.Status = (int)HttpStatusCode.BadRequest;
                }

            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("TeamDetails")]
        public async Task<IActionResult> GetUsersTeamDetailsAsync([Required] int goalType, long empId, bool isCoach)
        {
            var payload = new PayloadCustom<TeamDetails>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (goalType <= 0 || goalType > 2)
                ModelState.AddModelError("GoalType", "Goal type is invalid");

            if (isCoach)
            {
                var userRole = empId > 0 ? await _roleService.GetRolesByUserIdAsync(empId) : await _roleService.GetRolesByUserIdAsync(token.EmployeeId);
                var permissions = await _permissionService.GetPermissionsByRoleIdAsync(userRole.RoleId);
                if (permissions.Count > 0 && permissions.Any(x => x.ModuleName == "MyGoal" && x.Permissions.Any(y => y.PermissionName == "Coach" && !y.Status)))
                {
                    ModelState.AddModelError("ISCoach", "You don't have the coach rights.");
                }
            }

            if (ModelState.IsValid)
            {
                payload.EntityList = await _organizationService.GetUsersTeamDetailsAsync(token.EmployeeId, goalType, empId, isCoach);
                if (payload.EntityList != null && payload.EntityList.Any())
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("Team", $"No team found");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("TeamDetailsById")]
        public async Task<IActionResult> GetTeamDetailsByIdAsync([Required] long teamId)
        {
            var payload = new PayloadCustom<SubTeamDetails>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (teamId <= 0)
                ModelState.AddModelError("TeamId", "Team id is invalid");

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.GetTeamDetailsByIdAsync(teamId);
                if (payload.Entity != null && payload.Entity.OrganisationId != 0)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("Team", $"No team found");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("TeamEmployeesDetails")]
        public async Task<IActionResult> GetTeamDetailsAsync()
        {
            var payload = new PayloadCustom<SubTeamDetails>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            payload.EntityList = await _organizationService.GetTeamDetailsAsync();
            if (payload.EntityList != null && payload.EntityList.Any())
            {
                payload.MessageType = MessageType.Success.ToString();
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payload.MessageList.Add("Teams", $"No teams found");
                payload.IsSuccess = true;
                payload.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payload);
        }

        [HttpGet]
        [Route("DirectReportsById")]
        public async Task<IActionResult> GetDirectReportsByIdAsync([Required] long employeeId)
        {
            var payload = new PayloadCustom<DirectReportsDetails>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (employeeId <= 0)
                ModelState.AddModelError("EmployeeId", "employee id is invalid");

            if (ModelState.IsValid)
            {
                payload.EntityList = await _organizationService.GetDirectReportsByIdAsync(employeeId);
                if (payload.Entity != null)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("DirectReport", $"No Direct Report Found");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("ColorCodes")]
        public async Task<IActionResult> GetOrganizationColorCodes()
        {
            var payload = new PayloadCustom<ColorCodesResponse>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (ModelState.IsValid)
            {
                payload.EntityList = await _organizationService.GetOrganizationColorCodesAsync();
                if (payload.EntityList != null)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("Organization", $"No Organization Found");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpPut]
        [Route("EditOrganisationColor")]
        public async Task<IActionResult> UpdateOrganisationColorAsync()
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.UpdateOrganisationColorAsync(Convert.ToInt64(token.EmployeeId), UserToken);
                if ((payload.Entity != null && payload.Entity.Success) || payload.Entity.RecordsAffected == 0)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("Message", "Something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("OrganizationByEmployeeId")]
        public async Task<IActionResult> GetOrganizationByEmployeeId(long employeeId)
        {
            var payload = new PayloadCustom<EmployeeOrganizationDetails>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.GetOrganizationDetailsByEmployeeId(employeeId);
                if (payload.Entity != null)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("Organization", $"No Organization Found");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }

        [HttpGet]
        [Route("GetLicenceDetail")]
        public async Task<IActionResult> GetLicenceDetail()
        {
            var payload = new PayloadCustom<LicenseDetail>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (ModelState.IsValid)
            {
                payload.Entity = await _organizationService.GetLicenceDetail(UserToken);
                if (payload.Entity != null)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
                }
                else
                {
                    payload.MessageList.Add("Organization", $"No licence Found");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);
        }
    }
}
