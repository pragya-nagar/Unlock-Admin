using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace OKRAdminService.WebCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ApiControllerBase
    {
        private readonly IUserService userService;
        private readonly IRoleService roleService;
        private readonly IOrganisationService organisationService;
        private readonly IConfiguration configuration;
        public UserController(IIdentityService identityService, IRoleService roles, IOrganisationService organisationServices, IUserService userServices, IConfiguration configurations) : base(identityService)
        {
            userService = userServices;
            roleService = roles;
            organisationService = organisationServices;
            configuration = configurations;

        }

        [Route("AddUser")]
        [HttpPost]
        public async Task<IActionResult> AddUserAsync([Required] UserRequestModel userRequestModel)
        {
            var payloadAdd = new PayloadCustom<UserRequestModel>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);
            Dictionary<string, string> errors = new Dictionary<string, string>();
            string domain;
            var hasOrigin = Request.Headers.TryGetValue("OriginHost", out var origin);
            if ((!hasOrigin && Request.Host.Value.Contains("localhost")))
                domain = new Uri(configuration.GetValue<string>("FrontEndUrl")).Host;
            else if (!hasOrigin)
            {
                errors.Add("Domain", "Domain is required");
                payloadAdd.MessageList.Add(errors.First().Key, errors.First().Value);
                payloadAdd.IsSuccess = false;
                payloadAdd.Status = (int)HttpStatusCode.BadRequest;
                return Ok(payloadAdd);
            }
            else
            {
                domain = new Uri(origin).Host;
            }

            errors = ValidateUserDetails(userRequestModel);

            if (errors.Count == 0)
            {
                var response = await userService.IsUserExistInAdAsync(userRequestModel.EmailId);
                if (response.IsExist)
                {
                    var emailExists = await userService.GetUserByMailIdAsync(response.EmailId);
                    if (emailExists != null)
                        ModelState.AddModelError("userName", "User already exists");
                }
            }
            if (errors.Count == 0)
            {
                var result = await userService.AddUserAsync(userRequestModel, Convert.ToInt64(token.EmployeeId), domain);
                payloadAdd.Entity = result;
                if (payloadAdd.Entity != null)
                {
                    payloadAdd.MessageList.Add("userId", "User details are saved successfully.");
                    payloadAdd.IsSuccess = true;
                    payloadAdd.Status = Response.StatusCode;
                }
                else
                {
                    payloadAdd.MessageList.Add("userId", "No User details saved ");
                    payloadAdd.IsSuccess = true;
                    payloadAdd.Status = (int)HttpStatusCode.NoContent;
                }
            }

            else
            {
                payloadAdd.MessageList.Add(errors.First().Key, errors.First().Value);
                payloadAdd.IsSuccess = false;
                payloadAdd.Status = (int)HttpStatusCode.BadRequest;

            }

            return Ok(payloadAdd);
        }

        [Route("EditUser")]
        [HttpPut]
        public async Task<IActionResult> EditUserAsync([Required] UserRequestModel userRequestModel)
        {
            var payloadUpdate = new PayloadCustom<Employee>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var errors = ValidateUserDetails(userRequestModel);
            if (userRequestModel.EmployeeId <= 0)
                errors.Add("EmployeeId", "Employee id is not valid");
            else if (userRequestModel.EmployeeId > 0)
            {
                var employee = await userService.GetUserByEmployeeIdAsync(userRequestModel.EmployeeId);
                if (employee == null)
                    errors.Add("employee", "Employee does not exist in our database.");

                if (!string.IsNullOrEmpty(userRequestModel.EmployeeCode))
                {
                    var userEmp = await userService.GetUserByEmployeeCodeAsync(userRequestModel.EmployeeCode, userRequestModel.EmployeeId);
                    if (userEmp != null)
                        errors.Add("employeeCode", "User with same employee code already exists.");
                }
                if (!string.IsNullOrEmpty(userRequestModel.EmailId))
                {
                    var userEmail = await userService.GetUserByEmailIdAsync(userRequestModel.EmailId, userRequestModel.EmployeeId);
                    if (userEmail != null)
                        errors.Add("emailId", "User with same email id already exists.");
                }
                if (employee != null && userRequestModel.ReportingTo > 0)
                {
                    if (employee.EmployeeId == userRequestModel.ReportingTo)
                        errors.Add("reportingTo", "Same user can't report to himself or herself.");
                }
            }
            if (errors.Count == 0)
            {
                var result = await userService.EditUserAsync(userRequestModel, Convert.ToInt64(token.EmployeeId));
                if (result.Success)
                {
                    payloadUpdate.Entity = result.Entity;
                    payloadUpdate.IsSuccess = true;
                    payloadUpdate.Status = Response.StatusCode;
                }
            }
            else
            {
                payloadUpdate.MessageList.Add(errors.First().Key, errors.First().Value);
                payloadUpdate.IsSuccess = false;
                payloadUpdate.Status = (int)HttpStatusCode.BadRequest;
            }
            return Ok(payloadUpdate);
        }

        [Route("MultiSearchUserList")]
        [HttpPost]
        public async Task<IActionResult> MultiSearchUserListAsync(List<string> searchTexts, int pageIndex = 1, int pageSize = 10)
        {
            var payloadGet = new PayloadCustom<PageResults<AllUsersResponse>>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (pageSize == 0)
                ModelState.AddModelError("PageSize", "Request parameters are not valid");

            if (ModelState.IsValid)
            {

                var data = await userService.MultiSearchUserListAsync(token.UserToken, searchTexts, pageIndex, pageSize);
                if (data != null && data.TotalRecords >= 0)
                {
                    payloadGet.Entity = data;
                    payloadGet.MessageType = Common.MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("SearchTexts", "No employee exists with search values");
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payloadGet);
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }

            return Ok(payloadGet);
        }

        [Route("GetUsersById")]
        [HttpGet]
        public async Task<IActionResult> GetUserByIdAsync([Required] long empId)
        {
            var payloadGet = new PayloadCustom<UserDetails>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var record = await userService.GetUserByEmpIdAsync(empId);
            if (record != null)
            {
                payloadGet.Entity = record;
                payloadGet.MessageType = Common.MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("finder", "No employee exists");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payloadGet);
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([Required] LoginRequest loginRequest)
        {
            var payload = new PayloadCustom<UserLoginResponse>();
            if (string.IsNullOrWhiteSpace(loginRequest.UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
                ModelState.AddModelError("userName", "Username or Password cannot be empty or whitespace.");
            else if (!string.IsNullOrEmpty(loginRequest.UserName))
            {
                var userDetail = await userService.GetUserByMailIdAsync(loginRequest.UserName);
                if (userDetail == null)
                    ModelState.AddModelError("userName", "User does not exist in our database or must be inactive.");
                else if (userDetail.EmployeeId > 0)
                {
                    var role = await roleService.GetRolesByUserIdAsync(userDetail.EmployeeId);
                    if (role == null)
                        ModelState.AddModelError("Role", "No role assigned to this user.");

                    else if (userDetail.LoginFailCount == 5)
                    {
                        ModelState.AddModelError("userName", "Invalid Username or Password. For security, your account is locked. Now you can only reset your password.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                payload.Entity = await userService.LoginAsync(loginRequest);
                if (payload.Entity == null || string.IsNullOrEmpty(payload.Entity.TokenId))
                {
                    if (payload.Entity.LoginFailCount == 4)
                    {
                        payload.MessageList.Add("userName", "Invalid Username or Password. For security, you'll have one more attempt before your account is locked.");
                    }
                    else if (payload.Entity.LoginFailCount == 5)
                    {
                        payload.MessageList.Add("userName", "Invalid Username or Password. For security, your account is locked. Now you can only reset your password.");
                    }
                    else
                    {
                        payload.MessageList.Add("userName", "Invalid Username or Password ");
                    }
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.BadRequest;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.OK;
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
        [Route("LogOut")]
        public ActionResult LogOut()
        {
            var payload = new PayloadCustom<TokenResponse>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            userService.Logout(token.UserToken, Convert.ToInt64(token.EmployeeId));
            payload.MessageList.Add("TokenId", "Log out Successful.");
            payload.MessageType = MessageType.Success.ToString();
            payload.IsSuccess = true;
            payload.Status = Response.StatusCode;

            return Ok(payload);
        }

        [HttpGet]
        [Route("GetAllusers")]
        public async Task<IActionResult> GetAllUsersAsync(int pageIndex = 1, int pageSize = 10)
        {
            var payload = new PayloadCustomList<PageResults<AllUsersResponse>>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (pageSize == 0)
                ModelState.AddModelError("PageSize", "Requested parameters are not valid");
            if (ModelState.IsValid)
            {

                var result = await userService.GetAllUsersAsync(pageIndex, pageSize);
                if (result.Records != null && result.Records.Count > 0)
                {
                    payload.Entity = result;
                    payload.MessageType = Common.MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = Response.StatusCode;
                    return Ok(payload);
                }
                else
                {
                    payload.MessageList.Add("employee", "No employee exists");
                    payload.IsSuccess = true;
                    payload.Status = (int)HttpStatusCode.NoContent;
                    return Ok(payload);
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {

                        payload.MessageList.Add(state.Key, error.ErrorMessage);
                    }
                }
                payload.IsSuccess = false;
                payload.Status = (int)HttpStatusCode.BadRequest;
            }
            return Ok(payload);
        }

        [HttpGet]
        [Route("Search")]
        public async Task<ActionResult> SearchEmployee(string finder, int page = 1, int pagesize = 5)
        {
            var payloadGet = new PayloadCustomGenric<SearchUserList>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);
            if(page == 0)
                ModelState.AddModelError("Page", "Requested parameters are not valid");
            if(pagesize == 0)
                ModelState.AddModelError("PageSize", "Requested parameters are not valid");
            if(string.IsNullOrEmpty(finder))
                ModelState.AddModelError("Finder", "Search cannot be blank");

            if(ModelState.IsValid)
            {
                var data = await userService.SearchEmployee(finder, page, pagesize, Convert.ToInt64(token.EmployeeId));
                if (data.Records.Count > 0)
                {
                    payloadGet.EntityList = data.Records;
                    payloadGet.PaggingInfo = data.PaggingInfo;
                    payloadGet.MessageType = MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("finder", "No employee exists");
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {

                        payloadGet.MessageList.Add(state.Key, error.ErrorMessage);
                    }
                }
                payloadGet.IsSuccess = false;
                payloadGet.Status = (int)HttpStatusCode.BadRequest;
            }
            return Ok(payloadGet);

        }

        [HttpPost]
        [Route("Identity")]
        public async Task<IActionResult> Identity()
        {
            var payloadGet = new PayloadCustom<LoginUserDetails>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var record = await userService.Identity(token.EmployeeId, token.UserToken);
            if (record != null)
            {
                payloadGet.Entity = record;
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("finder", "No employee exists");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payloadGet);
        }

        [HttpPost]
        [Route("UserByToken")]
        public async Task<IActionResult> UserByToken()
        {
            var payloadGet = new PayloadCustom<UserLoginResponse>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);
            string domain = string.Empty;
            var hasOrigin = Request.Headers.TryGetValue("OriginHost", out var origin);
            if ((!hasOrigin && Request.Host.Value.Contains("localhost")))
                domain = new Uri(configuration.GetValue<string>("FrontEndUrl")).Host;
            else if (!hasOrigin)
                domain = string.Empty;
            else
                domain = new Uri(origin).Host;
            if (string.IsNullOrEmpty(domain))
            {

                payloadGet.MessageList.Add("Domain", "Domain is required");
                payloadGet.IsSuccess = false;
                payloadGet.Status = (int)HttpStatusCode.BadRequest;
                return Ok(payloadGet);
            }
            var record = await userService.UserByToken(LoggedInUserEmail, domain);
            if (record != null)
            {
                payloadGet.Entity = record;
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("finder", "No employee exists");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payloadGet);
        }

        [HttpPost]
        [Route("GetIdentity")]
        public async Task<IActionResult> GetIdentity(string userEmail)
        {
            var payloadGet = new PayloadCustom<UserLoginResponse>();
            var record = await userService.GetIdentity(userEmail);
            if (record != null)
            {
                payloadGet.Entity = record;
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("finder", "No employee exists");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payloadGet);
        }



        [Route("DeleteUser")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUserAsync([Required] List<long> employeeIdList)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (employeeIdList == null || employeeIdList.Count <= 0)
                ModelState.AddModelError("EmployeeId", "Please supply atleast one record to delete.");
            else
            {
                var deletedUser = employeeIdList.Where(x => x == Convert.ToInt64(token.EmployeeId));
                if (deletedUser.Any())
                    ModelState.AddModelError("EmployeeId", "You cannot delete your own record");

                var employeeExists = await userService.GetReportingToOrganisationHeadAsync(employeeIdList);
                if (employeeExists != null)
                    ModelState.AddModelError("EmployeeId", employeeExists.FirstName + " " + employeeExists.LastName + " with EmployeeId " + employeeExists.EmployeeCode + " is having users Reporting or an Organisation Head, so please change the reporting/organisation head to delete this record.");
            }

            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.DeleteUserAsync(employeeIdList, Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (payloadGet.Entity.Success)
                {
                    payloadGet.IsSuccess = true;
                }
                else
                {
                    payloadGet.MessageList.Add("EmpId", "No record deleted for the employee.");
                    payloadGet.IsSuccess = false;
                }
                payloadGet.Status = (int)HttpStatusCode.OK;
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }

        [HttpPost]
        [Route("UploadBulkUser")]
        public async Task<IActionResult> UploadBulkUserAsync([Required] IFormFile formFile)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            string filExtension = System.IO.Path.GetExtension(formFile.FileName);

            if (formFile.Length == 0)
                ModelState.AddModelError("FormFile", "Please upload file");

            string domain = "";
            var hasOrigin = Request.Headers.TryGetValue("OriginHost", out var origin);
            if ((!hasOrigin && Request.Host.Value.Contains("localhost")))
                domain = new Uri(configuration.GetValue<string>("FrontEndUrl")).Host;
            else if (!hasOrigin)
            {
                ModelState.AddModelError("Domain", "Domain is required");
                payloadGet.MessageList.Add("Domain", "Domain is required");
                payloadGet.IsSuccess = false;
                payloadGet.Status = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                domain = new Uri(origin).Host;
            }


            if (ModelState.IsValid)
            {
                if (filExtension != null && (filExtension.ToLower() == ".csv" || filExtension.ToLower() == ".xlsx"))
                {
                    payloadGet.Entity = await userService.UploadBulkUserAsync(formFile, Convert.ToInt64(token.EmployeeId), token.UserToken, domain);
                    if (payloadGet.Entity.Success)
                    {
                        payloadGet.MessageList.Add("EmpId", "Employee records added successfully.");
                        payloadGet.IsSuccess = true;
                        payloadGet.Status = Response.StatusCode;
                    }
                    else
                    {
                        payloadGet.MessageList.Add("empId", "No record added for the employee.");
                        payloadGet.IsSuccess = true;
                        payloadGet.Status = (int)HttpStatusCode.NoContent;

                    }
                }
                else
                {
                    payloadGet.MessageList.Add("FormFile", "Please upload .csv or .xlsx file format only");
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = (int)HttpStatusCode.BadRequest;

                }

            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }

        [HttpGet]
        [Route("DownloadCSV")]
        public async Task<IActionResult> DownloadCsvAsync()
        {
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var result = await userService.DownloadCsvAsync();

            return File(Encoding.UTF8.GetBytes(result), "text/csv", "EmployeeInfo.csv");
        }

        [Route("ChangeRole")]
        [HttpPut]
        public async Task<IActionResult> ChangeRoleAsync([Required] ChangeRoleRequestModel changeRoleRequestModel)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (changeRoleRequestModel == null)
                ModelState.AddModelError("changeRoleRequestModel", "Requested parameters are not valid");

            else
            {
                if (changeRoleRequestModel.EmployeeIds == null || changeRoleRequestModel.EmployeeIds.Count <= 0)
                    ModelState.AddModelError("employeeId", "Users are required to assign the role");

                if (changeRoleRequestModel.NewRoleId <= 0)
                    ModelState.AddModelError("newRoleId", "Requested role is not valid");
            }

            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.ChangeRoleAsync(changeRoleRequestModel, Convert.ToInt64(token.EmployeeId));
                if (payloadGet.Entity != null && payloadGet.Entity.Success)
                {
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Success.ToString();
                }
                else
                {
                    payloadGet.MessageList.Add("EmployeeId", "No role changed for the employee");
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Info.ToString();
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }


        [HttpPut]
        [Route("ChangeUserReporting")]
        public async Task<IActionResult> ChangeUserReporting([Required] EditUserReportingRequest reportingRequest)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (reportingRequest == null)
            {
                ModelState.AddModelError("Request", "Requested parameters are not valid");
            }
            else
            {
                if (reportingRequest.NewReportingToId <= 0)
                    ModelState.AddModelError("NewReportingToId", "Requested reporting user is not valid");
                else
                {
                    var user = await userService.GetUserByEmployeeIdAsync(reportingRequest.NewReportingToId);
                    if (user == null)
                        ModelState.AddModelError("ReportingId", "Reporting user does not exist");
                }
                if (reportingRequest.EmployeeIds == null || reportingRequest.EmployeeIds.Count <= 0)
                    ModelState.AddModelError("EmployeeIds", "Users are required to assign the reporting");
            }

            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.ChangeUserReportingAsync(reportingRequest, Convert.ToInt64(token.EmployeeId));
                if (payloadGet.Entity != null && payloadGet.Entity.Success)
                {
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Success.ToString();
                    payloadGet.MessageList.Add("Message", "Reporting has been changed for given users");
                }
                else
                {
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Info.ToString();
                    payloadGet.MessageList.Add("Message", "Something went wrong");
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }

            return Ok(payloadGet);

        }

        [HttpPut]
        [Route("ChangeUserOrganisation")]
        public async Task<IActionResult> ChangeUserOrganisation([Required] ChangeUserOrganisationRequest changeUserOrganisationRequest)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (changeUserOrganisationRequest == null)
            {
                ModelState.AddModelError("Request", "Requested parameters are not valid");
            }
            else
            {
                if (changeUserOrganisationRequest.NewOrganisationId <= 0)
                    ModelState.AddModelError("NewOrganisationId", "Requested organisation is not valid");
                else
                {
                    var organisation = await organisationService.GetOrganisationAsync(changeUserOrganisationRequest.NewOrganisationId);
                    if (organisation == null)
                        ModelState.AddModelError("NewOrganisationId", "Requested organisation does not exist in our database or inactive");
                }
                if (changeUserOrganisationRequest.EmployeeIds == null || changeUserOrganisationRequest.EmployeeIds.Count <= 0)
                    ModelState.AddModelError("EmployeeIds", "Users are required to assign the organisation");
            }

            if (ModelState.IsValid)
            {
                var identity = await userService.GetUserByEmployeeIdAsync(token.EmployeeId);
                payloadGet.Entity = await userService.ChangeUserOrganisationAsync(changeUserOrganisationRequest, identity.EmployeeId, UserToken);
                if (payloadGet.Entity != null && payloadGet.Entity.Success)
                {
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Success.ToString();
                    payloadGet.MessageList.Add("Message", "Organisation has been changed for given users");
                }
                else
                {
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Info.ToString();
                    payloadGet.MessageList.Add("Message", "Something went wrong");
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }

            return Ok(payloadGet);

        }

        [HttpGet]
        [Route("Designation")]
        public async Task<IActionResult> GetDesignationAsync([Required] string designation)
        {
            var payloadGet = new PayloadCustom<string>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var result = await userService.GetDesignationAsync(designation);
            if (result.Count > 0)
            {

                payloadGet.EntityList = result;
                payloadGet.MessageType = Common.MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("designation", "No designation exists");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }
            return Ok(payloadGet);
        }

        [Route("ResetPassword")]
        [HttpPut]
        public async Task<IActionResult> ResetPasswordAsync([Required] ResetPasswordRequest resetPasswordRequest)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (resetPasswordRequest == null)
                ModelState.AddModelError("newPassword", "Requested parameter is required");

            else
            {
                if (string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
                    ModelState.AddModelError("newPassword", "New Password is required");

                else if (!string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
                {
                    string passwordRegex = AppConstants.StrongPassRegex;
                    Regex re = new Regex(passwordRegex);
                    if (!re.IsMatch(resetPasswordRequest.NewPassword))
                        ModelState.AddModelError("newPassword", "A strong password is at least 8 characters long and includes uppercase and lowercase letters, numbers and symbols.");
                }
            }

            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.ResetPasswordAsync(Convert.ToInt64(token.EmployeeId), resetPasswordRequest);
                if (payloadGet.Entity.Success)
                {
                    payloadGet.MessageType = Common.MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageList.Add("newPassword", "Password has been changed successfully.");
                }
                else
                {
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Info.ToString();
                    payloadGet.MessageList.Add("newPassword", "Something went wrong");
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }

        [Route("SendResetPasswordMail")]
        [HttpPost]
        public async Task<IActionResult> SendResetPasswordMailAsync([Required] SendResetPasswordMailRequest sendResetPasswordMailRequest)
        {
            var payloadGet = new PayloadCustom<bool>();

            if (sendResetPasswordMailRequest == null)
                ModelState.AddModelError("emailId", "Requested parameter is required");

            else
            {
                if (string.IsNullOrWhiteSpace(sendResetPasswordMailRequest.EmailId))
                    ModelState.AddModelError("emailId", "EmailId is required");

                else if (!string.IsNullOrWhiteSpace(sendResetPasswordMailRequest.EmailId))
                {
                    string emailRegex = AppConstants.EmailRegex;

                    Regex re = new Regex(emailRegex);
                    if (!re.IsMatch(sendResetPasswordMailRequest.EmailId))
                        ModelState.AddModelError("emailId", "EmailId is not valid");
                    else
                    {
                        var userDetail = await userService.GetUserByMailIdAsync(sendResetPasswordMailRequest.EmailId);
                        if (userDetail == null)
                            ModelState.AddModelError("emailId", "User does not exist in our database or must be inactive.");
                    }
                }
            }
            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
                if (payloadGet.Entity)
                {
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageList.Add("emailId", "An email with password reset instructions has been sent to your email address, if it exists in our system.");
                }
                else
                {
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Info.ToString();
                    payloadGet.MessageList.Add("emailId", "Something went wrong");
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }

        [HttpPut]
        [Route("UpdateContactDetail")]
        public async Task<IActionResult> AddUpdateUserContactDetailAsync([Required] UserContactDetail userContactDetail)
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (userContactDetail == null)
                ModelState.AddModelError("userContactDetail", "Requested parameters are not valid");
            else
            {
                var user = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId));
                if (user == null)
                {
                    ModelState.AddModelError("userContactDetail", "Employee does not exist in our database or must be inactive");
                }
                if (!string.IsNullOrWhiteSpace(userContactDetail.CountryStdCode) && !IsValidStdCode(userContactDetail.CountryStdCode))
                {
                    ModelState.AddModelError("CountryStdCode", "Country std code is not valid");
                }
                if (!string.IsNullOrWhiteSpace(userContactDetail.PhoneNumber) && !IsValidPhoneNumber(userContactDetail.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number is not valid");
                }
                if (!string.IsNullOrWhiteSpace(userContactDetail.DeskPhoneNumber) && !IsValidPhoneNumber(userContactDetail.DeskPhoneNumber))
                {
                    ModelState.AddModelError("DeskPhoneNumber", "Desk phone number is not valid");
                }
                if (!string.IsNullOrWhiteSpace(userContactDetail.SkypeUrl) && !IsValidSkypeUsername(userContactDetail.SkypeUrl))
                {
                    ModelState.AddModelError("SkypeUrl", "Skype url is not valid");
                }
                if (!string.IsNullOrWhiteSpace(userContactDetail.LinkedInUrl) && !IsLinkedInUrl(userContactDetail.LinkedInUrl))
                {
                    ModelState.AddModelError("LinkedInUrl", "LinkedIn url is not valid");
                }
                if (!string.IsNullOrWhiteSpace(userContactDetail.TwitterUrl) && !IsTwitterUrl(userContactDetail.TwitterUrl))
                {
                    ModelState.AddModelError("TwitterUrl", "Twitter url is not valid");
                }
            }

            if (ModelState.IsValid)
            {
                var result = await userService.AddUpdateUserContactAsync(userContactDetail, Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (result.Success)
                {
                    payload.Entity = result;
                    payload.MessageType = Common.MessageType.Success.ToString();
                    payload.IsSuccess = true;
                    payload.Status = Response.StatusCode;
                }
                else
                {
                    payload.MessageList.Add("userContactDetail", "something went wrong");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }
            return Ok(payload);
        }

        [Route("EmployeeProfile")]
        [HttpGet]
        public async Task<IActionResult> GetEmployeeProfileByEmployeeIdAsync()
        {
            var payloadGet = new PayloadCustom<EmployeeProfileResponse>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var employee = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId));
            if (employee == null)
                ModelState.AddModelError("employeeId", "User profile does not exist in our database or must be inactive.");

            if (ModelState.IsValid)
            {
                var record = await userService.GetEmployeeProfileByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId), token.UserToken);
                if (record != null)
                {
                    payloadGet.Entity = record;
                    payloadGet.MessageType = Common.MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("employeeId", "No employee exists");
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }

            return Ok(payloadGet);
        }

        [Route("UpdateProfileImage")]
        [HttpPut]
        public async Task<IActionResult> UploadProfileImage([Required] IFormFile file)
        {

            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);
            string fileExtension = System.IO.Path.GetExtension(file.FileName);

            var fSize = file.Length;
            var fileSize = fSize / 1000;

            if (fileExtension != null && (fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".png"
                                                                            || fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".svg"))
            {
                var employee = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId));
                if (employee == null)
                    ModelState.AddModelError("employeeId", "User profile does not exist in our database or must be inactive.");

                if (file.Length == 0)
                {
                    ModelState.AddModelError("file", "Please upload file.");
                }

                else if (fileSize > 5000)
                {
                    ModelState.AddModelError("file", "Picture size should be less than 5MB.");

                }

                if (ModelState.IsValid)
                {
                    payload.Entity = await userService.UploadProfileImageAsync(file, Convert.ToInt64(token.EmployeeId));
                    if (payload.Entity.Success)
                    {
                        payload.MessageType = MessageType.Success.ToString();
                        payload.MessageList.Add("file", "Image uploaded successfully.");
                        payload.IsSuccess = true;
                        payload.Status = Response.StatusCode;
                    }
                    else
                    {
                        payload.MessageList.Add("file", "Something went wrong, the Image not uploaded successfully");
                        payload.IsSuccess = false;
                        payload.Status = (int)HttpStatusCode.BadRequest;
                    }

                }
                else
                {
                    payload = GetPayloadStatus(payload);
                }


            }
            else
            {
                payload.IsSuccess = false;
                payload.MessageList.Add("file", "Please upload only jpg or png or jpeg or svg file format.");
                payload.Status = (int)HttpStatusCode.BadRequest;


            }
            return Ok(payload);
        }

        [Route("DeleteImage")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProfileImage()
        {
            var payload = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var employee = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId));
            if (employee == null)
                ModelState.AddModelError("employeeId", "User profile does not exist in our database or must be inactive.");


            if (ModelState.IsValid)
            {
                payload.Entity = await userService.DeleteProfileImageAsync(Convert.ToInt64(token.EmployeeId));
                if (payload.Entity.Success)
                {
                    payload.MessageType = MessageType.Success.ToString();
                    payload.MessageList.Add("file", "Image has been deleted successfully");
                    payload.IsSuccess = true;
                    payload.Status = Response.StatusCode;
                }
                else
                {
                    payload.MessageList.Add("file", "Something went wrong, the Image not deleted successfully.");
                    payload.IsSuccess = false;
                    payload.Status = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                payload = GetPayloadStatus(payload);
            }

            return Ok(payload);

        }

        [Route("ChangePassword")]
        [HttpPut]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest)
        {
            var payloadGet = new PayloadCustom<IOperationStatus>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (changePasswordRequest == null)
                ModelState.AddModelError("changePasswordRequest", "Requested parameter is required");

            else
            {
                if (string.IsNullOrWhiteSpace(changePasswordRequest.OldPassword))
                    ModelState.AddModelError("oldPassword", "Old Password is required");

                else if (string.IsNullOrWhiteSpace(changePasswordRequest.NewPassword))
                    ModelState.AddModelError("newPassword", "New Password is required");

                else
                {
                    string passwordRegex = AppConstants.StrongPassRegex;
                    Regex re = new Regex(passwordRegex);
                    if (!re.IsMatch(changePasswordRequest.NewPassword))
                        ModelState.AddModelError("newPassword", "A strong password is at least 8 characters long and includes uppercase and lowercase letters, numbers and symbols.");
                    else
                    {
                        var response = await userService.IsUserExistInAdAsync(token.EmailId);
                        var employee = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId));

                        if (!response.IsExist)
                        {
                            ModelState.AddModelError("changePasswordRequest", "Employee does not exist in Active directory or must be inactive");
                        }
                        else if(employee == null)
                            ModelState.AddModelError("changePasswordRequest", "Employee does not exist in our database or must be inactive");
                        else
                        {
                            if (changePasswordRequest.OldPassword == changePasswordRequest.NewPassword)
                                ModelState.AddModelError("newPassword", "Password cannot be same as old password. Please enter a valid password.");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {

                var status = await userService.ChangeAdPasswordAsync(changePasswordRequest);
                if (status)
                {
                    payloadGet.MessageType = Common.MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageList.Add("changePasswordRequest", "Password has been changed successfully.");
                }
                else
                {
                    payloadGet.MessageList.Add("changePasswordRequest", "Current password does not match.");
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }



        [Route("ReSendResetPasswordMail")]
        [HttpPost]
        public async Task<IActionResult> ReSendResetPasswordMailAsync()
        {
            var payloadGet = new PayloadCustom<bool>();
            var token = GetTokenDetails();
            if (!string.IsNullOrEmpty(token.EmailId) && Convert.ToInt64(token.EmployeeId) > 0)
            {
                var employee = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(token.EmployeeId));
                if (employee == null)
                    ModelState.AddModelError("employee", "Employee does not exist in our database or must be inactive.");
            }
            else
            {
                ModelState.AddModelError("employee", "Invalid request.");
            }

            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.ReSendResetPasswordMailAsync(Convert.ToInt64(token.EmployeeId));
                if (payloadGet.Entity)
                {
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageList.Add("emailId", "An email with password reset instructions has been sent to your email address.");
                }
                else
                {
                    payloadGet.IsSuccess = false;
                    payloadGet.Status = Response.StatusCode;
                    payloadGet.MessageType = MessageType.Info.ToString();
                    payloadGet.MessageList.Add("emailId", "Something went wrong");
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }

        [Route("RefreshToken")]
        [HttpGet]
        public async Task<IActionResult> RefreshToken()
        {
            var payloadGet = new PayloadCustom<RefreshTokenResponse>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (token.EmployeeId > 0)
            {
                var employee = await userService.GetUserByEmployeeIdAsync(token.EmployeeId);
                if (employee == null)
                    ModelState.AddModelError("employee", "Employee does not exist in our database or must be inactive.");
            }
            else
            {
                ModelState.AddModelError("employee", "Invalid request.");
            }
            if (ModelState.IsValid)
            {
                var result = await userService.GetRefreshToken(token.UserToken, token.EmployeeId);
                if (result != null)
                {
                    payloadGet.Entity = result;
                    payloadGet.MessageType = Common.MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("token", "Unable to create new token");
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }


        [Route("GoalLockDate")]
        [HttpGet]
        public async Task<IActionResult> GetGoalLockDateAsync(long organisationCycleId)
        {
            var payloadGet = new PayloadCustom<GoalUnlockDate>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (organisationCycleId <= 0)
                ModelState.AddModelError("organisationCycleId", "Organisation cycle id  is not valid.");

            if (ModelState.IsValid)
            {
                payloadGet.EntityList = await userService.GetGoalLockDateAsync(organisationCycleId);
                if (payloadGet.EntityList != null && payloadGet.EntityList.Count > 0)
                {
                    payloadGet.MessageType = MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("organisationCycleId", "No record found in the system.");
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }

        [HttpGet]
        [Route("GlobalSearch")]

        public ActionResult GlobalSearch(string finder, int searchType, int page = 1, int pagesize = 5)
        {
            var payloadGet = new PayloadCustomGenric<GlobalSearchList>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (searchType < 0 || searchType >= 2)
                ModelState.AddModelError("searchType", "Search type is invalid");

            if (ModelState.IsValid)
            {
                var data = userService.GlobalSearch(finder, searchType, page, pagesize, Convert.ToInt64(token.EmployeeId));
                if (data.Records.Count > 0)
                {
                    payloadGet.EntityList = data.Records;
                    payloadGet.PaggingInfo = data.PaggingInfo;
                    payloadGet.MessageType = MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("finder", "No employee exists");
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {

                        payloadGet.MessageList.Add(state.Key, error.ErrorMessage);
                    }
                }
                payloadGet.IsSuccess = false;
                payloadGet.Status = (int)HttpStatusCode.BadRequest;
            }

            return Ok(payloadGet);

        }

        [HttpGet]
        [Route("SearchTeamEmployee")]
        public ActionResult SearchTeamEmployee(string finder, long teamId, int page = 1, int pagesize = 5)
        {
            var payloadGet = new PayloadCustomGenric<SearchUserList>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            var data = userService.SearchTeamEmployee(finder, teamId, page, pagesize, Convert.ToInt64(token.EmployeeId));
            if (data.Records.Count > 0)
            {
                payloadGet.EntityList = data.Records;
                payloadGet.PaggingInfo = data.PaggingInfo;
                payloadGet.MessageType = MessageType.Success.ToString();
                payloadGet.IsSuccess = true;
                payloadGet.Status = Response.StatusCode;
            }
            else
            {
                payloadGet.MessageList.Add("finder", "No employee exists");
                payloadGet.IsSuccess = true;
                payloadGet.Status = (int)HttpStatusCode.NoContent;
            }

            return Ok(payloadGet);

        }

        [Route("AddAdUser")]
        [HttpPost]
        public async Task<IActionResult> AddAdUserAsync([Required] UserRequestModel userRequest)
        {
            var payloadAdd = new PayloadCustom<Employee>();
            var token = GetTokenDetails();
            if (!token.IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);
            string domain = string.Empty;
            var hasOrigin = Request.Headers.TryGetValue("OriginHost", out var origin);
            if ((!hasOrigin && Request.Host.Value.Contains("localhost")))
                domain = new Uri(configuration.GetValue<string>("FrontEndUrl")).Host;
            else if (!hasOrigin)
                ModelState.AddModelError("Domain", "Domain is required");

            else
                domain = new Uri(origin).Host;

            if (string.IsNullOrEmpty(userRequest.EmailId))
                ModelState.AddModelError("userName", "Username is not valid");
            else
            {
                var response = await userService.IsUserExistInAdAsync(userRequest.EmailId);
                if (response.IsExist)
                {
                    var emailExists = await userService.GetUserByMailIdAsync(response.EmailId);
                    if (emailExists != null)
                        ModelState.AddModelError("userName", "Username already exists");
                }
            }

            if (userRequest.RoleId <= 0)
                ModelState.AddModelError("roleId", "RoleId is not valid");
            if (userRequest.OrganizationId <= 0)
                ModelState.AddModelError("organizationId", "OrganizationId is not valid");

            if (string.IsNullOrWhiteSpace(userRequest.Designation))
                ModelState.AddModelError("designation", "Designation is required");

            if (string.IsNullOrWhiteSpace(userRequest.FirstName))
                ModelState.AddModelError("firstName", "FirstName cannot be blank");

            if (userRequest.FirstName != null && userRequest.FirstName.Contains(" "))
                ModelState.AddModelError("firstName", "FirstName can not contain spaces");

            if (!string.IsNullOrWhiteSpace(userRequest.FirstName))
            {
                string firstRegex = AppConstants.FirstNameRegex;
                Regex re = new Regex(firstRegex);
                if (!re.IsMatch(userRequest.FirstName))
                    ModelState.AddModelError("firstName", "FirstName is not valid");
            }

            if (ModelState.IsValid)
            {
                var result = await userService.AddAdUserAsync(userRequest, Convert.ToInt64(token.EmployeeId), domain);
                if (result.Entity != null)
                {
                    payloadAdd.Entity = result.Entity;
                    payloadAdd.MessageList.Add("userId", "User details are saved successfully.");
                    payloadAdd.IsSuccess = true;
                    payloadAdd.Status = Response.StatusCode;
                }
                else
                {
                    payloadAdd.MessageList.Add("userId", "No User details saved ");
                    payloadAdd.IsSuccess = true;
                    payloadAdd.Status = (int)HttpStatusCode.NoContent;
                }
            }

            else
            {
                payloadAdd = GetPayloadStatus(payloadAdd);
            }

            return Ok(payloadAdd);
        }

        [Route("IsValidReporting")]
        [HttpGet]
        public async Task<IActionResult> IsValidReporting(long empId, long reportingId)
        {
            var payloadGet = new PayloadCustom<bool>();
            if (!IsTokenActive)
                return StatusCode((int)HttpStatusCode.Unauthorized);

            if (empId <= 0)
                ModelState.AddModelError("empId", "Employee id  is not valid.");

            if (reportingId <= 0)
                ModelState.AddModelError("reportingId", "Reporting id  is not valid.");

            if (ModelState.IsValid)
            {
                payloadGet.Entity = await userService.IsValidReporting(empId, reportingId);
                if (payloadGet.Entity)
                {
                    payloadGet.MessageType = MessageType.Success.ToString();
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = Response.StatusCode;
                }
                else
                {
                    payloadGet.MessageList.Add("empId", "Please asign a valid reporting");
                    payloadGet.IsSuccess = true;
                    payloadGet.Status = (int)HttpStatusCode.NoContent;
                }
            }
            else
            {
                payloadGet = GetPayloadStatus(payloadGet);
            }
            return Ok(payloadGet);
        }


        #region Private Methods
        private bool IsValidPhoneNumber(string number)
        {
            return (Regex.Match(number, AppConstants.PhoneNumberRegex).Success);
        }
        private bool IsValidStdCode(string number)
        {
            return (Regex.Match(number, AppConstants.CountryStdCodeRegex).Success);
        }
        private bool IsValidSkypeUsername(string skypeUrl)
        {
            return Regex.Match(skypeUrl, AppConstants.SkypeRegex).Success;
        }
        private bool IsTwitterUrl(string twitterUrl)
        {
            List<string> urlList = AppConstants.TwitterUrls.Split(",").ToList();
            return (Uri.IsWellFormedUriString(twitterUrl, UriKind.Absolute) && urlList.Any(x => twitterUrl.StartsWith(x)));
        }
        private bool IsLinkedInUrl(string linkedInUrl)
        {
            List<string> urlList = AppConstants.LinkedInUrls.Split(",").ToList();
            return (Uri.IsWellFormedUriString(linkedInUrl, UriKind.Absolute) && urlList.Any(x => linkedInUrl.StartsWith(x)));
        }
        private Dictionary<string, string> ValidateUserDetails(UserRequestModel userRequestModel)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            var licenseDetail = organisationService.GetLicenceDetail(UserToken).Result;
            if (licenseDetail != null)
            {
                int availableUserToAdd =
                    (licenseDetail.PurchaseLicense + licenseDetail.BufferLicense) - licenseDetail.ActiveUser;
                if (availableUserToAdd <= 0)
                    errors.Add("userId", "You've consumed all of your available licenses, please upgrade your account to add more users");
            }
            else
            {
                errors.Add("userId", "You've consumed all of your available licenses, please upgrade your account to add more users");
            }
           
            if (userRequestModel.RoleId < 0)
                errors.Add("roleId", "Role Id is not valid");

            if (userRequestModel.OrganizationId <= 0)
                errors.Add("organisationId", "OrganisationId is not valid");

            if (string.IsNullOrWhiteSpace(userRequestModel.EmailId))
                errors.Add("emailId", "EmailId is required");

            if (string.IsNullOrWhiteSpace(userRequestModel.Designation))
                errors.Add("designation", "Designation is required");

            if (string.IsNullOrWhiteSpace(userRequestModel.FirstName))
                errors.Add("firstName", "FirstName cannot be blank");

            if (!string.IsNullOrWhiteSpace(userRequestModel.FirstName))
            {
                string firstRegex = AppConstants.FirstNameRegex;
                Regex re = new Regex(firstRegex);
                if (!re.IsMatch(userRequestModel.FirstName))
                    errors.Add("firstName", "FirstName is not valid");
            }

            if (!string.IsNullOrWhiteSpace(userRequestModel.EmailId))
            {
                string emailRegex = AppConstants.EmailRegex;
                Regex re = new Regex(emailRegex);
                if (!re.IsMatch(userRequestModel.EmailId))
                    errors.Add("emailId", "EmailId is not valid");
            }
            if (userRequestModel.ReportingTo < 0)
                errors.Add("reportingTo", "ReportingTo is not valid");
            return errors;
        }
        #endregion
    }

}


