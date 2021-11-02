using Azure.Identity;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OfficeOpenXml;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using OperationStatus = OKRAdminService.EF.OperationStatus;

namespace OKRAdminService.Services
{
    public class UserService : BaseService, IUserService
    {
        private readonly IRepositoryAsync<Employee> employeeRepo;
        private readonly IRepositoryAsync<Organisation> organisationRepo;
        private readonly IRepositoryAsync<ErrorLog> errorLogRepo;
        private readonly IRepositoryAsync<UserToken> userTokenRepo;
        private readonly IRepositoryAsync<RoleMaster> roleMasterRepo;
        private readonly IRepositoryAsync<EmployeeContactDetail> contactDetailRepo;
        private readonly IRepositoryAsync<GoalUnlockDate> goalUnlockDateRepo;
        private readonly IPermissionService permissionService;
        private readonly IRoleService roleService;
        private readonly IOrganisationService organisationService;
        private readonly INotificationsEmailsService notificationsService;
        private readonly IRepositoryAsync<OrganisationCycle> organisationCycleRepo;
        protected readonly IDistributedCache _distributedCache;
        public UserService(IServicesAggregator servicesAggregateService, IPermissionService permission, IRoleService roleServices, IOrganisationService organisationServices, INotificationsEmailsService notificationsServices, IDistributedCache distributedCache) : base(servicesAggregateService)
        {
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
            organisationRepo = UnitOfWorkAsync.RepositoryAsync<Organisation>();
            userTokenRepo = UnitOfWorkAsync.RepositoryAsync<UserToken>();
            roleMasterRepo = UnitOfWorkAsync.RepositoryAsync<RoleMaster>();
            contactDetailRepo = UnitOfWorkAsync.RepositoryAsync<EmployeeContactDetail>();
            errorLogRepo = UnitOfWorkAsync.RepositoryAsync<ErrorLog>();
            goalUnlockDateRepo = UnitOfWorkAsync.RepositoryAsync<GoalUnlockDate>();
            organisationCycleRepo = UnitOfWorkAsync.RepositoryAsync<OrganisationCycle>();
            permissionService = permission;
            roleService = roleServices;
            organisationService = organisationServices;
            notificationsService = notificationsServices;
            _distributedCache = distributedCache;
        }

        public async Task<UserRequestModel> AddUserAsync(UserRequestModel userRequestModel, long loggedInUserId, string subDomain)
        {
            var roleDetails = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName.Equals(AppConstants.DefaultUserRole));
            string salt = Guid.NewGuid().ToString();
            IOperationStatus operationStatus = new OperationStatus();
            var employee = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmailId == userRequestModel.EmailId);
            if (employee != null && employee.EmployeeId > 0 && !employee.IsActive)
            {
                employee.EmployeeCode = userRequestModel.EmployeeCode;
                employee.FirstName = userRequestModel.FirstName;
                employee.LastName = userRequestModel.LastName;
                employee.Password = CryptoFunctions.EncryptRijndael("abcd@1234", salt);
                employee.PasswordSalt = salt;
                employee.Designation = userRequestModel.Designation;
                employee.EmailId = userRequestModel.EmailId;
                employee.ReportingTo = userRequestModel.ReportingTo;
                employee.OrganisationId = userRequestModel.OrganizationId;
                employee.IsActive = true;
                employee.CreatedBy = loggedInUserId;
                employee.CreatedOn = DateTime.UtcNow;
                employee.RoleId = userRequestModel.RoleId > 0 ? userRequestModel.RoleId : roleDetails.RoleId;
                employee.LoginFailCount = 0;
                employee.IsActive = true;
                employeeRepo.Update(employee);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();                
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }
            else if (employee == null)
            {
                employee = new Employee
                {
                    EmployeeCode = userRequestModel.EmployeeCode,
                    FirstName = userRequestModel.FirstName,
                    LastName = userRequestModel.LastName,
                    Password = CryptoFunctions.EncryptRijndael("abcd@1234", salt),
                    PasswordSalt = salt,
                    Designation = userRequestModel.Designation,
                    EmailId = userRequestModel.EmailId,
                    ReportingTo = userRequestModel.ReportingTo,
                    OrganisationId = userRequestModel.OrganizationId,
                    IsActive = true,
                    CreatedBy = loggedInUserId,
                    CreatedOn = DateTime.UtcNow,
                    RoleId = userRequestModel.RoleId > 0 ? userRequestModel.RoleId : roleDetails.RoleId,
                    LoginFailCount = 0
                };
                employeeRepo.Add(employee);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();

                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);


                OnBoardingRequest onBoardingRequest = new OnBoardingRequest();
                onBoardingRequest.EmployeeId = employee.EmployeeId;
                onBoardingRequest.CreatedBy = loggedInUserId;
                onBoardingRequest.CreatedOn = DateTime.UtcNow;

