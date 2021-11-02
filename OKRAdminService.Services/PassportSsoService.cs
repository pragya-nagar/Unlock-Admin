using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OKRAdminService.Services
{
    public class PassportSsoService : BaseService, IPassportSsoService
    {
        private readonly IRepositoryAsync<Employee> employeeRepo;
        private readonly IRepositoryAsync<Organisation> organisationRepo;
        private readonly IRepositoryAsync<UserToken> userTokenRepo;
        private readonly IRepositoryAsync<RoleMaster> roleMasterRepo;
        private readonly IRoleService roleService;
        private readonly IUserService userService;
        private readonly IPermissionService permissionService;

        public PassportSsoService(IServicesAggregator servicesAggregateService, IRoleService roleServices, IUserService userServices, IPermissionService permissionServices) : base(servicesAggregateService)
        {
            organisationRepo = UnitOfWorkAsync.RepositoryAsync<Organisation>();
            userTokenRepo = UnitOfWorkAsync.RepositoryAsync<UserToken>();
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
            roleMasterRepo = UnitOfWorkAsync.RepositoryAsync<RoleMaster>();
            roleService = roleServices;
            userService = userServices;
            permissionService = permissionServices;
        }

        public async Task<UserLoginResponse> SsoLoginAsync(SsoLoginRequest ssoLoginRequest)
        {
            var loginResponse = new UserLoginResponse();
            string privatekey = Configuration.GetSection("Passport:PrivateKey").Value;
            string baseAddress = Configuration.GetSection("Passport:QABaseAddress").Value;
            string issuedUrl = Configuration.GetSection("Passport:SsoUrl").Value;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("sessionid", ssoLoginRequest.SessionId);
            headers.Add("appid", ssoLoginRequest.AppId);
            headers.Add("privatekey", privatekey);

            var httpClient = GetHttpClient(null);
            httpClient.BaseAddress = new Uri(baseAddress);
            string header = string.Empty;
            foreach (string key in headers.Keys)
            {
                header += HttpUtility.UrlEncode(headers[key]) + ":";
            }
            header = header.TrimEnd(':');
            var bytes = Encoding.UTF8.GetBytes(header);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
            var response = await httpClient.GetAsync(issuedUrl);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"SsoLoginAsync completed ");
                var responseValue = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<PassportEmployeeResopnese>(responseValue);
                if (responseData != null)
                {
                    string loginEmailId = responseData.LoginId;
                    var userDetail = await userService.GetUserByMailIdAsync(loginEmailId);
                    if (userDetail != null)
                    {
                        var userRole = await roleService.GetRolesByUserIdAsync(userDetail.EmployeeId);
                        var reportingUser = await userService.GetUserByEmployeeIdAsync(Convert.ToInt64(userDetail.ReportingTo));
                        var orgDetail = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == userDetail.OrganisationId);
                        loginResponse.OrganisationId = userDetail.OrganisationId;
                        loginResponse.OrganisationName = orgDetail != null ? orgDetail.OrganisationName : string.Empty;
                        loginResponse.RoleName = userRole.RoleName;
                        if (reportingUser != null)
                        {
                            loginResponse.ReportingTo = reportingUser.EmployeeId;
                            loginResponse.ReportingName = reportingUser.FirstName + " " + reportingUser.LastName;
                        }
                        loginResponse.ImagePath = userDetail.ImagePath == null ? string.Empty : userDetail.ImagePath;
                        loginResponse.EmployeeCode = userDetail.EmployeeCode;
                        loginResponse.EmployeeId = userDetail.EmployeeId;
                        loginResponse.EmailId = userDetail.EmailId;
                        loginResponse.FirstName = userDetail.FirstName;
                        loginResponse.LastName = userDetail.LastName;
                        loginResponse.RoleId = userDetail.RoleId;
                        loginResponse.LoggedInAs = userRole.RoleName;
                        loginResponse.Designation = userDetail.Designation;
                        loginResponse.IsActive = userDetail.IsActive;
                        string ssoLogin = Configuration.GetSection("Passport:SsoLogin").Value;
                        loginResponse.SsoLogin = Convert.ToBoolean(ssoLogin);
                        loginResponse.LoginFailCount = userDetail.LoginFailCount;
                        loginResponse.Version = Configuration.GetSection("Copyright:Version").Value;
                        loginResponse.ProductID = Configuration.GetSection("Copyright:ProductID").Value;
                        loginResponse.License = Configuration.GetSection("Copyright:License").Value;
                        loginResponse.BelongsTo = Configuration.GetSection("Copyright:BelongsTo").Value;

                        var permissions = await permissionService.GetPermissionsByRoleIdAsync(userRole.RoleId);
                        if (permissions != null && permissions.Any())
                            loginResponse.RolePermissions = permissions;

                        var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForLoggedInUser);
                        loginResponse.ExpireTime = Convert.ToInt32(expireTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                        var token = await userService.GetExistingValidUserTokenAsync(userDetail.EmployeeId);
                        bool isNewToken = false;
                        if (!string.IsNullOrEmpty(token))
                        {
                            loginResponse.TokenId = token;
                            isNewToken = false;
                        }
                        else
                        {
                            loginResponse.TokenId = userService.GenerateJwtToken(userDetail, userRole.RoleId, expireTime);
                            isNewToken = true;
                        }
                        await userService.AddUpdateUserAccessTokenAsync(userDetail.EmployeeId, loginResponse.TokenId, expireTime, isNewToken);
                    }
                }

            }

            return loginResponse;
        }

        public async Task<List<PassportEmployeeResponse>> ActiveUserAsync()
        {
            var passportUsersList = new List<PassportEmployeeResponse>();
            var passportUsers = await GetAllPassportUsersAsync();
            if (passportUsers != null && passportUsers.Count > 0)
            {
                passportUsers = passportUsers.Where(x => x.IsActive).ToList();
                foreach (var user in passportUsers)
                {
                    if (user.OrganizationName == AppConstants.Learning || user.OrganizationName == AppConstants.UnlockLearn || user.OrganizationName == AppConstants.InfoProLearning || user.OrganizationName == AppConstants.Unlocklearn)
                    {
                        user.OrganizationName = AppConstants.InfoproLearning;
                    }
                    else if (user.OrganizationName == AppConstants.Digital)
                    {
                        user.OrganizationName = AppConstants.CompunnelDigital;
                    }
                    else if (user.OrganizationName == AppConstants.Staffing)
                    {
                        user.OrganizationName = AppConstants.CompunnelStaffing;
                    }
                    else 
                    {
                        user.OrganizationName = AppConstants.CompunnelSoftwareGroup;
                    }
                    var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode == Convert.ToString(user.EmployeeId));
                    var orgDetail = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationName == user.OrganizationName);
                    var employeeByEmailId = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == user.MailId);
                    if (employee != null && orgDetail != null)
                    {
                        var reportingUser = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode == Convert.ToString(user.ReportingTo));
                        employee.FirstName = user.FirstName;
                        employee.LastName = user.LastName;
                        employee.Designation = user.DesignationName;
                        if (reportingUser != null)
                        {
                            employee.ReportingTo = reportingUser.EmployeeId;
                        }
                        employee.IsActive = user.IsActive;
                        employee.UpdatedOn = DateTime.UtcNow;
                        employee.EmailId = user.MailId;
                        employeeRepo.Update(employee);
                        UnitOfWorkAsync.SaveChanges();
                        passportUsersList.Add(user);
                    }
                    else if (employee == null && orgDetail != null && employeeByEmailId == null)
                    {
                        var roleDetails = await roleMasterRepo.GetQueryable().FirstOrDefaultAsync(x => x.RoleName.Equals(AppConstants.DefaultUserRole));
                        var reportingUser = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode == Convert.ToString(user.ReportingTo));
                        string salt = Guid.NewGuid().ToString();
                        Employee employees = new Employee();
                        employees.EmployeeCode = Convert.ToString(user.EmployeeId);
                        employees.FirstName = user.FirstName;
                        employees.LastName = user.LastName;
                        employees.Password = CryptoFunctions.EncryptRijndael("abcd@1234", salt);
                        employees.PasswordSalt = salt;
                        employees.Designation = user.DesignationName;
                        employees.EmailId = user.MailId;
                        if (reportingUser != null)
                        {
                            employees.ReportingTo = reportingUser.EmployeeId;
                        }
                        employees.OrganisationId = orgDetail.OrganisationId;
                        employees.IsActive = user.IsActive;
                        employees.CreatedBy = 0;
                        employees.CreatedOn = DateTime.UtcNow;
                        employees.RoleId = roleDetails.RoleId;
                        employees.LoginFailCount = 0;
                        employeeRepo.Add(employees);
                        UnitOfWorkAsync.SaveChanges();
                        passportUsersList.Add(user);
                    }
                }
            }
            return passportUsersList;
        }

        public async Task<List<PassportEmployeeResponse>> InActiveUserAsync()
        {
            var passportUsersList = new List<PassportEmployeeResponse>();
            var passportUsers = await GetAllPassportUsersAsync();
            if (passportUsers != null && passportUsers.Count > 0)
            {
                passportUsers = passportUsers.Where(x => x.IsActive == false).ToList();
                foreach (var user in passportUsers)
                {
                    var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode == Convert.ToString(user.EmployeeId) && x.IsActive);
                    if (employee != null)
                    {
                        employee.IsActive = user.IsActive;
                        employee.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(employee);
                        UnitOfWorkAsync.SaveChanges();
                        passportUsersList.Add(user);

                        var getActiveToken = await userService.GetEmployeeAccessTokenAsync(employee.EmployeeId);
                        if (getActiveToken != null)
                        {
                            getActiveToken.ExpireTime = DateTime.UtcNow.AddMinutes(-1);
                            getActiveToken.LastLoginDate = getActiveToken.CurrentLoginDate;
                            userTokenRepo.Update(getActiveToken);
                            UnitOfWorkAsync.SaveChanges();
                        }
                    }
                }
            }
            return passportUsersList;
        }

        public async Task<List<PassportEmployeeResponse>> GetAllPassportUsersAsync()
        {
            var userType = AppConstants.PassportUserType;
            List<PassportEmployeeResponse> loginUserDetail = new List<PassportEmployeeResponse>();
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("Passport:BaseAddress"));
            var response = await httpClient.GetAsync($"User?userType=" + userType);

            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetAllUserFromPassport completed");
                var payload = JsonConvert.DeserializeObject<PayloadCustomPassport<PassportEmployeeResponse>>(await response.Content.ReadAsStringAsync());
                loginUserDetail = payload.EntityList;
            }
            return loginUserDetail;
        }



    }
}