                await SaveDataForOnBoarding(onBoardingRequest, UserToken);
            }
            else
            {
                operationStatus.Success = true;
            }

            //string ssoLogin = Configuration.GetSection("Passport:SsoLogin").Value;
            if (operationStatus.Success && employee.EmployeeId > 0)
            {
                userRequestModel.EmployeeId = employee.EmployeeId;
                // add new code Add new user in Tenant DB
                await AddUserTenantAsync(new UserRequestDomainModel()
                {
                    EmailId = userRequestModel.EmailId,
                    SubDomain = subDomain
                }, UserToken);

                //invite the user
                await InviteUserAsync(userRequestModel, UserToken);

                await Task.Run(async () =>
                {
                    await notificationsService.AddUserNotificationsAndEmailsAsync(employee, UserToken).ConfigureAwait(false);
                }).ConfigureAwait(false);

                #region Old function code
                //var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == userRequestModel.EmailId && x.IsActive);
                //if (employeeDetails != null)
                //{
                //    var expireTime = DateTime.UtcNow.AddHours(AppConstants.ResetPasswordExpireHoursForNewlyAddedUser);
                //    var token = GenerateJwtToken(employeeDetails, employeeDetails.RoleId, expireTime);
                //    UserToken userAccessToken = new UserToken();
                //    userAccessToken.ExpireTime = expireTime;
                //    userAccessToken.Token = token;
                //    userAccessToken.EmployeeId = employeeDetails.EmployeeId;
                //    userAccessToken.TokenType = 3;
                //    userTokenRepo.Add(userAccessToken);
                //    var status = UnitOfWorkAsync.SaveChanges();
                //    if (status.Success && !string.IsNullOrEmpty(employeeDetails.EmailId))
                //    {
                //        var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
                //        var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
                //        var template = await GetMailerTemplateAsync(TemplateCodes.NCU.ToString(), jwtToken);
                //        string body = template.Body;
                //        string subject = template.Subject != "" ? template.Subject : "";
                //        var firstName = employeeDetails.FirstName;
                //        var lastName = employeeDetails.LastName;
                //        body = body.Replace("<user>", char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1)).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId)
                //            .Replace("topBar", keyVault.BlobCdnUrl ?? "" + AppConstants.TopBar).Replace("getStartedButton", keyVault.BlobCdnUrl ?? "" + AppConstants.GetStartedButtonImage)
                //            .Replace("logo", keyVault.BlobCdnUrl ?? "" + AppConstants.LogoImages).Replace("MFG", DateTime.Now.ToString("yyyy"))
                //             .Replace("screen-image", keyVault.BlobCdnUrl ?? "" + AppConstants.ScreenImage).Replace("signInButton", keyVault.BlobCdnUrl ?? "" + AppConstants.LoginButtonImage)
                //                                           .Replace("<mailTo>", employeeDetails.EmailId).Replace("<signIn>", settings.FrontEndUrl + "?redirectUrl=unlock-me" + "&empId=" + employeeDetails.EmployeeId).Replace("<resetUrl>", settings.ResetPassUrl + token.TrimEnd());
                //        MailRequest mailRequest = new MailRequest();
                //        mailRequest.MailTo = employeeDetails.EmailId;
                //        mailRequest.Subject = subject;
                //        mailRequest.Body = body;
                //        await SentMailAsync(mailRequest, jwtToken);
                //    }
                //}

                #endregion
            }
            return userRequestModel;
        }

        public async Task<IOperationStatus> EditUserAsync(UserRequestModel userRequestModel, long loggedInUserId)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var user = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == userRequestModel.EmployeeId && x.IsActive);

            var roleDetails = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName.Equals(AppConstants.DefaultUserRole));
            if (!(user is null))
            {
                user.FirstName = userRequestModel.FirstName;
                user.LastName = userRequestModel.LastName;
                user.EmployeeCode = userRequestModel.EmployeeCode;
                user.EmailId = userRequestModel.EmailId;
                user.Designation = userRequestModel.Designation;
                user.OrganisationId = userRequestModel.OrganizationId;
                user.ReportingTo = userRequestModel.ReportingTo;
                if (roleDetails != null)
                    user.RoleId = userRequestModel.RoleId <= 0 ? roleDetails.RoleId : userRequestModel.RoleId;
                user.UpdatedBy = loggedInUserId;
                user.UpdatedOn = DateTime.UtcNow;
                employeeRepo.Update(user);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + user.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                if (operationStatus.Success)
                    operationStatus.Entity = user;
            }

            await EditUserInADAsync(userRequestModel);

            return operationStatus;
        }

        public async Task<UserDetails> GetUserByEmpIdAsync(long empId)
        {
            var roleDetails = from employee in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                              join rpt in employeeRepo.GetQueryable() on employee.ReportingTo equals rpt.EmployeeId into employees
                              from rptdtl in employees.DefaultIfEmpty()
                              where employee.EmployeeId == empId
                              select (new UserDetails
                              {
                                  EmployeeId = employee.EmployeeId,
                                  FirstName = employee.FirstName,
                                  Designation = employee.Designation,
                                  LastName = employee.LastName,
                                  EmployeeCode = employee.EmployeeCode,
                                  EmailId = employee.EmailId,
                                  IsActive = employee.IsActive,
                                  ReportingTo = employee.ReportingTo,
                                  ReportingName = rptdtl != null ? rptdtl.FirstName + " " + rptdtl.LastName : string.Empty,
                                  ImagePath = employee.ImagePath,
                                  OrganisationId = employee.OrganisationId,
                                  OrganisationName = employee.Organisation.OrganisationName,
                                  RoleName = employee.RoleMaster.RoleName,
                                  RoleId = employee.RoleId
                              });
            return await roleDetails.FirstOrDefaultAsync();
        }

        public async Task<LoginUserDetails> Identity(long userId, string token)
        {
            LoginUserDetails userDetail = new LoginUserDetails();
            var userRecord = GetEmployeeByEmpId(userId);
            if (userRecord != null)
            {
                userDetail.EmployeeId = userRecord.EmployeeId;
                userDetail.RoleId = userRecord.RoleId;
                userDetail.FirstName = userRecord.FirstName;
                userDetail.LastName = userRecord.LastName;
                userDetail.EmailId = userRecord.EmailId;
                userDetail.EmployeeCode = userRecord.EmployeeCode;
                userDetail.OrganisationId = userRecord.OrganisationId;
                userDetail.IsActive = userRecord.IsActive;
                userDetail.ReportingTo = userRecord.ReportingTo;
                userDetail.ImageDetail = userRecord.ImagePath;
                userDetail.LastLoginDateTime = await GetUserLastLoginTime(userRecord.EmployeeId) ?? DateTime.UtcNow;
                var permissions = await permissionService.GetPermissionsByRoleIdAsync(userRecord.RoleId);
                if (permissions != null && permissions.Any())
                    userDetail.RolePermissions = permissions;
            }
            return userDetail;
        }

        public async Task<UserLoginResponse> UserByToken(string userEmail, string subDomainName)
        {
            var loginResponse = new UserLoginResponse();

            var userDetail = await GetUserByMailId(userEmail);
            var data = await OnBoardingControlDetailById(UserToken);

            if (userDetail == null)
            {
                var emailSplit = userEmail.Split('@');
                var orgDetailByName = GetDefaultOrganization();
                var designation = GetDefaultDesignation();

                var userRequestModel = new UserRequestModel
                {
                    FirstName = emailSplit.Length == 0 ? userEmail : emailSplit[0],
                    LastName = ".",
                    EmailId = userEmail,
                    EmployeeCode = "",
                    Designation = designation,
                    RoleId = 0,
                    OrganizationId = orgDetailByName.OrganisationId,
                    ReportingTo = 0
                };

                await AddUserAsync(userRequestModel, 0, subDomainName);

            }
            else if (!userDetail.IsActive)
            {
                userDetail.IsActive = true;
                employeeRepo.Update(userDetail);
                await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + userDetail.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }

            userDetail = await GetUserByMailIdAsync(userEmail);
            loginResponse.LastLoginTime = await GetUserLastLoginTime(userDetail.EmployeeId);
            await InsertUserTokenAsync(userDetail.EmployeeId, UserToken);
            var userRole = await roleService.GetRolesByUserIdAsync(userDetail.EmployeeId);
            var reportingIUser = await GetUserByEmployeeIdAsync(Convert.ToInt64(userDetail.ReportingTo));
            var orgDetail = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == userDetail.OrganisationId);
            loginResponse.OrganisationId = userDetail.OrganisationId;
            loginResponse.OrganisationName = orgDetail != null ? orgDetail.OrganisationName : string.Empty;
            if (reportingIUser != null)
            {
                loginResponse.ReportingTo = reportingIUser.EmployeeId;
                loginResponse.ReportingName = reportingIUser.FirstName + " " + reportingIUser.LastName;
            }

            var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForLoggedInUser);

            loginResponse.TokenId = UserToken;

            loginResponse.ExpireTime = Convert.ToInt32(expireTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            loginResponse.RoleName = userRole.RoleName;
            loginResponse.RoleId = userRole.RoleId;
            loginResponse.LoggedInAs = userRole.RoleName;

            loginResponse.Designation = userDetail.Designation;
            loginResponse.ImagePath = userDetail.ImagePath ?? string.Empty;
            loginResponse.EmployeeId = userDetail.EmployeeId;
            loginResponse.EmployeeCode = userDetail.EmployeeCode;
            loginResponse.EmailId = userDetail.EmailId;
            loginResponse.FirstName = userDetail.FirstName;
            loginResponse.LastName = userDetail.LastName;
            loginResponse.IsActive = userDetail.IsActive;
            loginResponse.SsoLogin = false; ///this is hardcoded bcz this will be used by secret login
            loginResponse.Version = Configuration.GetSection("Copyright:Version").Value;
            loginResponse.ProductID = Configuration.GetSection("Copyright:ProductID").Value;
            loginResponse.License = Configuration.GetSection("Copyright:License").Value;
            loginResponse.BelongsTo = Configuration.GetSection("Copyright:BelongsTo").Value;
            loginResponse.DirectReports = employeeRepo.GetQueryable().Any(x => x.ReportingTo == userDetail.EmployeeId && x.IsActive);
            if (data != null)
            {
                loginResponse.SkipCount = data.SkipCount;
                loginResponse.ReadyCount = data.ReadyCount;
            }
            else
            {
                loginResponse.SkipCount = AppConstants.SkipCountConstant;
                loginResponse.ReadyCount = AppConstants.SkipCountConstant;

            }

            loginResponse.IsTeamLeader = IsTeamLeader(userDetail.EmployeeId);
            var permissions = await permissionService.GetPermissionsByRoleIdAsync(userRole.RoleId);
            if (permissions != null && permissions.Any())
                loginResponse.RolePermissions = permissions;
            return loginResponse;
        }

        public async Task<UserLoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            var loginResponse = new UserLoginResponse();
            var userDetail = await GetUserByMailIdAsync(loginRequest.UserName);
            string password = CryptoFunctions.DecryptRijndael(userDetail.Password, userDetail.PasswordSalt);
            if (password != loginRequest.Password)
            {
                userDetail.LoginFailCount = (userDetail.LoginFailCount ?? 0) + 1;
                userDetail.UpdatedBy = userDetail.EmployeeId;
                userDetail.UpdatedOn = DateTime.UtcNow;
                employeeRepo.Update(userDetail);
                UnitOfWorkAsync.SaveChanges();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + userDetail.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                loginResponse.LoginFailCount = userDetail.LoginFailCount;
            }
            else if (password == loginRequest.Password)
            {
                var userRole = await roleService.GetRolesByUserIdAsync(userDetail.EmployeeId);
                var reportingIUser = await GetUserByEmployeeIdAsync(Convert.ToInt64(userDetail.ReportingTo));
                var orgDetail = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == userDetail.OrganisationId);
                loginResponse.OrganisationId = userDetail.OrganisationId;
                loginResponse.OrganisationName = orgDetail != null ? orgDetail.OrganisationName : string.Empty;
                if (reportingIUser != null)
                {
                    loginResponse.ReportingTo = reportingIUser.EmployeeId;
                    loginResponse.ReportingName = reportingIUser.FirstName + " " + reportingIUser.LastName;
                }
                var token = await GetExistingValidUserTokenAsync(userDetail.EmployeeId);
                var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForLoggedInUser);
                bool isNewToken;
                if (!string.IsNullOrEmpty(token))
                {
                    loginResponse.TokenId = token;
                    isNewToken = false;
                }
                else
                {
                    loginResponse.TokenId = GenerateJwtToken(userDetail, userRole.RoleId, expireTime);
                    isNewToken = true;
                }
                await AddUpdateUserAccessTokenAsync(userDetail.EmployeeId, loginResponse.TokenId, expireTime, isNewToken);

                loginResponse.ExpireTime = Convert.ToInt32(expireTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                loginResponse.RoleName = userRole.RoleName;
                loginResponse.RoleId = userRole.RoleId;
                loginResponse.LoggedInAs = userRole.RoleName;

                loginResponse.Designation = userDetail.Designation;
                loginResponse.ImagePath = userDetail.ImagePath ?? string.Empty;
                loginResponse.EmployeeId = userDetail.EmployeeId;
                loginResponse.EmployeeCode = userDetail.EmployeeCode;
                loginResponse.EmailId = userDetail.EmailId;
                loginResponse.FirstName = userDetail.FirstName;
                loginResponse.LastName = userDetail.LastName;
                loginResponse.IsActive = userDetail.IsActive;
                loginResponse.SsoLogin = false; ///this is hardcoded bcz this will be used by secret login
                loginResponse.Version = Configuration.GetSection("Copyright:Version").Value;
                loginResponse.ProductID = Configuration.GetSection("Copyright:ProductID").Value;
                loginResponse.License = Configuration.GetSection("Copyright:License").Value;
                loginResponse.BelongsTo = Configuration.GetSection("Copyright:BelongsTo").Value;
                loginResponse.DirectReports = employeeRepo.GetQueryable().Any(x => x.ReportingTo == userDetail.EmployeeId && x.IsActive);
                loginResponse.IsTeamLeader = organisationRepo.GetQueryable()
                    .Any(x => x.OrganisationHead == userDetail.EmployeeId && x.IsActive);
                var permissions = await permissionService.GetPermissionsByRoleIdAsync(userRole.RoleId);
                if (permissions != null && permissions.Any())
                    loginResponse.RolePermissions = permissions;


                userDetail.LoginFailCount = 0;
                userDetail.UpdatedBy = userDetail.EmployeeId;
                userDetail.UpdatedOn = DateTime.UtcNow;
                employeeRepo.Update(userDetail);
                await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + userDetail.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
            }
            return loginResponse;
        }

        public async Task<UserLoginResponse> GetIdentity(string userEmail)
        {
            var loginResponse = new UserLoginResponse();
            var userDetail = await GetUserByMailIdAsync(userEmail);

            var userRole = await roleService.GetRolesByUserIdAsync(userDetail.EmployeeId);
            var reportingIUser = await GetUserByEmployeeIdAsync(Convert.ToInt64(userDetail.ReportingTo));
            var orgDetail = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == userDetail.OrganisationId);
            loginResponse.OrganisationId = userDetail.OrganisationId;
            loginResponse.OrganisationName = orgDetail != null ? orgDetail.OrganisationName : string.Empty;
            if (reportingIUser != null)
            {
                loginResponse.ReportingTo = reportingIUser.EmployeeId;
                loginResponse.ReportingName = reportingIUser.FirstName + " " + reportingIUser.LastName;
            }
            var token = await GetExistingValidUserTokenAsync(userDetail.EmployeeId);
            var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForLoggedInUser);
            bool isNewToken = false;
            if (!string.IsNullOrEmpty(token))
            {
                loginResponse.TokenId = token;
                isNewToken = false;
            }
            else
            {
                loginResponse.TokenId = GenerateJwtToken(userDetail, userRole.RoleId, expireTime);
                isNewToken = true;
            }
            await AddUpdateUserAccessTokenAsync(userDetail.EmployeeId, loginResponse.TokenId, expireTime, isNewToken);

            loginResponse.ExpireTime = Convert.ToInt32(expireTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            loginResponse.RoleName = userRole.RoleName;
            loginResponse.RoleId = userRole.RoleId;
            loginResponse.LoggedInAs = userRole.RoleName;

            loginResponse.Designation = userDetail.Designation;
            loginResponse.ImagePath = userDetail.ImagePath ?? string.Empty;
            loginResponse.EmployeeId = userDetail.EmployeeId;
            loginResponse.EmployeeCode = userDetail.EmployeeCode;
            loginResponse.EmailId = userDetail.EmailId;
            loginResponse.FirstName = userDetail.FirstName;
            loginResponse.LastName = userDetail.LastName;
            loginResponse.IsActive = userDetail.IsActive;
            loginResponse.SsoLogin = false; ///this is hardcoded bcz this will be used by secret login
            loginResponse.Version = Configuration.GetSection("Copyright:Version").Value;
            loginResponse.ProductID = Configuration.GetSection("Copyright:ProductID").Value;
            loginResponse.License = Configuration.GetSection("Copyright:License").Value;
            loginResponse.BelongsTo = Configuration.GetSection("Copyright:BelongsTo").Value;
            loginResponse.DirectReports = employeeRepo.GetQueryable().Any(x => x.ReportingTo == userDetail.EmployeeId && x.IsActive);

            var permissions = await permissionService.GetPermissionsByRoleIdAsync(userRole.RoleId);
            if (permissions != null && permissions.Any())
                loginResponse.RolePermissions = permissions;


            userDetail.LoginFailCount = 0;
            userDetail.UpdatedBy = userDetail.EmployeeId;
            userDetail.UpdatedOn = DateTime.UtcNow;
            employeeRepo.Update(userDetail);
            await UnitOfWorkAsync.SaveChangesAsync();
            await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + userDetail.OrganisationId);
            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);

            return loginResponse;
        }

        public async Task<Employee> GetUserByEmployeeIdAsync(long userId)
        {
            return await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == userId && x.IsActive);
        }

        public async Task<Employee> GetUserByEmailIdAsync(string emailId, long employeeId)
        {
            return await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == emailId && x.EmployeeId != employeeId);
        }

        public async Task<PageResults<AllUsersResponse>> GetAllUsersAsync(int pageIndex = 1, int pageSize = 10)
        {
            var uquery = await (from emp in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                                join reportto in employeeRepo.GetQueryable() on emp.ReportingTo equals reportto.EmployeeId into employee
                                from rptdtl in employee.DefaultIfEmpty()
                                select new AllUsersResponse
                                {
                                    EmployeeId = emp.EmployeeId,
                                    FirstName = emp.FirstName,
                                    LastName = emp.LastName,
                                    EmployeeCode = emp.EmployeeCode,
                                    EmailId = emp.EmailId,
                                    OrganisationId = emp.OrganisationId,
                                    OrganisationName = emp.Organisation.OrganisationName,
                                    IsActive = emp.IsActive,
                                    ImagePath = emp.ImagePath ?? "",
                                    ReportingTo = emp.ReportingTo,
                                    ReportingToFirstName = rptdtl.FirstName,
                                    ReportingToLastName = rptdtl.LastName,
                                    ReportingToImagePath = rptdtl.ImagePath ?? "",
                                    ReportingToDesignation = rptdtl.Designation,
                                    Designation = emp.Designation,
                                    RoleId = emp.RoleId,
                                    RoleName = emp.RoleMaster.RoleName
                                }).ToListAsync();


            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var skipAmount = pageSize * (pageIndex - 1);
            var totalRecords = uquery.Count;
            var totalPages = totalRecords / pageSize;

            if (totalRecords % pageSize > 0)
                totalPages = totalPages + 1;

            var result = new PageResults<AllUsersResponse>();
            result.PageIndex = pageIndex;
            result.PageSize = pageSize;
            result.TotalRecords = totalRecords;
            result.TotalPages = totalPages;

            var PagedQuery = uquery.OrderBy(x => x.FirstName).Skip(skipAmount).Take(pageSize).ToList();
            result.Records = PagedQuery;
            return result;

        }

        public bool IsActiveToken(string tokenId, long employeeId, int tokenType = 1)
        {
            bool isActiveToken = false;
            var usertoken = GetAccessTokens(tokenId, employeeId, tokenType);
            var employeedetails = GetEmployeeByEmpId(employeeId);
            if (usertoken != null && employeedetails != null && employeedetails.RoleId == AppConstants.AdminRoleId)
            {
                isActiveToken = usertoken.ExpireTime >= DateTime.UtcNow;
            }
            return isActiveToken;
        }

        public bool IsUsersActiveToken(string tokenId, long employeeId, int tokenType = 1)
        {
            bool isActiveToken = false;
            var usertoken = GetAccessTokens(tokenId, employeeId, tokenType);
            var employeedetails = GetEmployeeByEmpId(employeeId);
            if (usertoken != null && employeedetails != null)
            {
                isActiveToken = usertoken.ExpireTime >= DateTime.UtcNow;
            }
            return isActiveToken;
        }

        public string GenerateJwtToken(Employee userDetails, long loggedInRoleId, DateTime expireTime)
        {
            var userRole = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleId == loggedInRoleId);
            var secretKey = AppConstants.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userDetails.EmployeeId.ToString()),
                    new Claim(ClaimTypes.Role, userRole?.RoleId.ToString()),
                    new Claim(ClaimTypes.Email, userDetails.EmailId)

                }),
                IssuedAt = DateTime.UtcNow,
                Expires = expireTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public void Logout(string tokenId, long employeeId)
        {
            var usertoken = GetAccessTokens(tokenId, employeeId);
            if (usertoken != null)
            {
                DateTime logoutTime = DateTime.UtcNow.AddMinutes(-1);
                usertoken.LastLoginDate = usertoken.CurrentLoginDate;
                usertoken.ExpireTime = logoutTime;
                userTokenRepo.Update(usertoken);
                UnitOfWorkAsync.SaveChanges();
            }
        }

        public async Task<PageResult<SearchUserList>> SearchEmployee(string key, int page, int pageSize, long employeeId)
        {
            if (key is null)
            {
                return null;
            }
            key = key.ToLower();
            var finalResult = new List<SearchUserList>();
            var searchResult = await SearchEmployeeByKey(key, employeeId);

            var filteredList1 = searchResult.Where(e => (e.FirstName + " " + e.LastName).ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList1);
            searchResult.RemoveAll(e => filteredList1.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList2 = searchResult.Where(e => e.FirstName.ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList2);
            searchResult.RemoveAll(e => filteredList2.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList3 = searchResult.Where(e => e.LastName.ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList3);
            searchResult.RemoveAll(e => filteredList3.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            finalResult.AddRange(searchResult);

            var totalRecords = finalResult.Count;
            var totalPages = (int)Math.Floor((float)totalRecords / pageSize);
            totalPages = totalPages + (totalRecords % pageSize == 0 ? 0 : 1);
            pageSize = pageSize < 0 ? 5 : pageSize;

            page = page < 0 ? 1 : page;
            page = page > totalPages ? totalPages : page;

            var result = new PageResult<SearchUserList>
            {
                Records = finalResult.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                PaggingInfo =
                {
                    Page = page, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages
                }
            };
            return result;
        }

        public async Task<List<SearchUserList>> SearchEmployeeByKey(string finder, long employeeId)
        {
            var organizationDetails = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == employeeId && x.IsActive);
            var loggedInEmpParentId = await organisationService.GetParentOrganisationIdAsync(organizationDetails.OrganisationId);
            List<SearchUserList> finalUsers = new List<SearchUserList>();
            var searchList = (from ud1 in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                              join ud2 in employeeRepo.GetQueryable() on ud1.ReportingTo equals ud2.EmployeeId into ps
                              from p in ps.DefaultIfEmpty()
                              where
                              (ud1.EmployeeId.ToString().Contains(finder) || (ud1.FirstName + " " + ud1.LastName).Contains(finder)) && ud1.IsActive
                              select
                              new SearchUserList
                              {
                                  EmployeeCode = ud1.EmployeeCode,
                                  EmailId = ud1.EmailId,
                                  EmployeeId = ud1.EmployeeId,
                                  OrganisationId = ud1.OrganisationId,
                                  OrganisationName = p.Organisation == null ? string.Empty : p.Organisation.OrganisationName,
                                  FirstName = ud1.FirstName,
                                  LastName = ud1.LastName,
                                  ImagePath = ud1.ImagePath,
                                  Designation = ud1.Designation,
                                  ReportingTo = p.EmployeeId,
                                  ReportingName = p == null ? string.Empty : p.FirstName + " " + p.LastName,
                                  RoleId = ud1.RoleId,
                                  RoleName = ud1.RoleMaster.RoleName,
                                  ReportingToDesignation = p == null ? string.Empty : p.Designation
                              }).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

            ////foreach (var item in searchList.ToList())
            ////{
            ////    var ParentId = organisationService.GetParentOrganisationId(item.OrganisationId);
            ////    if (ParentId != loggedInEmpParentId)
            ////    {
            ////        searchList.Remove(item);
            ////    }
            ////}

            finalUsers = searchList.ToList();

            var organisationIdList = finalUsers.GroupBy(x => x.OrganisationId).Select(x => Convert.ToInt64(x.Key)).ToList();
            if (organisationIdList.Any())
            {
                foreach (var orgId in organisationIdList)
                {
                    var parentOrgId = organisationService.GetParentOrganisationIdAsync(orgId).Result;
                    var finalOrgId = parentOrgId == 0 ? orgId : parentOrgId;
                    var cycleDetails = organisationService.GetCurrentCycleAsync(finalOrgId).Result;
                    if (cycleDetails != null)
                    {
                        finalUsers.Where(x => x.OrganisationId == orgId).ToList().ForEach(o =>
                        {
                            o.CycleDuration = cycleDetails.CycleDuration;
                            o.StartDate = cycleDetails.CycleStartDate.Date;
                            o.EndDate = cycleDetails.CycleEndDate?.Date;
                            o.CycleId = cycleDetails.OrganisationCycleId;
                            o.Year = cycleDetails.CycleYear;
                        });

                    }
                }
            }
            return finalUsers;
        }

        public async Task<PageResults<AllUsersResponse>> MultiSearchUserListAsync(string jwtToken, List<string> searchTexts, int pageIndex = 1, int pageSize = 10)
        {
            List<AllUsersResponse> finalUsers = new List<AllUsersResponse>();
            var userQuery = await (from emp in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                                   join reportto in employeeRepo.GetQueryable().Where(x => x.IsActive) on emp.ReportingTo equals reportto.EmployeeId into employee
                                   from rptdtl in employee.DefaultIfEmpty()
                                   where emp.IsActive
                                   select new AllUsersResponse
                                   {
                                       EmployeeId = emp.EmployeeId,
                                       FirstName = emp.FirstName,
                                       LastName = emp.LastName,
                                       EmployeeCode = emp.EmployeeCode,
                                       EmailId = emp.EmailId,
                                       OrganisationId = emp.OrganisationId,
                                       OrganisationName = emp.Organisation.OrganisationName ?? "",
                                       IsActive = emp.IsActive,
                                       ImagePath = emp.ImagePath ?? "",
                                       ReportingTo = emp.ReportingTo,
                                       ReportingToFirstName = rptdtl.FirstName ?? "",
                                       ReportingToLastName = rptdtl.LastName ?? "",
                                       ReportingToImagePath = rptdtl.ImagePath ?? "",
                                       ReportingToDesignation = rptdtl.Designation ?? "",
                                       Designation = emp.Designation,
                                       RoleId = emp.RoleId,
                                       RoleName = emp.RoleMaster.RoleName,

                                   }).ToListAsync();

            if (userQuery != null && searchTexts != null && searchTexts.Any())
            {
                searchTexts = searchTexts.ConvertAll(d => d.ToLower());
                foreach (var searchText in searchTexts)
                {
                    if (userQuery.Count > 0)
                    {
                        var queryData = userQuery.Where(x => Convert.ToString(x.FirstName + " " + x.LastName).ToLower().StartsWith(searchText)
                        || (x.FirstName.ToLower().StartsWith(searchText))
                        || (x.LastName.ToLower().StartsWith(searchText))
                        || (x.EmployeeCode.ToLower().StartsWith(searchText))
                        || (x.RoleName.ToLower().StartsWith(searchText))
                        || (x.Designation.ToLower().StartsWith(searchText))
                        || (x.OrganisationName.ToLower().StartsWith(searchText))
                        || (x.ReportingToFirstName + " " + x.ReportingToFirstName).ToLower().StartsWith(searchText)
                        || (x.ReportingToFirstName.ToLower().StartsWith(searchText))
                        || (x.ReportingToLastName.ToLower().StartsWith(searchText))
                        || (x.EmailId.ToLower().StartsWith(searchText))
                        ).ToList();

                        if (queryData.Count > 0)
                        {
                            finalUsers.AddRange(queryData);
                            userQuery = userQuery.Except(finalUsers).ToList();
                        }
                    }
                }
            }
            else if (userQuery != null)
            {
                finalUsers = userQuery;
            }

            var parentList = finalUsers.GroupBy(x => x.OrganisationId).Select(x => Convert.ToInt64(x.Key)).ToList();
            if (parentList.Any())
            {
                var lockLog = await GetUnLockLog(jwtToken);
                foreach (var orgId in parentList)
                {
                    long parentId = await organisationService.GetParentOrganisationIdAsync(orgId);

                    if (parentId > 0)
                    {
                        var organisation = await organisationService.GetOrganisationAsync(parentId);
                        var currentCycleDetail = await organisationService.GetCurrentCycleAsync(parentId);
                        if (organisation != null)
                        {
                            finalUsers.Where(x => x.OrganisationId == orgId).ToList().ForEach(o =>
                            {
                                o.HeadOrganisationName = organisation.OrganisationName;
                                o.HeadOrganisationId = parentId;
                                o.CurrentCycleId = currentCycleDetail?.OrganisationCycleId;
                                o.CurrentCycleYear = currentCycleDetail?.CycleYear;
                                o.CurrentCycleStartDate = currentCycleDetail?.CycleStartDate.Date;
                                o.LockStatus = UnlockStatus(lockLog, o.CurrentCycleStartDate, o.CurrentCycleId, o.CurrentCycleYear, o.EmployeeId).Result;
                            });

                        }
                    }
                }
            }

            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            var skipAmount = pageSize * (pageIndex - 1);
            var totalRecords = finalUsers.Count;
            var totalPages = totalRecords / pageSize;

            if (totalRecords % pageSize > 0)
                totalPages += 1;

            var result = new PageResults<AllUsersResponse>();
            result.PageIndex = pageIndex;
            result.PageSize = pageSize;
            result.TotalRecords = totalRecords;
            result.TotalPages = totalPages;

            var pagedQuery = finalUsers.OrderBy(x => x.FirstName).Skip(skipAmount).Take(pageSize).ToList();
            result.Records = pagedQuery;
            return result;

        }
        /// <summary>
        /// Maintaion lock icn status
        /// unlockStatus = -1 then lock icon disabled and Black
        /// unlockStatus = 0 then lock icon clicable and Black
        /// unlockStatus = 1 then lock icon Clickable and Purple
        /// unlockStatus = 2 then lock icon Open Lock Icon and disabled
        /// </summary>
        /// <param name="unLockLogs"></param>
        /// <param name="currentCycleStartDate"></param>
        /// <param name="currentCycle"></param>
        /// <param name="currentYear"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task<int> UnlockStatus(List<UnLockLog> unLockLogs, DateTime? currentCycleStartDate, long? currentCycle, int? currentYear, long employeeId)
        {
            int unlockStatus = 0;
            DateTime currentCyclestartdateNew;
            long currentCycleNew;
            int currentYearNew;
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            if (currentCycleStartDate == null || currentCycle == null || currentYear == null)
            {
                return unlockStatus;
            }
            else
            {
                var unlockDate = goalUnlockDateRepo.GetQueryable().FirstOrDefault(x => x.OrganisationCycleId == currentCycle && x.Type == 1);
                var unlockTime = settings.OkrUnlockTime; //// Configuration.GetSection("Services:UnlockTime").Value;
                currentCyclestartdateNew = unlockDate?.SubmitDate.Date ?? Convert.ToDateTime(currentCycleStartDate).AddDays(Convert.ToDouble(unlockTime));
                currentCycleNew = currentCycle ?? Convert.ToInt64(0);
                currentYearNew = currentYear ?? Convert.ToInt32(0);
            }
            if (currentCyclestartdateNew.Date >= DateTime.UtcNow.Date)
            {
                unlockStatus = -1;
                return unlockStatus;
            }
            var currentlockstatus = unLockLogs.OrderByDescending(x => x.UnLockLogId).FirstOrDefault(x => x.EmployeeId == employeeId && x.Cycle == currentCycleNew && x.Year == currentYearNew);
            if (currentlockstatus != null)
            {
                if (currentlockstatus.Status == 1)
                {
                    unlockStatus = 1;
                    return unlockStatus;
                }
                else if (currentCyclestartdateNew.Date <= DateTime.UtcNow.Date && currentlockstatus.Status == 2 && currentlockstatus.LockedTill.Date >= DateTime.UtcNow.Date)
                {
                    unlockStatus = 2;
                    return unlockStatus;
                }
                else if (currentCyclestartdateNew.Date <= DateTime.UtcNow.Date && (currentlockstatus.Status == 2 || currentlockstatus.Status == 1) && currentlockstatus.LockedTill.Date > DateTime.UtcNow.Date)
                {
                    unlockStatus = 1;
                    return unlockStatus;
                }
                else
                {
                    unlockStatus = 0;
                    return unlockStatus;
                }
            }
            else
            {
                unlockStatus = 0;
                return unlockStatus;
            }
        }

        public async Task<List<UnLockLog>> GetUnLockLog(string jwtToken)
        {
            var okrDetailResponse = new List<UnLockLog>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            var apiUrl = new Uri(settings.UnlockLog);
            var response = httpClient.GetAsync(apiUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonConvert.DeserializeObject<PayloadCustom<UnLockLog>>(await response.Content.ReadAsStringAsync());
                okrDetailResponse = payload.EntityList;
            }
            return okrDetailResponse;
        }

        public async Task<IOperationStatus> DeleteUserAsync(List<long> employeeIdList, long loggedInUserId, string jwtToken)
        {
            List<Employee> employees = new List<Employee>();
            foreach (var employeeId in employeeIdList)
            {
                var employeeDetails = await employeeRepo.FindOneAsync(x => x.EmployeeId == employeeId && x.IsActive);
                if (!(employeeDetails is null))
                {
                    employees.Add(employeeDetails);
                    employeeDetails.IsActive = false;
                    employeeDetails.UpdatedBy = loggedInUserId;
                    employeeDetails.UpdatedOn = DateTime.UtcNow;
                    employeeRepo.Update(employeeDetails);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeeDetails.OrganisationId);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                }
                var tokenDetails = await userTokenRepo.FindOneAsync(x => x.EmployeeId == employeeId && x.ExpireTime >= DateTime.UtcNow && x.TokenType == 1);
                if (tokenDetails != null)
                {
                    tokenDetails.ExpireTime = DateTime.UtcNow;
                    userTokenRepo.Update(tokenDetails);
                }

            }

            string ssoLogin = Configuration.GetSection("Passport:SsoLogin").Value;
            if (ssoLogin == "false")
            {
                await Task.Run(async () =>
            {
                await notificationsService.DeleteUserNotificationsAndEmailsAsync(employees, jwtToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
            }

            await Task.Run(async () =>
                {
                    await notificationsService.DeleteUserFromSystemNotificationsAndEmailsAsync(employees, jwtToken).ConfigureAwait(false);
                }).ConfigureAwait(false);

            
            return await UnitOfWorkAsync.SaveChangesAsync();

        }

        public async Task<Employee> GetReportingToOrganisationHeadAsync(List<long> employeeIdList)
        {
            Employee employee = null;
            if (employeeIdList != null && employeeIdList.Any())
            {
                var reportTo = await employeeRepo.GetQueryable().Where(x => x.ReportingTo != null && employeeIdList.Contains((long)x.ReportingTo) && x.IsActive).ToListAsync();
                if (reportTo != null && reportTo.Any())
                {
                    employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == reportTo.First().ReportingTo && x.IsActive);
                }
                else
                {
                    var orgHead = await organisationRepo.GetQueryable().Where(x => x.OrganisationHead != null && employeeIdList.Contains((long)x.OrganisationHead) && x.IsActive).ToListAsync();
                    if (orgHead != null && orgHead.Any())
                    {
                        employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == orgHead.First().OrganisationHead && x.IsActive);
                    }
                }
            }
            return employee;
        }

        public async Task<IOperationStatus> UploadBulkUserAsync(IFormFile formFile, long loggedInUserId, string jwtToken, string subDomain)
        {
            IOperationStatus operationStatus = new OperationStatus();
            if (formFile != null && formFile.Length > 0 && !string.IsNullOrEmpty(formFile.FileName))
            {
                var fileName = formFile.FileName;
                using (var stream = new MemoryStream())
                {
                    await formFile.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var fileExt = Path.GetExtension(fileName);
                    var totalRowsAffected = 0;

                    var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
                    var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
                    var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
                    var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
                    var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

                    if (keyVault != null)
                    {
                        var strFolderName = Configuration.GetValue<string>("AzureBlob:BulkUploadFolderName");
                        var azureLocation = strFolderName + "/" + loggedInUserId + "_" + DateTime.UtcNow.Year + DateTime.UtcNow.Month + DateTime.UtcNow.Day + "_" + DateTime.UtcNow.Hour + DateTime.UtcNow.Minute + DateTime.UtcNow.Second + fileExt;
                        var account = new CloudStorageAccount(new StorageCredentials(keyVault.BlobAccountName, keyVault.BlobAccountKey), true);
                        var cloudBlobClient = account.CreateCloudBlobClient();

                        var cloudBlobContainer = cloudBlobClient.GetContainerReference(keyVault.BlobContainerName);

                        if (await cloudBlobContainer.CreateIfNotExistsAsync())
                        {
                            await cloudBlobContainer.SetPermissionsAsync(
                                new BlobContainerPermissions
                                {
                                    PublicAccess = BlobContainerPublicAccessType.Blob
                                }
                                );
                        }

                        var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(azureLocation);
                        cloudBlockBlob.Properties.ContentType = formFile.ContentType;

                        await cloudBlockBlob.UploadFromStreamAsync(formFile.OpenReadStream());

                    }


                    if (fileExt == ".csv")
                    {
                        var errors = new List<KeyValuePair<string, string>>();
                        var bulkList = new List<BulkUploadDataModel>();
                        stream.Position = 0;
                        TextReader textReader = new StreamReader(stream);
                        var csv = new CsvReader(textReader, CultureInfo.InvariantCulture);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.Configuration.HeaderValidated = null;
                        csv.Configuration.Delimiter = ",";
                        try
                        {
                            bulkList = csv.GetRecords<BulkUploadDataModel>().ToList();
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error in uploading csv file" + e);
                        }
                        if (bulkList.Count > 1)
                        {
                            var totalLicenceDetails = organisationService.GetLicenceDetail(jwtToken).Result;
                            if (totalLicenceDetails != null)
                            {
                                int totalAvailable = (totalLicenceDetails.PurchaseLicense + totalLicenceDetails.BufferLicense) - totalLicenceDetails.ActiveUser;
                                if (bulkList.Count - 1 <= totalAvailable)
                                {
                                    var rowIterator = 1;
                                    var addedUsersIds = new List<long>();
                                    foreach (var item in bulkList.Skip(1))
                                    {
                                        var salt = Guid.NewGuid().ToString();
                                        var employee = new Employee();
                                        var error = string.Empty;
                                        rowIterator += 1;

                                        if (string.IsNullOrWhiteSpace(item.FirstName))
                                            error += ", FirstName is required";

                                        if (!string.IsNullOrWhiteSpace(item.FirstName))
                                        {
                                            var firstRegex = AppConstants.FirstNameRegex;
                                            var re = new Regex(firstRegex);
                                            if (!re.IsMatch(item.FirstName))
                                                error += ", FirstName is not valid";
                                        }

                                        if (string.IsNullOrWhiteSpace(item.Designation))
                                            error += ", Designation is required";

                                        if (string.IsNullOrWhiteSpace(item.EmailId))
                                            error += ", EmailId is required";

                                        if (!string.IsNullOrWhiteSpace(item.EmailId))
                                        {
                                            var emailRegex = AppConstants.EmailRegex;
                                            var re = new Regex(emailRegex);
                                            if (!re.IsMatch(item.EmailId))
                                                error = error + ", EmailId is not valid";
                                        }

                                        if (string.IsNullOrWhiteSpace(item.Organisation))
                                            error += ", Organisation is required";

                                        if (string.IsNullOrEmpty(item.LastName))
                                            error += ", LastName is required";

                                        if (!string.IsNullOrEmpty(item.EmployeeCode) && !string.IsNullOrEmpty(item.ReportingTo) && item.EmployeeCode == item.ReportingTo)
                                        {
                                            error = error + ", EmployeeId & ReportingTo cannot be same";
                                        }

                                        if (!string.IsNullOrEmpty(item.ReportingTo) && item.ReportingTo.Contains(','))
                                        {
                                            error = error + ", ReportingTo does not accept comma";
                                        }

                                        var cellValue = item.Role;
                                        var role = string.IsNullOrEmpty(cellValue) ? AppConstants.DefaultUserRole : cellValue;
                                        var roleDetails = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == role);
                                        if (roleDetails is null)
                                            error += ", Role does not exist";

                                        if (!string.IsNullOrEmpty(error))
                                        {
                                            error = error.TrimStart(',');
                                            errors.Add(new KeyValuePair<string, string>(Convert.ToString(rowIterator) + "-" + item.FirstName + " " + item.LastName, error));
                                        }
                                        else
                                        {
                                            var organisationDetail = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationName == item.Organisation && x.IsActive && x.IsDeleted == false);
                                            var employeeDetail = await employeeRepo.FindOneAsync(x => x.EmailId == item.EmailId);
                                            if (!string.IsNullOrWhiteSpace(item.ReportingTo))
                                            {
                                                var reportTo = await employeeRepo.FindOneAsync(x => x.EmployeeCode == item.ReportingTo && x.IsActive);
                                                if (reportTo is null)
                                                    error += ", ReportingTo does not exist";
                                            }

                                            if (!string.IsNullOrWhiteSpace(item.EmployeeCode))
                                            {
                                                var empCode = await employeeRepo.FindOneAsync(x => x.EmployeeCode == item.EmployeeCode);
                                                if (empCode != null)
                                                    error += ", EmployeeId already exists";
                                            }

                                            if (!(employeeDetail is null))
                                                error += ", EmailId already exists";

                                            if (organisationDetail is null)
                                                error += ", Organisation does not exist";

                                            if (!string.IsNullOrEmpty(error))
                                            {
                                                error = error.TrimStart(',');
                                                errors.Add(new KeyValuePair<string, string>(Convert.ToString(rowIterator) + "-" + item.FirstName + " " + item.LastName, error.ToString()));
                                            }
                                            else if (employeeDetail is null && !(organisationDetail is null))
                                            {
                                                employee.EmployeeCode = string.IsNullOrWhiteSpace(item.EmployeeCode) ? " " : item.EmployeeCode;
                                                employee.FirstName = item.FirstName;
                                                employee.LastName = item.LastName;
                                                employee.Password = CryptoFunctions.EncryptRijndael("abcd@1234", salt);
                                                employee.PasswordSalt = salt;
                                                employee.Designation = item.Designation;
                                                employee.EmailId = item.EmailId;
                                                if (!string.IsNullOrWhiteSpace(item.ReportingTo))
                                                {
                                                    var reportTo = await employeeRepo.FindOneAsync(x => x.EmployeeCode == item.ReportingTo && x.IsActive);
                                                    employee.ReportingTo = reportTo.EmployeeId;
                                                }
                                                else
                                                {
                                                    employee.ReportingTo = 0;
                                                }
                                                employee.OrganisationId = organisationDetail.OrganisationId;
                                                employee.RoleId = roleDetails.RoleId;
                                                employee.IsActive = true;
                                                employee.CreatedBy = loggedInUserId;
                                                employee.CreatedOn = DateTime.UtcNow;
                                                employee.LoginFailCount = 0;
                                                employeeRepo.Add(employee);
                                                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                                                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                                                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                                                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                                                totalRowsAffected++;
                                                addedUsersIds.Add(employee.EmployeeId);
                                            }
                                            // add new code Add new user in Tenant DB
                                            var tenantResponse = await AddUserTenantAsync(new UserRequestDomainModel()
                                            {
                                                EmailId = item.EmailId,
                                                SubDomain = subDomain
                                            }, UserToken);

                                            if (tenantResponse.IsSuccess)
                                            {
                                                var userRequestModel = new UserRequestModel()
                                                {
                                                    FirstName = item.FirstName,
                                                    LastName = item.LastName,
                                                    EmailId = item.EmailId
                                                };
                                                //invite the user
                                                await InviteUserAsync(userRequestModel, UserToken);
                                            }

                                        }
                                    }
                                    string ssoLogin = Configuration.GetSection("Passport:SsoLogin").Value;
                                    if (operationStatus.Success && ssoLogin == "false")
                                    {
                                        await Task.Run(async () =>
                                    {
                                        await notificationsService.BulkUploadNotificationsAndEmailsForCsvAsync(bulkList, jwtToken).ConfigureAwait(false);
                                    }).ConfigureAwait(false);

                                        foreach (var user in addedUsersIds)
                                        {
                                            var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == user && x.IsActive);
                                            if (employeeDetails != null)
                                            {
                                                var expireTime = DateTime.UtcNow.AddHours(AppConstants.ResetPasswordExpireHoursForNewlyAddedUser);
                                                var token = GenerateJwtToken(employeeDetails, employeeDetails.RoleId, expireTime);
                                                UserToken userAccessToken = new UserToken();
                                                userAccessToken.ExpireTime = expireTime;
                                                userAccessToken.Token = token;
                                                userAccessToken.EmployeeId = employeeDetails.EmployeeId;
                                                userAccessToken.TokenType = 3;
                                                userTokenRepo.Add(userAccessToken);
                                                var status = UnitOfWorkAsync.SaveChanges();
                                                if (status.Success && !string.IsNullOrEmpty(employeeDetails.EmailId))
                                                {
                                                    var settings = await KeyVaultService.GetSettingsAndUrlsAsync();

                                                    var template = await GetMailerTemplateAsync(TemplateCodes.NCU.ToString(), jwtToken);
                                                    string body = template.Body;
                                                    string subject = template.Subject != "" ? template.Subject : "";
                                                    var firstName = employeeDetails.FirstName;
                                                    var lastName = employeeDetails.LastName;
                                                    body = body.Replace("<user>", char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1))
                                                        .Replace("topBar", keyVault.BlobCdnUrl ?? "" + AppConstants.TopBar).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("MFG", DateTime.Now.ToString("yyyy"))
                                                        .Replace("logo", keyVault.BlobCdnUrl ?? "" + AppConstants.LogoImages).Replace("getStartedButton", keyVault.BlobCdnUrl ?? "" + AppConstants.GetStartedButtonImage)
                                                         .Replace("screen-image", keyVault.BlobCdnUrl ?? "" + AppConstants.ScreenImage).Replace("signInButton", keyVault.BlobCdnUrl ?? "" + AppConstants.LoginButtonImage)
                                                                                       .Replace("<signIn>", settings.FrontEndUrl + "?redirectUrl=unlock-me" + "&empId=" + employeeDetails.EmployeeId).Replace("<resetUrl>", settings.ResetPassUrl + token.TrimEnd()).Replace("srcInstagram", keyVault.BlobCdnUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnUrl + AppConstants.Linkedin)
                                                         .Replace("srcTwitter", keyVault.BlobCdnUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                                                         .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                                                    MailRequest mailRequest = new MailRequest();
                                                    mailRequest.MailTo = employeeDetails.EmailId;
                                                    mailRequest.Subject = subject;
                                                    mailRequest.Body = body;
                                                    await SentMailAsync(mailRequest, jwtToken);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    errors.Add(new KeyValuePair<string, string>("ErrorMessage", "The file has more users than the purchased licenses, please edit the file to reduce few users."));
                                }
                            }
                            else
                            {
                                errors.Add(new KeyValuePair<string, string>("ErrorMessage", "The file has more users than the purchased licenses, please edit the file to reduce few users."));
                            }
                        }
                        else
                        {
                            errors.Add(new KeyValuePair<string, string>("ErrorMessage", "Uploaded file does not have any record or not provided required values."));
                        }
                        operationStatus.RecordsAffected = totalRowsAffected;
                        operationStatus.BulkErrors = errors;
                    }
                    else
                    {
                        using (var package = new ExcelPackage(stream))
                        {
                            List<long> reporting = new List<long>();
                            List<string> employeeCodes = new List<string>();
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            List<KeyValuePair<string, string>> errors = new List<KeyValuePair<string, string>>();
                            if (noOfRow > 1 && noOfCol == 8)
                            {
                                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                                {
                                    string salt = Guid.NewGuid().ToString();
                                    var employee = new Employee();
                                    string error = string.Empty;

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 1].Value)))
                                        error = error + ", FirstName is required";

                                    if (!string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 1].Value)))
                                    {
                                        string firstRegex = AppConstants.FirstNameRegex;
                                        Regex re = new Regex(firstRegex);
                                        if (!re.IsMatch(Convert.ToString(workSheet.Cells[rowIterator, 1].Value)))
                                            error = error + ", FirstName is not valid";
                                    }

                                    if (string.IsNullOrEmpty(Convert.ToString(workSheet.Cells[rowIterator, 2].Value)))
                                        error = error + ", LastName is required";

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 4].Value)))
                                        error = error + ", EmailId is required";

                                    if (!string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 4].Value)))
                                    {
                                        string emailRegex = AppConstants.EmailRegex;

                                        Regex re = new Regex(emailRegex);
                                        if (!re.IsMatch(Convert.ToString(workSheet.Cells[rowIterator, 4].Value)))
                                            error = error + ", EmailId is not valid";
                                    }

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 5].Value)))
                                        error = error + ", Designation is required";

                                    if (!string.IsNullOrEmpty(Convert.ToString(workSheet.Cells[rowIterator, 3].Value)) && !string.IsNullOrEmpty(Convert.ToString(workSheet.Cells[rowIterator, 8].Value)) && Convert.ToString(workSheet.Cells[rowIterator, 3].Value) == Convert.ToString(workSheet.Cells[rowIterator, 8].Value))
                                    {
                                        error = error + ", EmployeeId & ReportingTo cannot be same";
                                    }

                                    if (!string.IsNullOrEmpty(Convert.ToString(workSheet.Cells[rowIterator, 8].Value)) && Convert.ToString(workSheet.Cells[rowIterator, 8].Value).Contains(','))
                                    {
                                        error = error + ", ReportingTo does not accept comma";
                                    }

                                    var cellValue = Convert.ToString(workSheet.Cells[rowIterator, 6].Value);
                                    string role = string.IsNullOrEmpty(cellValue) ? AppConstants.DefaultUserRole : cellValue;
                                    var roleDetails = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == role);
                                    if (roleDetails is null)
                                        error = error + ", Role does not exist";

                                    if (string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 7].Value)))
                                        error += ", Organisation is required";

                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        error = error.TrimStart(',');
                                        errors.Add(new KeyValuePair<string, string>(Convert.ToString(rowIterator) + "-" + Convert.ToString(workSheet.Cells[rowIterator, 1].Value) + " " + Convert.ToString(workSheet.Cells[rowIterator, 2].Value), error.ToString()));
                                    }

                                    else
                                    {
                                        var organisationDetail = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationName == Convert.ToString(workSheet.Cells[rowIterator, 7].Value) && x.IsActive && x.IsDeleted == false);
                                        var employees = await employeeRepo.FindOneAsync(x => x.EmailId == Convert.ToString(workSheet.Cells[rowIterator, 4].Value));

                                        if (!string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 8].Value)))
                                        {
                                            var reportTo = await employeeRepo.FindOneAsync(x => x.EmployeeCode == Convert.ToString(workSheet.Cells[rowIterator, 8].Value) && x.IsActive);
                                            if (reportTo is null)
                                                error = error + ", ReportingTo does not exist";
                                        }
                                        if (!string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 3].Value)))
                                        {
                                            var empCode = await employeeRepo.FindOneAsync(x => x.EmployeeCode == Convert.ToString(workSheet.Cells[rowIterator, 3].Value));
                                            if (empCode != null)
                                                error = error + ", EmployeeId already exists";
                                        }

                                        if (!(employees is null))
                                            error = error + ", EmailId already exists";

                                        if (organisationDetail is null)
                                            error = error + ", Organisation does not exist";

                                        if (!string.IsNullOrEmpty(error))
                                        {
                                            error = error.TrimStart(',');
                                            errors.Add(new KeyValuePair<string, string>(Convert.ToString(rowIterator) + "-" + Convert.ToString(workSheet.Cells[rowIterator, 1].Value) + " " + Convert.ToString(workSheet.Cells[rowIterator, 2].Value), error.ToString()));
                                        }
                                        else if (employees is null && !(organisationDetail is null))
                                        {
                                            employee.EmployeeCode = string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 3].Value)) ? " " : Convert.ToString(workSheet.Cells[rowIterator, 3].Value);
                                            employee.FirstName = Convert.ToString(workSheet.Cells[rowIterator, 1].Value);
                                            employee.LastName = Convert.ToString(workSheet.Cells[rowIterator, 2].Value);
                                            employeeCodes.Add(employee.EmployeeCode);

                                            employee.Password = CryptoFunctions.EncryptRijndael("abcd@1234", salt);
                                            employee.PasswordSalt = salt;
                                            employee.Designation = Convert.ToString(workSheet.Cells[rowIterator, 5].Value);
                                            employee.EmailId = Convert.ToString(workSheet.Cells[rowIterator, 4].Value);
                                            if (!string.IsNullOrWhiteSpace(Convert.ToString(workSheet.Cells[rowIterator, 8].Value)))
                                            {
                                                var reportTo = await employeeRepo.FindOneAsync(x => x.EmployeeCode == Convert.ToString(workSheet.Cells[rowIterator, 8].Value) && x.IsActive);
                                                employee.ReportingTo = reportTo.EmployeeId;
                                                reporting.Add(Convert.ToInt64(employee.ReportingTo));
                                            }
                                            else
                                            {
                                                employee.ReportingTo = 0;
                                            }
                                            employee.OrganisationId = organisationDetail.OrganisationId;
                                            employee.RoleId = roleDetails.RoleId;
                                            employee.IsActive = true;
                                            employee.CreatedBy = loggedInUserId;
                                            employee.CreatedOn = DateTime.UtcNow;
                                            employee.LoginFailCount = 0;
                                            employeeRepo.Add(employee);
                                            operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                                            await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                                            await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                                            totalRowsAffected++;
                                        }

                                        // add new code Add new user in Tenant DB
                                        var tenantResponse = await AddUserTenantAsync(new UserRequestDomainModel()
                                        {
                                            EmailId = Convert.ToString(workSheet.Cells[rowIterator, 4].Value),
                                            SubDomain = subDomain
                                        }, UserToken);

                                        if (tenantResponse.IsSuccess)
                                        {
                                            var userRequestModel = new UserRequestModel()
                                            {
                                                FirstName = Convert.ToString(workSheet.Cells[rowIterator, 1].Value),
                                                LastName = Convert.ToString(workSheet.Cells[rowIterator, 2].Value),
                                                EmailId = Convert.ToString(workSheet.Cells[rowIterator, 4].Value)
                                            };
                                            //invite the user
                                            await InviteUserAsync(userRequestModel, UserToken);
                                        }
                                    }
                                }
                                string ssoLogin = Configuration.GetSection("Passport:SsoLogin").Value;
                                if (operationStatus.Success && ssoLogin == "false")
                                {
                                    await Task.Run(async () =>
                                {
                                    await notificationsService.BulkUploadNotificationsAndEmailsForExcelAsync(reporting, employeeCodes, jwtToken).ConfigureAwait(false);
                                }).ConfigureAwait(false);

                                    foreach (var user in employeeCodes)
                                    {
                                        var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode.Equals(user) && x.IsActive);
                                        if (employeeDetails != null)
                                        {
                                            var expireTime = DateTime.UtcNow.AddHours(AppConstants.ResetPasswordExpireHoursForNewlyAddedUser);
                                            var token = GenerateJwtToken(employeeDetails, employeeDetails.RoleId, expireTime);
                                            UserToken userAccessToken = new UserToken();
                                            userAccessToken.ExpireTime = expireTime;
                                            userAccessToken.Token = token;
                                            userAccessToken.EmployeeId = employeeDetails.EmployeeId;
                                            userAccessToken.TokenType = 3;
                                            userTokenRepo.Add(userAccessToken);
                                            var status = UnitOfWorkAsync.SaveChanges();
                                            if (status.Success && !string.IsNullOrEmpty(employeeDetails.EmailId))
                                            {
                                                var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
                                                var template = await GetMailerTemplateAsync(TemplateCodes.NCU.ToString(), jwtToken);
                                                string body = template.Body;
                                                string subject = template.Subject != "" ? template.Subject : "";
                                                var firstName = employeeDetails.FirstName;
                                                var lastName = employeeDetails.LastName;
                                                body = body.Replace("<user>", char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1))///.Replace("<token>", AppConstants.ResetPasswordMail + token.TrimEnd())
                                                    .Replace("topBar", keyVault.BlobCdnUrl ?? "" + AppConstants.TopBar).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("MFG", DateTime.Now.ToString("yyyy"))
                                                    .Replace("logo", keyVault.BlobCdnUrl ?? "" + AppConstants.LogoImages).Replace("getStartedButton", keyVault.BlobCdnUrl ?? "" + AppConstants.GetStartedButtonImage)
                                                     .Replace("screen-image", keyVault.BlobCdnUrl ?? "" + AppConstants.ScreenImage).Replace("signInButton", keyVault.BlobCdnUrl ?? "" + AppConstants.LoginButtonImage)
                                                                                  .Replace("<signIn>", settings.FrontEndUrl + "?redirectUrl=unlock-me" + "&empId=" + employeeDetails.EmployeeId).Replace("<resetUrl>", settings.ResetPassUrl + token.TrimEnd()).Replace("srcInstagram", keyVault.BlobCdnUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnUrl + AppConstants.Linkedin)
                                                     .Replace("srcTwitter", keyVault.BlobCdnUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                                                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                                                MailRequest mailRequest = new MailRequest();
                                                mailRequest.MailTo = employeeDetails.EmailId;
                                                mailRequest.Subject = subject;
                                                mailRequest.Body = body;
                                                await SentMailAsync(mailRequest, jwtToken);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                errors.Add(new KeyValuePair<string, string>("ErrorMessage", "Uploaded file does not have any record or not provided required values."));
                            }
                            operationStatus.RecordsAffected = totalRowsAffected;
                            operationStatus.BulkErrors = errors;
                        }
                    }
                }
            }
            return operationStatus;
        }

        public async Task<string> DownloadCsvAsync()
        {
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                csvWriter.WriteField("S.No*");
                csvWriter.WriteField("First Name*");
                csvWriter.WriteField("Last Name*");
                csvWriter.WriteField("Employee Id*");
                csvWriter.WriteField("E-mail Id*");
                csvWriter.WriteField("Designation*");
                csvWriter.WriteField("Role");
                csvWriter.WriteField("Organization*");
                csvWriter.WriteField("Reporting To");
                csvWriter.NextRecord();

                writer.Flush();
                var result = await Task.FromResult(Encoding.UTF8.GetString(mem.ToArray()));
                return result;
            }
        }

        public async Task<IOperationStatus> ChangeUserReportingAsync(EditUserReportingRequest editUserReportingRequest, long loggedInUserId)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var reportingToUser = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == editUserReportingRequest.NewReportingToId);
            if (editUserReportingRequest.EmployeeIds != null && editUserReportingRequest.EmployeeIds.Any() && reportingToUser != null)
            {
                foreach (var item in editUserReportingRequest.EmployeeIds)
                {
                    var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == item && x.IsActive);
                    if (employee != null && employee.ReportingTo != reportingToUser.EmployeeId)
                    {
                        employee.ReportingTo = editUserReportingRequest.NewReportingToId;
                        employee.UpdatedBy = loggedInUserId;
                        employee.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(employee);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                    }
                }
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                
            }

            return operationStatus;
        }

        public async Task<IOperationStatus> ChangeUserOrganisationAsync(ChangeUserOrganisationRequest changeUserOrganisationRequest, long loggedInUserId, string jwtToken)
        {
            var organisation = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == changeUserOrganisationRequest.NewOrganisationId && x.IsActive);
            if (changeUserOrganisationRequest.EmployeeIds != null && changeUserOrganisationRequest.EmployeeIds.Any() && organisation != null)
            {
                List<long> oldOrganisationId = new List<long>();
                List<long> reportingIds = new List<long>();
                List<string> firstNames = new List<string>();
                List<string> emailIds = new List<string>();
                long updatedBy = 1;

                foreach (var item in changeUserOrganisationRequest.EmployeeIds)
                {
                    var employee = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == item && x.IsActive);
                    oldOrganisationId.Add(employee.OrganisationId);
                    reportingIds.Add(Convert.ToInt64(employee.ReportingTo));
                    firstNames.Add(employee.FirstName);

                    if (employee != null && employee.OrganisationId != organisation.OrganisationId)
                    {
                        employee.OrganisationId = changeUserOrganisationRequest.NewOrganisationId;
                        employee.UpdatedBy = loggedInUserId;
                        employee.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(employee);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                    }
                    emailIds.Add(employee.EmailId);
                    updatedBy = Convert.ToInt64(employee.UpdatedBy);
                }
                if (oldOrganisationId != null && firstNames != null && reportingIds != null && emailIds != null)
                {
                    await Task.Run(async () =>
                     {
                         await notificationsService.EditUserNotificationsAndEmailsAsync(oldOrganisationId, changeUserOrganisationRequest.NewOrganisationId, firstNames, reportingIds, updatedBy, emailIds, jwtToken).ConfigureAwait(false);
                     }).ConfigureAwait(false);
                }
            }
            
            return await UnitOfWorkAsync.SaveChangesAsync();
        }

        public void SaveLog(string pageName, string functionName, string errorDetail)
        {
            ErrorLog errorLog = new ErrorLog();
            errorLog.PageName = pageName;
            errorLog.FunctionName = functionName;
            errorLog.ErrorDetail = errorDetail;
            errorLog.CreatedOn = DateTime.UtcNow;
            errorLogRepo.Add(errorLog);
            UnitOfWorkAsync.SaveChanges();

        }

        public UserToken GetAccessTokens(string token, long empId, int tokenType = 1)
        {
            return userTokenRepo.GetQueryable().AsEnumerable().FirstOrDefault(x => x.Token == token && x.EmployeeId == empId && x.TokenType == tokenType);
        }

        public async Task<UserToken> GetEmployeeAccessTokenAsync(long empId)
        {
            return await userTokenRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == empId && x.TokenType == 1);
        }
        public UserToken GetUserTokenByTokenId(string token)
        {
            return userTokenRepo.GetQueryable().AsEnumerable().FirstOrDefault(x => x.Token == token);
        }

        public Employee GetEmployeeByEmpId(long empId)
        {
            return employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == empId && x.IsActive);
        }

        public async Task<Employee> GetUserByMailIdAsync(string mailId)
        {
            return await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == mailId && x.IsActive);
        }

        public async Task<Employee> GetUserByMailId(string mailId)
        {
            return await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == mailId);
        }

        public async Task<Employee> GetUserByEmployeeCodeAsync(string empCode, long employeeId)
        {
            return await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode == empCode && x.EmployeeId != employeeId);
        }

        public async Task<UserDetails> GetUserByEmployeeCodeAsync(string empCode)
        {
            var empDetail = await (from emp in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                                   join reportto in employeeRepo.GetQueryable() on emp.ReportingTo equals reportto.EmployeeId into employee
                                   from rptdtl in employee.DefaultIfEmpty()
                                   where emp.EmployeeCode == empCode
                                   select new UserDetails
                                   {
                                       EmployeeId = emp.EmployeeId,
                                       FirstName = emp.FirstName,
                                       LastName = emp.LastName,
                                       EmployeeCode = emp.EmployeeCode,
                                       EmailId = emp.EmailId,
                                       IsActive = emp.IsActive,
                                       ReportingTo = emp.ReportingTo,
                                       ReportingName = rptdtl.FirstName + " " + rptdtl.LastName,
                                       ImagePath = emp.ImagePath,
                                       OrganisationId = emp.OrganisationId,
                                       OrganisationName = emp.Organisation.OrganisationName,
                                       RoleId = emp.RoleId,
                                       RoleName = emp.RoleMaster.RoleName
                                   }).ToListAsync();
            return empDetail.FirstOrDefault();
        }

        public async Task<IOperationStatus> ChangeRoleAsync(ChangeRoleRequestModel changeRoleRequestModel, long loggedInUserId)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var roleDetails = await roleMasterRepo.FindOneAsync(x => x.RoleId == changeRoleRequestModel.NewRoleId && x.IsActive);
            if (changeRoleRequestModel.EmployeeIds != null && changeRoleRequestModel.EmployeeIds.Any() && roleDetails != null)
            {
                foreach (var users in changeRoleRequestModel.EmployeeIds)
                {
                    var employeeDetails = await employeeRepo.FindOneAsync(x => x.EmployeeId == users);
                    if (!(employeeDetails is null) && changeRoleRequestModel.NewRoleId != employeeDetails.RoleId)
                    {
                        employeeDetails.RoleId = roleDetails.RoleId;
                        employeeDetails.UpdatedBy = loggedInUserId;
                        employeeDetails.UpdatedOn = DateTime.UtcNow;
                        employeeRepo.Update(employeeDetails);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeeDetails.OrganisationId);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                    }
                }
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                
            }
            return operationStatus;
        }

        public async Task<List<string>> GetDesignationAsync(string designation)
        {
            var result = await employeeRepo.GetQueryable().Where(x => x.Designation.StartsWith(designation)).Select(x => x.Designation).Distinct().ToListAsync();
            return result;
        }

        public async Task<IOperationStatus> ResetPasswordAsync(long employeeId, ResetPasswordRequest resetPasswordRequest)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive);
            if (employeeDetails != null)
            {
                string salt = Guid.NewGuid().ToString();
                employeeDetails.Password = CryptoFunctions.EncryptRijndael(resetPasswordRequest.NewPassword, salt);
                employeeDetails.PasswordSalt = salt;
                employeeDetails.LoginFailCount = 0;
                employeeDetails.UpdatedBy = employeeId;
                employeeDetails.UpdatedOn = DateTime.UtcNow;
                employeeRepo.Update(employeeDetails);
                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeeDetails.OrganisationId);
                await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);

                if (operationStatus.Success && !string.IsNullOrEmpty(employeeDetails.EmailId))
                {
                    await Task.Run(async () =>
                    {
                        await notificationsService.ResetPasswordNotificationsAndEmailAsync(employeeDetails, employeeId).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                    var tokendetails = await userTokenRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.ExpireTime > DateTime.UtcNow).ToListAsync();
                    foreach (var item in tokendetails)
                    {
                        item.ExpireTime = DateTime.UtcNow.AddMinutes(-1);
                        userTokenRepo.Update(item);
                        operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                    }
                }
            }
            return operationStatus;
        }

        public async Task<bool> SendResetPasswordMailAsync(SendResetPasswordMailRequest sendResetPasswordMailRequest)
        {
            bool mailSent = false;
            var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == sendResetPasswordMailRequest.EmailId && x.IsActive);
            if (employeeDetails != null)
            {
                var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForResetPassword);
                var token = GenerateJwtToken(employeeDetails, employeeDetails.RoleId, expireTime);
                var getActiveToken = await userTokenRepo.GetQueryable().Where(x => x.EmployeeId == employeeDetails.EmployeeId && x.ExpireTime > DateTime.UtcNow && x.TokenType == 2).ToListAsync();
                foreach (var item in getActiveToken)
                {
                    item.ExpireTime = DateTime.UtcNow.AddMinutes(-1);
                    userTokenRepo.Update(item);
                    UnitOfWorkAsync.SaveChanges();
                }
                UserToken userAccessToken = new UserToken();
                userAccessToken.ExpireTime = expireTime;
                userAccessToken.Token = token;
                userAccessToken.EmployeeId = employeeDetails.EmployeeId;
                userAccessToken.TokenType = 2;
                userTokenRepo.Add(userAccessToken);
                var status = UnitOfWorkAsync.SaveChanges();
                if (status.Success && !string.IsNullOrEmpty(employeeDetails.EmailId))
                {
                    var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
                    var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
                    var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
                    var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
                    var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
                    var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
                    var template = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.FP.ToString());
                    string body = template.Body;
                    string subject = template.Subject != "" ? template.Subject : "";
                    var firstName = employeeDetails.FirstName;
                    var lastName = employeeDetails.LastName;
                    subject = subject.Replace("<user>", char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1));

                    body = body.Replace("<user>", char.ToUpper(firstName[0]) + firstName.Substring(1)).Replace("<token>", settings.ResetPassUrl + token.TrimEnd()).Replace("resetButton", keyVault.BlobCdnUrl ?? "" + AppConstants.ResetButtonImage).Replace("topBar", keyVault.BlobCdnUrl ?? "" + AppConstants.TopBar)
                                .Replace("logo", keyVault.BlobCdnUrl ?? "" + AppConstants.LogoImages).Replace("passwordImage", keyVault.BlobCdnUrl ?? "" + AppConstants.PasswordImage).Replace("login", keyVault.BlobCdnUrl ?? "" + AppConstants.LoginButtonImage).Replace("MFG", DateTime.Now.Year.ToString())
                               .Replace("<userEmail>", employeeDetails.EmailId).Replace("<signIn>", settings.FrontEndUrl + "?redirectUrl=unlock-me" + "&empId=" + employeeDetails.EmployeeId).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("srcInstagram", keyVault.BlobCdnUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnUrl + AppConstants.Linkedin)
                                .Replace("srcTwitter", keyVault.BlobCdnUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                                .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                    MailRequest mailRequest = new MailRequest();
                    mailRequest.MailTo = employeeDetails.EmailId;
                    mailRequest.Subject = subject;
                    mailRequest.Body = body;
                    mailSent = await SentMailWithoutAuthenticationAsync(mailRequest);
                }
            }
            return mailSent;

        }

        public async Task<IOperationStatus> AddUpdateUserContactAsync(UserContactDetail userContactDetail, long loggedInUserId, string jwtToken)
        {
            if (userContactDetail != null)
            {
                var contactDetail = await contactDetailRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == loggedInUserId);
                if (contactDetail != null)
                {
                    contactDetail.CountryStdCode = userContactDetail.CountryStdCode;
                    contactDetail.PhoneNumber = userContactDetail.PhoneNumber;
                    contactDetail.DeskPhoneNumber = userContactDetail.DeskPhoneNumber;
                    contactDetail.SkypeUrl = userContactDetail.SkypeUrl;
                    contactDetail.LinkedInUrl = userContactDetail.LinkedInUrl;
                    contactDetail.TwitterUrl = userContactDetail.TwitterUrl;
                    contactDetail.UpdatedBy = loggedInUserId;
                    contactDetail.UpdatedOn = DateTime.UtcNow;
                    contactDetailRepo.Update(contactDetail);

                    var employee = GetEmployeeByEmpId(loggedInUserId);
                    await Task.Run(async () =>
                    {
                        await notificationsService.AddUpdateUserContactNotificationsAndMailsAsync(employee, loggedInUserId, jwtToken).ConfigureAwait(false);
                    }).ConfigureAwait(false);

                }
                else
                {
                    var employeeContactDetail = Mapper.Map<EmployeeContactDetail>(userContactDetail);
                    employeeContactDetail.EmployeeId = loggedInUserId;
                    employeeContactDetail.CreatedBy = loggedInUserId;
                    employeeContactDetail.CreatedOn = DateTime.UtcNow;
                    contactDetailRepo.Add(employeeContactDetail);
                }
                return await UnitOfWorkAsync.SaveChangesAsync();
            }
            return new OperationStatus();
        }

        public async Task<EmployeeProfileResponse> GetEmployeeProfileByEmployeeIdAsync(long employeeId, string jwtToken)
        {
            var employeeProfileResponse = new EmployeeProfileResponse();
            var employeeDetails = await employeeRepo.GetQueryable().Include(x => x.RoleMaster).FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive);
            if (employeeDetails != null)
            {
                employeeProfileResponse.FirstName = employeeDetails.FirstName;
                employeeProfileResponse.LastName = employeeDetails.LastName;
                employeeProfileResponse.EmployeeCode = employeeDetails.EmployeeCode;
                employeeProfileResponse.EmailId = employeeDetails.EmailId;
                employeeProfileResponse.Designation = employeeDetails.Designation;
                employeeProfileResponse.Role = employeeDetails.RoleMaster.RoleName;
                employeeProfileResponse.ImagePath = employeeDetails.ImagePath;

                var contactDetail = await contactDetailRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeDetails.EmployeeId);
                if (contactDetail != null)
                {
                    employeeProfileResponse.CountryStdCode = contactDetail.CountryStdCode;
                    employeeProfileResponse.PhoneNumber = contactDetail.PhoneNumber;
                    employeeProfileResponse.DeskPhoneNumber = contactDetail.DeskPhoneNumber;
                    employeeProfileResponse.SkypeUrl = contactDetail.SkypeUrl;
                    employeeProfileResponse.TwitterUrl = contactDetail.TwitterUrl;
                    employeeProfileResponse.LinkedInUrl = contactDetail.LinkedInUrl;
                }
                var organisationCycleDetails = await organisationService.GetCurrentCycleAsync(employeeDetails.OrganisationId);
                var organisationDetails = await organisationService.GetOrganisationAsync(employeeDetails.OrganisationId);
                if (organisationCycleDetails != null && organisationDetails != null)
                {
                    var employeeViewDetails = await GetEmployeeScoreDetails(employeeDetails.EmployeeId, Convert.ToInt32(organisationCycleDetails.OrganisationCycleId), Convert.ToInt32(organisationCycleDetails.CycleYear), jwtToken);
                    if (employeeViewDetails != null)
                    {
                        employeeProfileResponse.Department = organisationDetails.OrganisationName;
                        employeeProfileResponse.Team = "";
                        employeeProfileResponse.Score = employeeViewDetails.AvgScore;
                        employeeProfileResponse.Objectives = employeeViewDetails.OkrCount;
                        employeeProfileResponse.KeyResults = employeeViewDetails.KrCount;
                    }
                }
            }
            return employeeProfileResponse;
        }

        public async Task<IOperationStatus> UploadProfileImageAsync(IFormFile file, long loggedInUser)
        {
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            if (keyVault != null)
            {
                string imageGuid = Guid.NewGuid().ToString();
                string strFolderName = Configuration.GetValue<string>("AzureBlob:ProfileImageFolderName");
                string fileExt = System.IO.Path.GetExtension(file.FileName);
                string azureLocation = strFolderName + "/" + imageGuid + fileExt;
                var user = await GetUserByEmployeeIdAsync(loggedInUser);
                var account = new CloudStorageAccount(new StorageCredentials(keyVault.BlobAccountName, keyVault.BlobAccountKey), true);
                var cloudBlobClient = account.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(keyVault.BlobContainerName);
                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    await cloudBlobContainer.SetPermissionsAsync(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        });
                }
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(azureLocation);
                cloudBlockBlob.Properties.ContentType = file.ContentType;
                await cloudBlockBlob.UploadFromStreamAsync(file.OpenReadStream());
                var result = keyVault.BlobCdnUrl + keyVault.BlobContainerName + "/" + azureLocation;
                if (result != "" && user != null)
                {
                    if (user.ImagePath == null)
                    {
                        user.ProfileImageFile = imageGuid + fileExt;
                        user.ImagePath = result;
                        user.UpdatedOn = DateTime.UtcNow;
                        user.UpdatedBy = loggedInUser;
                        employeeRepo.Update(user);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + user.OrganisationId);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                        return await UnitOfWorkAsync.SaveChangesAsync();

                    }
                    else
                    {
                        string deleteAwsLocation = strFolderName + "/" + user.ProfileImageFile;
                        var cloudBlobDelete = cloudBlobContainer.GetBlockBlobReference(deleteAwsLocation);
                        await cloudBlobDelete.DeleteAsync();

                        user.ProfileImageFile = imageGuid + fileExt;
                        user.ImagePath = result;
                        user.UpdatedOn = DateTime.UtcNow;
                        user.UpdatedBy = loggedInUser;
                        employeeRepo.Update(user);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + user.OrganisationId);
                        await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                        return await UnitOfWorkAsync.SaveChangesAsync();
                    }
                }
            }
            return new OperationStatus();
        }

        public async Task<IOperationStatus> DeleteProfileImageAsync(long logedInUser)
        {
            var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
            if (keyVault != null)
            {
                var account = new CloudStorageAccount(new StorageCredentials(keyVault.BlobAccountName, keyVault.BlobAccountKey), true);
                string strFolderName = Configuration.GetValue<string>("AzureBlob:ProfileImageFolderName");
                var cloudBlobClient = account.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(keyVault.BlobContainerName);
                var user = await GetUserByEmployeeIdAsync(logedInUser);
                if (user != null && user.ImagePath != null)
                {
                    string deleteAzureLocation = strFolderName + "/" + user.ProfileImageFile;
                    CloudBlockBlob cloudBlobDelete = cloudBlobContainer.GetBlockBlobReference(deleteAzureLocation);
                    await cloudBlobDelete.DeleteAsync();
                    user.ImagePath = null;
                    user.ProfileImageFile = null;
                    user.UpdatedBy = logedInUser;
                    user.UpdatedOn = DateTime.UtcNow;
                    employeeRepo.Update(user);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + user.OrganisationId);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                    return await UnitOfWorkAsync.SaveChangesAsync();

                }
            }
            return new OperationStatus();
        }

        public async Task<IOperationStatus> ChangePasswordAsync(long employeeId, ChangePasswordRequest changePasswordRequest)
        {
            IOperationStatus operationStatus = new OperationStatus();
            var employeeDetails = await GetUserByEmployeeIdAsync(employeeId);
            if (employeeDetails != null)
            {
                string password = CryptoFunctions.DecryptRijndael(employeeDetails.Password, employeeDetails.PasswordSalt);
                if (password == changePasswordRequest.OldPassword)
                {
                    string salt = Guid.NewGuid().ToString();
                    employeeDetails.Password = CryptoFunctions.EncryptRijndael(changePasswordRequest.NewPassword, salt);
                    employeeDetails.PasswordSalt = salt;
                    employeeDetails.LoginFailCount = 0;
                    employeeDetails.UpdatedBy = employeeId;
                    employeeDetails.UpdatedOn = DateTime.UtcNow;
                    employeeRepo.Update(employeeDetails);
                    operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employeeDetails.OrganisationId);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);

                    if (operationStatus.Success)
                    {
                        var tokenDetails = await userTokenRepo.GetQueryable().Where(x => x.EmployeeId == employeeId && x.ExpireTime > DateTime.UtcNow).ToListAsync();
                        if (tokenDetails != null)
                        {
                            foreach (var item in tokenDetails)
                            {
                                item.ExpireTime = DateTime.UtcNow.AddMinutes(-1);
                                userTokenRepo.Update(item);
                                operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                            }
                        }

                        await Task.Run(async () =>
                        {
                            await notificationsService.ResetPasswordNotificationsAndEmailAsync(employeeDetails, employeeId).ConfigureAwait(false);
                        }).ConfigureAwait(false);
                    }
                }
            }
            return operationStatus;
        }
        public async Task<bool> ChangeAdPasswordAsync(ChangePasswordRequest changePasswordRequest)
        {
            bool success = false;
            try
            {
                Logger.Information("GetUserIdentity called");
                var hasAccessTokenId = HttpContext.Request.Headers.TryGetValue("AccessTokenId", out var accessTokenId);
                Logger.Information("is found the Access TokenId in  header-" + hasAccessTokenId);

                //accessTokenId = "eyJ0eXAiOiJKV1QiLCJub25jZSI6IllCRlBpblA3anROMFJwOHV5MTVlSF9XU3RJYlZwRVVCOWhYcGZRZ045Y1EiLCJhbGciOiJSUzI1NiIsIng1dCI6Imwzc1EtNTBjQ0g0eEJWWkxIVEd3blNSNzY4MCIsImtpZCI6Imwzc1EtNTBjQ0g0eEJWWkxIVEd3blNSNzY4MCJ9.eyJhdWQiOiJodHRwczovL2dyYXBoLm1pY3Jvc29mdC5jb20iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lNjQ4NjI4Zi1mNjVjLTQwY2MtOGEyOC1jNjAxZGFmMjZhODkvIiwiaWF0IjoxNjM0NTU0NzM4LCJuYmYiOjE2MzQ1NTQ3MzgsImV4cCI6MTYzNDU1ODYzOCwiYWNjdCI6MSwiYWNyIjoiMSIsImFpbyI6IkFVUUF1LzhUQUFBQWZlZmc5anc2b1o4VTM2ait6OUpqeWd5dURsaXFUMzRUSHFXVWlwck4vN3FJMTJVbG5JRVJnWkk1YWx4eFAvYTNMREI2QnYrZmp3TXRBQngvek5vWnhRPT0iLCJhbHRzZWNpZCI6IjE6bGl2ZS5jb206MDAwMzQwMDFDRTg3MDA2NCIsImFtciI6WyJwd2QiXSwiYXBwX2Rpc3BsYXluYW1lIjoiT0tSQXV0aG9yaXphdGlvbkFwcC1EZXYiLCJhcHBpZCI6ImFkMmU2MzUxLTVlMjYtNDZlNy05MGJjLWU0NDk5MzRmNDNlMiIsImFwcGlkYWNyIjoiMCIsImVtYWlsIjoicHVqYS5taWNyb3NvbHV0aW9uc0BnbWFpbC5jb20iLCJmYW1pbHlfbmFtZSI6Ikt1bWFyaSIsImdpdmVuX25hbWUiOiJQdWphIiwiaWRwIjoibGl2ZS5jb20iLCJpZHR5cCI6InVzZXIiLCJpcGFkZHIiOiI0Ny4xNS4zLjE1NCIsIm5hbWUiOiJQdWphIE1pY3JvIiwib2lkIjoiNTlkZmVlYWMtN2QwOS00ZTllLWI0NTgtMzczZjA0ZjA1YWIyIiwicGxhdGYiOiIzIiwicHVpZCI6IjEwMDMyMDAxOEVDQ0FDQjQiLCJyaCI6IjAuQVNnQWoySkk1bHoyekVDS0tNWUIydkpxaVZGakxxMG1YdWRHa0x6a1NaTlBRLUlvQUVzLiIsInNjcCI6IkFwcGxpY2F0aW9uLlJlYWQuQWxsIEFwcGxpY2F0aW9uLlJlYWRXcml0ZS5BbGwgQXVkaXRMb2cuUmVhZC5BbGwgRGlyZWN0b3J5LkFjY2Vzc0FzVXNlci5BbGwgRGlyZWN0b3J5LlJlYWQuQWxsIERpcmVjdG9yeS5SZWFkV3JpdGUuQWxsIERvbWFpbi5SZWFkLkFsbCBlbWFpbCBHcm91cC5SZWFkLkFsbCBHcm91cC5SZWFkV3JpdGUuQWxsIEdyb3VwTWVtYmVyLlJlYWQuQWxsIEdyb3VwTWVtYmVyLlJlYWRXcml0ZS5BbGwgb3BlbmlkIE9yZ2FuaXphdGlvbi5SZWFkLkFsbCBPcmdhbml6YXRpb24uUmVhZFdyaXRlLkFsbCBQZW9wbGUuUmVhZCBQZW9wbGUuUmVhZC5BbGwgUHJlc2VuY2UuUmVhZCBQcmVzZW5jZS5SZWFkLkFsbCBwcm9maWxlIFNlY3VyaXR5QWN0aW9ucy5SZWFkLkFsbCBTZWN1cml0eUFjdGlvbnMuUmVhZFdyaXRlLkFsbCBTZWN1cml0eUV2ZW50cy5SZWFkLkFsbCBTZWN1cml0eUV2ZW50cy5SZWFkV3JpdGUuQWxsIFRocmVhdEluZGljYXRvcnMuUmVhZC5BbGwgVGhyZWF0SW5kaWNhdG9ycy5SZWFkV3JpdGUuT3duZWRCeSBVc2VyLkV4cG9ydC5BbGwgVXNlci5JbnZpdGUuQWxsIFVzZXIuTWFuYWdlSWRlbnRpdGllcy5BbGwgVXNlci5SZWFkIFVzZXIuUmVhZC5BbGwgVXNlci5SZWFkQmFzaWMuQWxsIFVzZXIuUmVhZFdyaXRlIFVzZXIuUmVhZFdyaXRlLkFsbCBVc2VyQXV0aGVudGljYXRpb25NZXRob2QuUmVhZCBVc2VyQXV0aGVudGljYXRpb25NZXRob2QuUmVhZC5BbGwgVXNlckF1dGhlbnRpY2F0aW9uTWV0aG9kLlJlYWRXcml0ZSBVc2VyQXV0aGVudGljYXRpb25NZXRob2QuUmVhZFdyaXRlLkFsbCIsInN1YiI6InlqdjU3YksySC1NamdQdy1ROWVPYU1jVE5aVmRPRlpSTGk2ZlpVcXhSM0kiLCJ0ZW5hbnRfcmVnaW9uX3Njb3BlIjoiTkEiLCJ0aWQiOiJlNjQ4NjI4Zi1mNjVjLTQwY2MtOGEyOC1jNjAxZGFmMjZhODkiLCJ1bmlxdWVfbmFtZSI6ImxpdmUuY29tI3B1amEubWljcm9zb2x1dGlvbnNAZ21haWwuY29tIiwidXRpIjoiNW4tbUl6OUxEa21mYlhxQjVOT1VBQSIsInZlciI6IjEuMCIsIndpZHMiOlsiMTNiZDFjNzItNmY0YS00ZGNmLTk4NWYtMThkM2I4MGYyMDhhIl0sInhtc19zdCI6eyJzdWIiOiI3UFFZQjhZdl9keXFKZnQzRzVzUXBaekc5S0wyMjNLZDhRNld0SDZUd3pjIn0sInhtc190Y2R0IjoxNTM3NDUzNjAyfQ.gpOryBYRD99xO2r8xuLl4x78D7GNj1oFEWqeWI3tE_RjviAu-EhCcBHxxUFICZu-zFB5ivcdwoemTXb9kT69RRQFaq_RJLsQNpLSaVm5xa_FgrmZFNc6EBRd3IgparsM8DEjh1o9_BsmoUFNJrR35UaVX7CqiM9hmfPLLsudB_fikfnTqGdr0lH8oklZcPD33q3jHZaAQLUlPT02o6nMjv1FyB4eBc5ceulTqJeDVQjMj8h9G06NjV19ZsOo1mvXsDztqc-wcmtillA63mDs-23vP0a4VoovfkTTcKl-6Ebhxac5Oq2mumrK6IRdxUihpBFpilZT5IFaL8TPoZov1Q";

                HttpClient httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;
                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessTokenId);
                var contentBody = new ChangeADPasswordRequest() { CurrentPassword = changePasswordRequest.OldPassword, NewPassword = changePasswordRequest.NewPassword };

                var JsonData = JsonConvert.SerializeObject(contentBody);
                var content = new StringContent(JsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(AppConstants.ChangePasswordGraphUrl, content);
                success = response.IsSuccessStatusCode;
            }

            catch (Exception)
            {

            }
            return success;
        }

        public async Task<bool> ReSendResetPasswordMailAsync(long employeeId)
        {
            bool mailSent = false;
            var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.IsActive);
            if (employeeDetails != null)
            {
                var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForResetPassword);
                var token = GenerateJwtToken(employeeDetails, employeeDetails.RoleId, expireTime);
                var getActiveToken = await userTokenRepo.GetQueryable().Where(x => x.EmployeeId == employeeDetails.EmployeeId && x.ExpireTime > DateTime.UtcNow).ToListAsync();
                foreach (var item in getActiveToken)
                {
                    item.ExpireTime = DateTime.UtcNow.AddMinutes(-1);
                    userTokenRepo.Update(item);
                    UnitOfWorkAsync.SaveChanges();
                }
                UserToken userAccessToken = new UserToken();
                userAccessToken.ExpireTime = expireTime;
                userAccessToken.Token = token;
                userAccessToken.EmployeeId = employeeDetails.EmployeeId;
                userAccessToken.TokenType = 2;
                userTokenRepo.Add(userAccessToken);
                var status = UnitOfWorkAsync.SaveChanges();
                if (status.Success && !string.IsNullOrEmpty(employeeDetails.EmailId))
                {
                    var template = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.FP.ToString());
                    var keyVault = await KeyVaultService.GetAzureBlobKeysAsync();
                    var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
                    var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
                    var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
                    var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
                    var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
                    string body = template.Body;
                    string subject = template.Subject != "" ? template.Subject : "";
                    subject = subject.Replace("<user>", employeeDetails.FirstName + " " + employeeDetails.LastName);
                    body = body.Replace("<user>", employeeDetails.FirstName + " " + employeeDetails.LastName).Replace("<token>", token.TrimEnd()).Replace("login", keyVault.BlobCdnUrl ?? "" + AppConstants.LoginButtonImage).Replace("MFG", DateTime.Now.Year.ToString()).Replace("srcInstagram", keyVault.BlobCdnUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                    MailRequest mailRequest = new MailRequest();
                    mailRequest.MailTo = employeeDetails.EmailId;
                    mailRequest.Subject = subject;
                    mailRequest.Body = body;
                    mailSent = await SentMailWithoutAuthenticationAsync(mailRequest);
                }
            }
            return mailSent;
        }

        public async Task<RefreshTokenResponse> GetRefreshToken(string jwtToken, long logedInUser)
        {
            RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse();
            var expireTime = DateTime.UtcNow.AddHours(AppConstants.ExpireHoursForLoggedInUser);
            var employeeDetail = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == logedInUser && x.IsActive);
            refreshTokenResponse.TokenId = GenerateJwtToken(employeeDetail, employeeDetail.RoleId, expireTime);
            refreshTokenResponse.ExpireTime = Convert.ToInt32(expireTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            await AddUpdateUserAccessTokenAsync(logedInUser, refreshTokenResponse.TokenId, expireTime, true);
            return refreshTokenResponse;
        }

        public async Task<List<GoalUnlockDate>> GetGoalLockDateAsync(long organisationCycleId)
        {
            var goalUnlockDateDetails = await goalUnlockDateRepo.GetQueryable().Where(x => x.OrganisationCycleId == organisationCycleId).ToListAsync();
            return goalUnlockDateDetails;
        }

        public async Task<IOperationStatus> AddUpdateUserAccessTokenAsync(long employeeId, string token, DateTime expireTime, bool isNewToken = false)
        {
            var userToken = await GetEmployeeAccessTokenAsync(employeeId);
            if (userToken != null)
            {
                userToken.Token = token;
                if (isNewToken)
                {
                    userToken.ExpireTime = expireTime;
                }
                userToken.LastLoginDate = userToken.CurrentLoginDate;
                userToken.CurrentLoginDate = DateTime.UtcNow;
                userTokenRepo.Update(userToken);
            }
            else
            {
                UserToken userAccessToken = new UserToken();
                userAccessToken.ExpireTime = expireTime;
                userAccessToken.CurrentLoginDate = DateTime.UtcNow;
                userAccessToken.Token = token;
                userAccessToken.EmployeeId = employeeId;
                userAccessToken.TokenType = 1;
                userTokenRepo.Add(userAccessToken);
            }
            return await UnitOfWorkAsync.SaveChangesAsync();
        }

        public PageResult<GlobalSearchList> GlobalSearch(string key, int searchType, int page, int pageSize, long employeeId)
        {
            if (key == null)
            {
                return null;
            }

            key = key.ToLower();

            var finalResult = new List<GlobalSearchList>();
            var searchResult = GlobalSearchByKey(key, employeeId, searchType);

            var filteredList_1 = searchResult.Where(e => (e.FirstName + " " + e.LastName).ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList_1);
            searchResult.RemoveAll(e => filteredList_1.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList2 = searchResult.Where(e => e.FirstName.ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList2);
            searchResult.RemoveAll(e => filteredList2.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList3 = searchResult.Where(e => e.LastName.ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList3);
            searchResult.RemoveAll(e => filteredList3.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList4 = searchResult.Where(e => e.OrganisationName.ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList4);
            searchResult.RemoveAll(e => filteredList4.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            finalResult.AddRange(searchResult);

            int totalRecords = finalResult.Count;
            int totalPages = (int)Math.Floor((float)totalRecords / pageSize);
            totalPages = totalPages + (totalRecords % pageSize == 0 ? 0 : 1);
            pageSize = pageSize < 0 ? 5 : pageSize;

            page = page < 0 ? 1 : page;
            page = page > totalPages ? totalPages : page;

            var result = new PageResult<GlobalSearchList>
            {
                Records = finalResult.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                PaggingInfo =
                {
                    Page = page, PageSize = pageSize, TotalRecords = totalRecords, TotalPages = totalPages
                }
            };
            return result;
        }

        public List<GlobalSearchList> GlobalSearchByKey(string finder, long employeeId, int searchType)
        {
            var organizationDetails = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == employeeId && x.IsActive);
            var loggedInEmpParentId = organisationService.GetParentOrganisationIdAsync(organizationDetails.OrganisationId).Result;
            List<GlobalSearchList> finalUsers = new List<GlobalSearchList>();
            var searchList = (from ud1 in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                              join ud2 in employeeRepo.GetQueryable() on ud1.ReportingTo equals ud2.EmployeeId into ps
                              from p in ps.DefaultIfEmpty()
                              where
                              (ud1.EmployeeId.ToString().Contains(finder) || (ud1.FirstName + " " + ud1.LastName).Contains(finder))
                              && ud1.IsActive
                              select
                              new GlobalSearchList
                              {
                                  SearchType = 1,
                                  EmployeeCode = ud1.EmployeeCode,
                                  EmailId = ud1.EmailId,
                                  EmployeeId = ud1.EmployeeId,
                                  OrganisationId = ud1.OrganisationId,
                                  OrganisationName = ud1.Organisation == null ? string.Empty : ud1.Organisation.OrganisationName,
                                  FirstName = ud1.FirstName,
                                  LastName = ud1.LastName,
                                  ImagePath = ud1.ImagePath,
                                  Designation = ud1.Designation,
                                  ReportingTo = p.EmployeeId,
                                  ReportingName = p == null ? string.Empty : p.FirstName + " " + p.LastName,
                                  RoleId = ud1.RoleId,
                                  RoleName = ud1.RoleMaster.RoleName,
                                  ReportingToDesignation = p == null ? string.Empty : p.Designation,
                                  ColorCode = ud1.Organisation.ColorCode == null ? string.Empty : ud1.Organisation.ColorCode,
                                  BackGroundColorCode = ud1.Organisation.BackGroundColorCode == null ? string.Empty : ud1.Organisation.BackGroundColorCode

                              }).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

            foreach (var item in searchList.ToList())
            {
                var ParentId = organisationService.GetParentOrganisationIdAsync(item.OrganisationId).Result;
                if (ParentId != loggedInEmpParentId)
                {
                    searchList.Remove(item);
                }
            }

            finalUsers = searchList.ToList();

            if (searchType == 0)
            {
                var organisationSearchList = (from org in organisationRepo.GetQueryable()
                                              join emp in employeeRepo.GetQueryable() on org.OrganisationHead equals emp.EmployeeId into g
                                              from orgdtl in g.DefaultIfEmpty()
                                              where org.OrganisationName.Contains(finder) && org.IsActive
                                              select
                                              new GlobalSearchList
                                              {
                                                  SearchType = 2,
                                                  EmployeeCode = string.Empty,
                                                  EmailId = string.Empty,
                                                  EmployeeId = orgdtl.EmployeeId,
                                                  OrganisationId = org.OrganisationId,
                                                  OrganisationName = org.OrganisationName == null ? string.Empty : org.OrganisationName,
                                                  FirstName = orgdtl.FirstName == null ? string.Empty : orgdtl.FirstName,
                                                  LastName = orgdtl.LastName == null ? string.Empty : orgdtl.LastName,
                                                  ImagePath = org.ImagePath,
                                                  Designation = string.Empty,
                                                  ReportingTo = null,
                                                  ReportingName = string.Empty,
                                                  RoleId = null,
                                                  RoleName = string.Empty,
                                                  ReportingToDesignation = string.Empty,
                                                  ColorCode = org.ColorCode == null ? string.Empty : org.ColorCode,
                                                  BackGroundColorCode = org.BackGroundColorCode == null ? string.Empty : org.BackGroundColorCode
                                              }).ToList();


                foreach (var itemOrg in organisationSearchList.ToList())
                {
                    var ParentId = organisationService.GetParentOrganisationIdAsync(itemOrg.OrganisationId).Result;
                    if (ParentId != loggedInEmpParentId)
                    {
                        organisationSearchList.Remove(itemOrg);
                    }
                }

                finalUsers.AddRange(organisationSearchList);
            }

            List<long> organisationIdList = finalUsers.GroupBy(x => x.OrganisationId).Select(x => Convert.ToInt64(x.Key)).ToList();

            if (organisationIdList != null && organisationIdList.Any())
            {
                foreach (var orgId in organisationIdList)
                {
                    var parentOrgId = organisationService.GetParentOrganisationIdAsync(orgId).Result;
                    var finalOrgId = parentOrgId == 0 ? orgId : parentOrgId;
                    var cycleDetails = organisationService.GetCurrentCycleAsync(finalOrgId).Result;
                    if (cycleDetails != null)
                    {
                        finalUsers.Where(x => x.OrganisationId == orgId).ToList().ForEach(o =>
                        {
                            o.CycleDuration = cycleDetails.CycleDuration;
                            o.StartDate = cycleDetails.CycleStartDate.Date;
                            o.EndDate = cycleDetails.CycleEndDate?.Date;
                            o.CycleId = cycleDetails.OrganisationCycleId;
                            o.Year = cycleDetails.CycleYear;
                        });

                    }
                }
            }
            return finalUsers;
        }

        public PageResult<SearchUserList> SearchTeamEmployee(string key, long teamId, int page, int pageSize, long employeeId)
        {
            if (key != null)
            {
                key = key.ToLower();
            }
            var finalResult = new List<SearchUserList>();
            var searchResult = SearchTeamEmployeeByKey(key, employeeId, teamId);

            var filteredList_1 = searchResult.Where(e => (e.FirstName + " " + e.LastName).ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList_1);
            searchResult.RemoveAll(e => filteredList_1.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList_2 = searchResult.Where(e => e.FirstName.ToLower().StartsWith(key)).ToList();
            finalResult.AddRange(filteredList_2);
            searchResult.RemoveAll(e => filteredList_2.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            var filteredList_3 = searchResult.Where(e => e.LastName.ToLower().StartsWith(key));
            finalResult.AddRange(filteredList_3);
            searchResult.RemoveAll(e => filteredList_3.Select(x => x.EmployeeId).ToList().Contains(e.EmployeeId));

            finalResult.AddRange(searchResult);

            int totalRecords = finalResult.Count;
            int totalPages = (int)Math.Floor((float)totalRecords / pageSize);
            totalPages = totalPages + (totalRecords % pageSize == 0 ? 0 : 1);
            pageSize = pageSize < 0 ? 5 : pageSize;

            page = page < 0 ? 1 : page;
            page = page > totalPages ? totalPages : page;

            PageResult<SearchUserList> result = new PageResult<SearchUserList>();
            result.Records = finalResult.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            result.PaggingInfo.Page = page;
            result.PaggingInfo.PageSize = pageSize;
            result.PaggingInfo.TotalRecords = totalRecords;
            result.PaggingInfo.TotalPages = totalPages;
            return result;
        }

        public List<SearchUserList> SearchTeamEmployeeByKey(string finder, long employeeId, long teamId)
        {
            List<SearchUserList> finalUsers = new List<SearchUserList>();
            var searchList = (from ud1 in employeeRepo.GetQueryable().Include(x => x.RoleMaster).Include(x => x.Organisation)
                              join ud2 in employeeRepo.GetQueryable() on ud1.ReportingTo equals ud2.EmployeeId into ps
                              from p in ps.DefaultIfEmpty()
                              where
                              ((ud1.EmployeeId.ToString().Contains(finder) || (ud1.FirstName + " " + ud1.LastName).Contains(finder)) && ud1.Organisation.OrganisationId == teamId) && ud1.IsActive
                              select
                              new SearchUserList
                              {
                                  EmployeeCode = ud1.EmployeeCode,
                                  EmailId = ud1.EmailId,
                                  EmployeeId = ud1.EmployeeId,
                                  OrganisationId = ud1.OrganisationId,
                                  OrganisationName = p.Organisation == null ? string.Empty : p.Organisation.OrganisationName,
                                  FirstName = ud1.FirstName,
                                  LastName = ud1.LastName,
                                  ImagePath = ud1.ImagePath,
                                  Designation = ud1.Designation,
                                  ReportingTo = p.EmployeeId,
                                  ReportingName = p == null ? string.Empty : p.FirstName + " " + p.LastName,
                                  RoleId = ud1.RoleId,
                                  RoleName = ud1.RoleMaster.RoleName,
                                  ReportingToDesignation = p == null ? string.Empty : p.Designation
                              }).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToList();

            finalUsers = searchList.ToList();

            List<long> organisationIdList = finalUsers.GroupBy(x => x.OrganisationId).Select(x => Convert.ToInt64(x.Key)).ToList();

            if (organisationIdList.Any())
            {
                foreach (var orgId in organisationIdList)
                {
                    var parentOrgId = organisationService.GetParentOrganisationIdAsync(orgId).Result;
                    var finalOrgId = parentOrgId == 0 ? orgId : parentOrgId;
                    var cycleDetails = organisationService.GetCurrentCycleAsync(finalOrgId).Result;
                    if (cycleDetails != null)
                    {
                        finalUsers.Where(x => x.OrganisationId == orgId).ToList().ForEach(o =>
                        {
                            o.CycleDuration = cycleDetails.CycleDuration;
                            o.StartDate = cycleDetails.CycleStartDate.Date;
                            o.EndDate = cycleDetails.CycleEndDate?.Date;
                            o.CycleId = cycleDetails.OrganisationCycleId;
                            o.Year = cycleDetails.CycleYear;
                        });

                    }
                }
            }
            return finalUsers;
        }

        public async Task<string> GetExistingValidUserTokenAsync(long employeeId)
        {
            var userTokenDetail = await userTokenRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.TokenType == 1 && x.ExpireTime > DateTime.UtcNow);
            return (userTokenDetail != null && !string.IsNullOrEmpty(userTokenDetail.Token)) ? userTokenDetail.Token : string.Empty;
        }

        public async Task<IOperationStatus> AddAdUserAsync(UserRequestModel userRequest, long loggedInUserId, string domain)
        {
            IOperationStatus operationStatus = new OperationStatus();
            GraphServiceClient graphClient = GetGraphServiceClient();

            var userId = string.Empty;
            var adResponse = await IsUserExistInAdAsync(userRequest.EmailId);
            string emailAddress = userRequest.EmailId.Trim() + Configuration.GetValue<string>("AppSecrets:Domain");
            if (!adResponse.IsExist)
            {
                var user = new User
                {
                    AccountEnabled = true,
                    DisplayName = userRequest.FirstName.Trim() + " " + userRequest.LastName.Trim(),
                    MailNickname = userRequest.FirstName.Trim(),
                    UserType = "Guest",
                    JobTitle = userRequest.Designation,
                    UserPrincipalName = userRequest.EmailId.Trim() + Configuration.GetValue<string>("AppSecrets:Domain"),
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = Configuration.GetValue<string>("AppSecrets:DefaultPassword")
                    }
                };
                var response = await graphClient.Users.Request().AddAsync(user);
                if (!string.IsNullOrEmpty(response.Id))
                    userId = response.Id;
            }
            else
            {
                userId = adResponse.Id;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                var roleDetails = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName.Equals(AppConstants.DefaultUserRole));
                var salt = Guid.NewGuid().ToString();
                var employee = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmailId == adResponse.EmailId);
                if (employee == null)
                {
                    employee = new Employee
                    {
                        EmailId = adResponse.EmailId.Trim(),
                        EmployeeCode = userRequest.EmployeeCode,
                        FirstName = userRequest.FirstName.Trim(),
                        LastName = userRequest.LastName.Trim(),
                        Password = CryptoFunctions.EncryptRijndael(Configuration.GetValue<string>("AppSecrets:DefaultPassword"), salt),
                        PasswordSalt = Guid.NewGuid().ToString(),
                        Designation = userRequest.Designation,
                        ReportingTo = userRequest.ReportingTo,
                        OrganisationId = userRequest.OrganizationId,
                        IsActive = true,
                        CreatedBy = loggedInUserId,
                        CreatedOn = DateTime.UtcNow,
                        RoleId = userRequest.RoleId > 0 ? userRequest.RoleId : roleDetails.RoleId,
                        LoginFailCount = 0
                    };
                    employeeRepo.Add(employee);
                    operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);

                    OnBoardingRequest onBoardingRequest = new OnBoardingRequest();
                    onBoardingRequest.EmployeeId = employee.EmployeeId;
                    onBoardingRequest.CreatedBy = loggedInUserId;
                    onBoardingRequest.CreatedOn = DateTime.UtcNow;

                    await SaveDataForOnBoarding(onBoardingRequest, UserToken);
                }
                else if (employee.EmployeeId > 0 && !employee.IsActive)
                {
                    employee.EmployeeCode = userRequest.EmployeeCode;
                    employee.FirstName = userRequest.FirstName.Trim();
                    employee.LastName = userRequest.LastName.Trim();
                    employee.Password = CryptoFunctions.EncryptRijndael(Configuration.GetValue<string>("AppSecrets:DefaultPassword"), salt);
                    employee.PasswordSalt = Guid.NewGuid().ToString();
                    employee.Designation = userRequest.Designation;
                    employee.EmailId = adResponse.EmailId.Trim();
                    employee.ReportingTo = userRequest.ReportingTo;
                    employee.OrganisationId = userRequest.OrganizationId;
                    employee.IsActive = true;
                    employee.CreatedBy = loggedInUserId;
                    employee.CreatedOn = DateTime.UtcNow;
                    employee.RoleId = userRequest.RoleId > 0 ? userRequest.RoleId : roleDetails.RoleId;
                    employee.LoginFailCount = 0;
                    employee.IsActive = true;
                    employeeRepo.Update(employee);
                    operationStatus = await UnitOfWorkAsync.SaveChangesAsync();
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.GetAllUsers);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsById + employee.OrganisationId);
                    await _distributedCache.RemoveAsync(TenantId + AppConstants.TeamsDetails);
                }
                else
                {
                    operationStatus.Success = true;
                }

                if (operationStatus.Success)
                {
                    operationStatus.Entity = employee;
                    await AddUserTenantAsync(new UserRequestDomainModel()
                    {
                        SubDomain = domain,
                        EmailId = emailAddress
                    }, UserToken);
                }

            }
            return operationStatus;
        }

        public async Task<AdUserResponse> IsUserExistInAdAsync(string username)
        {
            var response = new AdUserResponse();
            var isEmail = username.Contains('@');
            if (!isEmail)
                username = username + Configuration.GetValue<string>("AppSecrets:Domain");

            var graphClient = GetGraphServiceClient();
            var graphResponse = await graphClient.Users.Request().Filter("mail eq '" + username + "' or userPrincipalName eq '" + username + "'").GetAsync();

            response.IsExist = graphResponse.Count > 0;
            response.EmailId = username;
            if (graphResponse.Count > 0)
                response.Id = graphResponse.FirstOrDefault()?.Id;
            return response;
        }

        public async Task<bool> IsValidReporting(long empId, long reportingId)
        {
            bool isValidReporting = false;
            var getReportingDetail = await employeeRepo.GetQueryable()
                .FirstOrDefaultAsync(x => x.EmployeeId == reportingId && x.ReportingTo == empId && x.IsActive);
            isValidReporting = getReportingDetail == null ? true : false;
            return isValidReporting;
        }


        #region Private Methods

        private bool IsTeamLeader(long employeeId)
        {
            bool isAnyUser = false;
            var organisationList = organisationRepo.GetQueryable().Where(x => x.OrganisationHead == employeeId && x.IsActive).Select(y => y.OrganisationId).ToList();
            if (organisationList.Count > 0)
            {
                isAnyUser = employeeRepo.GetQueryable().Any(x => organisationList.Contains(x.OrganisationId) && x.IsActive && x.EmployeeId != employeeId);
            }

            return isAnyUser;
        }

        public Organisation GetDefaultOrganization()
        {
            var orgCycle = organisationCycleRepo.GetQueryable().Where(x => x.IsActive && x.IsDiscarded == false && x.IsProcessed == false && x.CycleStartDate < DateTime.UtcNow && x.CycleEndDate > DateTime.UtcNow)?.Select(y => y.OrganisationId);
            return organisationRepo.GetQueryable().FirstOrDefault(x => x.IsActive && x.IsDeleted == false && x.ParentId == 0 && orgCycle.Contains(x.OrganisationId));
        }
        public string GetDefaultDesignation()
        {
            return employeeRepo.GetQueryable().FirstOrDefault(x => x.IsActive)?.Designation;
        }

        private async Task<bool> InviteUserAsync(UserRequestModel userRequestModel, string jwtToken)
        {
            if (string.IsNullOrEmpty(userRequestModel.EmailId)) return false;
            var domainUrl = GetOriginUrl();
            var graphClient = GetGraphServiceClient();
            var invitation = new Invitation
            {
                InvitedUserDisplayName = userRequestModel.FirstName + " " + userRequestModel.LastName,
                InvitedUserEmailAddress = userRequestModel.EmailId,
                InviteRedirectUrl = domainUrl,
                SendInvitationMessage = false,
                InvitedUserMessageInfo = new InvitedUserMessageInfo()
                {
                    CustomizedMessageBody = Configuration.GetValue<string>("AppSecrets:InviteMessage").Replace("<user>", userRequestModel.FirstName + " " + userRequestModel.LastName)
                }
            };
            var response = await graphClient.Invitations.Request().AddAsync(invitation);
            //response.InviteRedeemUrl
            await notificationsService.InviteAdUserEmailsAsync(userRequestModel, domainUrl, jwtToken).ConfigureAwait(false);


            return !string.IsNullOrEmpty(response.Id);
        }
        private async Task<bool> EditUserInADAsync(UserRequestModel userRequestModel)
        {
            if (string.IsNullOrEmpty(userRequestModel.FirstName) && string.IsNullOrEmpty(userRequestModel.LastName)) return false;

            var graphClient = GetGraphServiceClient();

            var userdetails = await IsUserExistInAdAsync(userRequestModel.EmailId);

            var user = new User
            {
                DisplayName = userRequestModel.FirstName + " " + userRequestModel.LastName,
                GivenName = userRequestModel.FirstName,
                Surname = userRequestModel.LastName
            };

            var idemp = userdetails.Id.ToString();

            await graphClient.Users[idemp].Request().UpdateAsync(user);

            return !string.IsNullOrEmpty(idemp);
        }
        private async Task<long> InsertUserTokenAsync(long empId, string token)
        {
            var getUserToken = await userTokenRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == empId && x.TokenType == AppConstants.AzureTokenType);
            if (getUserToken == null)
            {
                getUserToken = new UserToken
                {
                    EmployeeId = empId,
                    Token = token,
                    CurrentLoginDate = DateTime.UtcNow,
                    TokenType = AppConstants.AzureTokenType,
                    ExpireTime = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow
                };
                userTokenRepo.Add(getUserToken);
                await UnitOfWorkAsync.SaveChangesAsync();
            }
            else
            {
                getUserToken.LastLoginDate = getUserToken.CurrentLoginDate;
                getUserToken.CurrentLoginDate = DateTime.UtcNow;
                getUserToken.Token = token;
                userTokenRepo.Update(getUserToken);
                await UnitOfWorkAsync.SaveChangesAsync();
            }
            return getUserToken.Id;
        }
        private async Task<DateTime?> GetUserLastLoginTime(long empId)
        {

            var userToken = await userTokenRepo.GetQueryable().FirstOrDefaultAsync(x =>
               x.EmployeeId == empId && x.TokenType == AppConstants.AzureTokenType);
            var lastLoginTime = userToken != null ? userToken.LastLoginDate : null;
            return lastLoginTime;
        }
        private string GetOriginUrl()
        {
            string domainUrl;
            var hasOrigin = HttpContext.Request.Headers.TryGetValue("OriginHost", out var origin);
            if ((!hasOrigin && HttpContext.Request.Host.Value.Contains("localhost")))
                domainUrl = Configuration.GetValue<string>("FrontEndUrl");
            else
                domainUrl = origin;
            return domainUrl;
        }

        #endregion

    }
}