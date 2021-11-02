using Microsoft.EntityFrameworkCore;
using OKRAdminService.Common;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OKRAdminService.Services
{
    public class NotificationsEmailsService : BaseService, INotificationsEmailsService
    {
        private readonly IRepositoryAsync<Employee> employeeRepo;
        private readonly IRepositoryAsync<RoleMaster> roleMasterRepo;
        private readonly IRepositoryAsync<Organisation> organisationRepo;
        private readonly IRepositoryAsync<CycleDurationMaster> cycleDurationMasterRepo;
        private readonly IRepositoryAsync<OrganizationObjective> organisationObjectiveRepo;
        private readonly IKeyVaultService keyVaultService;


        public NotificationsEmailsService(IServicesAggregator servicesAggregateService, IKeyVaultService keyVault) : base(servicesAggregateService)
        {
            employeeRepo = UnitOfWorkAsync.RepositoryAsync<Employee>();
            roleMasterRepo = UnitOfWorkAsync.RepositoryAsync<RoleMaster>();
            cycleDurationMasterRepo = UnitOfWorkAsync.RepositoryAsync<CycleDurationMaster>();
            organisationRepo = UnitOfWorkAsync.RepositoryAsync<Organisation>();
            organisationObjectiveRepo = UnitOfWorkAsync.RepositoryAsync<OrganizationObjective>();
            keyVaultService = keyVault;
        }

        /// <summary>
        /// Mail will be send to users and Reporting manager
        /// Notification will be send to Reporting manager
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task AddUserNotificationsAndEmailsAsync(Employee employee, string jwtToken)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            if (employee.ReportingTo != null)
            {
                var user = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == employee.ReportingTo && x.IsActive);

                var firstName = employee.FirstName;
                var lastName = employee.LastName;
                var designation = employee.Designation;
                if (!(user is null))
                {
                    var templateForOrganisationLeader = await GetMailerTemplateAsync(TemplateCodes.OLNU.ToString(), jwtToken);
                    string templateBody = templateForOrganisationLeader.Body;

                    var initials = $"{employee.FirstName.FirstOrDefault()} {employee.LastName.FirstOrDefault()}";
                    var userDetail = string.Empty + "<tr><td cellpadding =\"0\" cellspacing=\"0\" style=\"padding-bottom: 15px; padding-left: 20px; text-align: center;\" valign=\"top\"><table cellpadding =\"0\" cellspacing=\"0\"  style=\"Margin: auto; background-color: #E3E6EA; border-radius: 80px; padding-right: 10px;\"><tr><td style =\"padding: 0;\"><table cellpadding =\"0\" cellspacing=\"0\" width=\"100%\"><tr><td  width =\"60\" style=\"background-color: #ffe3e3; color: #ff9e9e; text-transform: uppercase; border-radius: 100%; font-size: 20px; text-align: center; padding: 20px 0px; vertical-align: middle;\"><p style =\"  padding: 0; margin: 0; width: 60px;\">" + initials + "</p></td><td style =\"text-align: left; padding: 10px 15px;\"><p style =\"padding: 0; margin: 0; font-weight: bold;\">" + char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1) + "</p><p style =\"padding: 0; margin: 0;\">" + char.ToUpper(designation[0]) + designation.Substring(1) + "</p></td></tr></table></td></tr></table></td></tr>";
                    var loginUrl = settings.FrontEndUrl;
                    if (!string.IsNullOrEmpty(loginUrl))
                    {
                        loginUrl = loginUrl + "?empId=" + user.EmployeeId;
                    }
                    templateBody = templateBody.Replace("<teamLeader>", user.FirstName + " " + user.LastName).Replace("<usern>", employee.FirstName + " " + employee.LastName).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("MFG", DateTime.Now.Year.ToString())
                     .Replace("topBar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages).Replace("<userDetail>", userDetail).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                     .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + user.EmployeeId).Replace("<URL>", settings.FrontEndUrl).Replace("<userDetail>", userDetail).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);


                    MailRequest organisationHeadMailRequest = new MailRequest();
                    if (user.EmailId != null && templateForOrganisationLeader.Subject != "")
                    {
                        organisationHeadMailRequest.MailTo = user.EmailId;
                        organisationHeadMailRequest.Subject = templateForOrganisationLeader.Subject;
                        organisationHeadMailRequest.Body = templateBody;

                        await SentMailAsync(organisationHeadMailRequest, jwtToken);
                    }


                    ///Notifications
                    await NotificationsAsync(user.EmployeeId, AppConstants.NewUserCreationMessage, AppConstants.AppIdForOkrService, (int)NotificationType.NewUserCreation, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", null, user.EmployeeId);

                }

            }

        }

        /// <summary>
        /// Mail will be send to organisation leader
        /// Notification will be send to organisation leader
        /// </summary>
        /// <param name="employeeDetails"></param>
        /// <param name="organisationHead"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task DeleteUserNotificationsAndEmailsAsync(List<Employee> employees, string jwtToken)
        {
            var html = string.Empty;
            var employeeDetails = employees.GroupBy(x => x.OrganisationId);
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            foreach (var emp in employeeDetails)
            {
                var head = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == emp.Key && x.IsActive);
                if (head.OrganisationHead > 0)
                {
                    ///Notification to organisation leader
                    var message = string.Empty;
                    var users = employees.Where(x => x.OrganisationId == emp.Key);
                    message = string.Join(",", users.Select(x => x.FirstName)) + AppConstants.UserRemovalMessage;

                    await NotificationsAsync(emp.Key, message, AppConstants.AppIdForOkrService, (int)NotificationType.RemovalOfUser, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", null, Convert.ToInt64(head.OrganisationHead));

                    var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == head.OrganisationHead && x.IsActive);
                    foreach (var (user, employeeImagePath, designation, firstName, lastName) in from user in users
                                                                                                let employeeImagePath = string.Format("{0} {1}", user.FirstName.FirstOrDefault(), user.LastName.FirstOrDefault())
                                                                                                let designation = user.Designation
                                                                                                let firstName = user.FirstName
                                                                                                let lastName = user.LastName
                                                                                                select (user, employeeImagePath, designation, firstName, lastName))
                    {
                        if (user.ImagePath != null)
                        {
                            html = html + "<tr><td cellpadding =\"0\" cellspacing=\"0\" style=\"padding-bottom: 15px; padding-left: 20px; text-align: center;\" valign=\"top\"><table cellpadding =\"0\" cellspacing=\"0\"  style=\"Margin: auto; background-color: #E3E6EA; border-radius: 80px; padding-right: 10px;\"><tr><td style =\"padding: 0;\"><table cellpadding =\"0\" cellspacing=\"0\" width=\"100%\"><tr><td  width =\"60\"><img src=\"" + user.ImagePath + "\" align =\"left\" width=\"60\" height=\"60\" style=\" width: 60px; height: 60px; border-radius: 100%;\"/></td><td style =\"text-align: left; padding: 10px 15px;\"><p style =\"padding: 0; margin: 0; font-weight: bold;\">" + char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1) + "</p><p style =\"padding: 0; margin: 0;\">" + char.ToUpper(designation[0]) + designation.Substring(1) + "</p></td></tr></table></td></tr></table></td></tr>";
                        }
                        else
                        {
                            html = html + "<tr><td cellpadding =\"0\" cellspacing=\"0\" style=\"padding-bottom: 15px; padding-left: 20px; text-align: center;\" valign=\"top\"><table cellpadding =\"0\" cellspacing=\"0\"  style=\"Margin: auto; background-color: #E3E6EA; border-radius: 80px; padding-right: 10px;\"><tr><td style =\"padding: 0;\"><table cellpadding =\"0\" cellspacing=\"0\" width=\"100%\"><tr><td  width =\"60\" style=\"background-color: #ffe3e3; color: #ff9e9e; text-transform: uppercase; border-radius: 100%; font-size: 20px; text-align: center; padding: 20px 0px; vertical-align: middle;\"><p style =\"  padding: 0; margin: 0; width: 60px;\">" + employeeImagePath + "</p></td ><td style =\"text-align: left; padding: 10px 15px;\"><p style =\"padding: 0; margin: 0; font-weight: bold;\">" + char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1) + "</p><p style =\"padding: 0; margin: 0;\">" + char.ToUpper(designation[0]) + designation.Substring(1) + "</p></td></tr></table></td></tr></table></td></tr>";
                        }
                    }

                    List<Employee> reportingDetailList = (from reporting in users.GroupBy(x => x.ReportingTo)
                                                          let reportingDetail = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == reporting.Key && x.IsActive)
                                                          select reportingDetail).ToList();
                    if (employee != null)
                    {
                        var managerName = employee.FirstName;
                        var template = await GetMailerTemplateAsync(TemplateCodes.UR.ToString(), jwtToken);
                        string body = template.Body;
                        var loginUrl = settings.FrontEndUrl;
                        if (!string.IsNullOrEmpty(loginUrl))
                        {
                            loginUrl = loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employee.EmployeeId;
                        }
                        body = body.Replace("<manager>", char.ToUpper(managerName[0]) + managerName.Substring(1)).Replace("<Details>", html).Replace("MFG", DateTime.Now.Year.ToString())
                      .Replace("topBar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("<mailId>", AppConstants.UnlockSupportEmailId).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId)
              .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages).Replace("<emailId>", AppConstants.UnlockSupportEmailId)
              .Replace("tick", keyVault.BlobCdnCommonUrl + AppConstants.TickImages).Replace("<userEmail>", employee.EmailId).Replace("<signIn>", loginUrl).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage);
                        body = body.Replace("<manager>", employee.FirstName + " " + employee.LastName).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                        MailRequest mailRequest = new MailRequest();
                        if (employee.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = employee.EmailId;
                            mailRequest.Subject = template.Subject;
                            mailRequest.Body = body;
                            if (reportingDetailList != null)
                            {
                                foreach (var reporting in reportingDetailList)
                                {
                                    if (reporting != null)
                                    {
                                        mailRequest.CC = reporting.EmailId;
                                    }
                                }
                            }
                            await SentMailAsync(mailRequest, jwtToken);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Mail will be send to Reporting Managers 
        /// Notification will be send to reporting to manager
        /// </summary>
        /// <param name="bulkList"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task BulkUploadNotificationsAndEmailsForCsvAsync(List<BulkUploadDataModel> bulkList, string jwtToken)
        {
            var reportings = bulkList.GroupBy(x => x.ReportingTo).Select(x => x.Key).ToList();
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settigs = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            ///mail to all Reporting Manager
            foreach (var user in reportings)
            {
                var employeelist = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeCode == user);
                if (!(employeelist is null))
                {
                    var userDetail = string.Empty;
                    var templateForNewUser = await GetMailerTemplateAsync(TemplateCodes.BU.ToString(), jwtToken);
                    var reportingUsers = bulkList.Where(x => x.ReportingTo == user).ToList();
                    foreach (var employee in reportingUsers)
                    {
                        var initials = string.Format("{0} {1}", employee.FirstName.FirstOrDefault(), employee.LastName.FirstOrDefault());
                        var firstName = employee.FirstName;
                        var lastName = employee.LastName;
                        var designation = employee.Designation;
                        userDetail = userDetail + "<tr><td cellpadding =\"0\" cellspacing=\"0\" style=\"padding-bottom: 15px; padding-left: 20px; text-align: center;\" valign=\"top\"><table cellpadding =\"0\" cellspacing=\"0\"  style=\"Margin: auto; background-color: #E3E6EA; border-radius: 80px; padding-right: 10px;\"><tr><td style =\"padding: 0;\"><table cellpadding =\"0\" cellspacing=\"0\" width=\"100%\"><tr><td  width =\"60\" style=\"background-color: #ffe3e3; color: #ff9e9e; text-transform: uppercase; border-radius: 100%; font-size: 20px; text-align: center; padding: 20px 0px; vertical-align: middle;\"><p style =\"  padding: 0; margin: 0; width: 60px;\">" + initials + "</p></td><td style =\"text-align: left; padding: 10px 15px;\"><p style =\"padding: 0; margin: 0; font-weight: bold;\">" + char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1) + "</p><p style =\"padding: 0; margin: 0;\">" + char.ToUpper(designation[0]) + designation.Substring(1) + "</p></td></tr></table></td></tr></table></td></tr>";

                    }
                    string body = templateForNewUser.Body;
                    var loginUrl = settigs.FrontEndUrl;
                    if (!string.IsNullOrEmpty(loginUrl))
                    {
                        loginUrl = loginUrl + "?empId=" + employeelist.EmployeeId;
                    }
                    body = body.Replace("<teamLeader>", employeelist.FirstName + " " + employeelist.LastName).Replace("<userDetail>", userDetail).Replace("MFG", DateTime.Now.Year.ToString());
                    body = body.Replace("topBar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                           .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employeelist.EmployeeId).Replace("<URL>", settigs.FrontEndUrl).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);
                    MailRequest mailRequest = new MailRequest();
                    if (employeelist.EmailId != null && templateForNewUser.Subject != "")
                    {
                        mailRequest.MailTo = employeelist.EmailId;
                        mailRequest.Subject = templateForNewUser.Subject;
                        mailRequest.Body = body;
                        /// mailRequest.CC = employees.EmailId;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    ///Notification to reporting manager
                    await NotificationsAsync(employeelist.EmployeeId, AppConstants.BulkUploadMessage, AppConstants.AppIdForOkrService, (int)NotificationType.BulkUploadCsv, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", null, employeelist.EmployeeId);
                }

            }

        }

        /// <summary>
        /// Mail will be send to Reporting Managers 
        /// Notification will be send to reporting to manager
        /// </summary>
        /// <param name="reporting"></param>
        /// <param name="employeeCodes"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task BulkUploadNotificationsAndEmailsForExcelAsync(List<long> reporting, List<string> employeeCodes, string jwtToken)
        {
            var reportings = reporting.Distinct();
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            if (reportings != null && reportings.Any())
            {

                foreach (var user in reportings)
                {
                    List<Employee> users = new List<Employee>();
                    var reportingDetail = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == user && x.IsActive);
                    users.AddRange(from empCode in employeeCodes
                                   let employee = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeCode.Equals(empCode))
                                   where employee != null && employee.ReportingTo == user
                                   select employee);

                    if (!(reportingDetail is null))
                    {

                        var templateForNewUser = await GetMailerTemplateAsync(TemplateCodes.BU.ToString(), jwtToken);
                        string body = templateForNewUser.Body;
                        var userDetail = string.Empty;
                        foreach (var userData in users)
                        {
                            var initials = string.Format("{0} {1}", userData.FirstName.FirstOrDefault(), userData.LastName.FirstOrDefault());
                            var firstName = userData.FirstName;
                            var lastName = userData.LastName;
                            var designation = userData.Designation;
                            userDetail = userDetail + "<tr><td cellpadding =\"0\" cellspacing=\"0\" style=\"padding-bottom: 15px; padding-left: 20px; text-align: center;\" valign=\"top\"><table cellpadding =\"0\" cellspacing=\"0\"  style=\"Margin: auto; background-color: #E3E6EA; border-radius: 80px; padding-right: 10px;\"><tr><td style =\"padding: 0;\"><table cellpadding =\"0\" cellspacing=\"0\" width=\"100%\"><tr><td  width =\"60\" style=\"background-color: #ffe3e3; color: #ff9e9e; text-transform: uppercase; border-radius: 100%; font-size: 20px; text-align: center; padding: 20px 0px; vertical-align: middle;\"><p style =\"  padding: 0; margin: 0; width: 60px;\">" + initials + "</p></td><td style =\"text-align: left; padding: 10px 15px;\"><p style =\"padding: 0; margin: 0; font-weight: bold;\">" + char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1) + "</p><p style =\"padding: 0; margin: 0;\">" + char.ToUpper(designation[0]) + designation.Substring(1) + "</p></td></tr></table></td></tr></table></td></tr>";

                        }
                        var loginUrl = settings.FrontEndUrl;
                        if (!string.IsNullOrEmpty(loginUrl))
                        {
                            loginUrl = loginUrl + "?empId=" + reportingDetail.EmployeeId;
                        }

                        body = body.Replace("<userDetail>", userDetail).Replace("MFG", DateTime.Now.Year.ToString());
                        body = body.Replace("topBar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages).Replace("<teamLeader>", reportingDetail.FirstName)
                            .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + reportingDetail.EmployeeId).Replace("<URL>", settings.FrontEndUrl).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                             .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                             .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl);

                        MailRequest mailRequest = new MailRequest();
                        if (reportingDetail.EmailId != null && templateForNewUser.Subject != "")///Mail to ReportingTo
                        {
                            mailRequest.MailTo = reportingDetail.EmailId;
                            mailRequest.Subject = templateForNewUser.Subject;
                            mailRequest.Body = body;
                            ///mailRequest.CC = employees.EmailId;
                            await SentMailAsync(mailRequest, jwtToken);
                        }

                        ///Notification To ReportingTo
                        await NotificationsAsync(user, AppConstants.BulkUploadMessage, AppConstants.AppIdForOkrService, (int)NotificationType.BulkUploadCsv, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", null, user);
                    }

                }
            }
        }

        /// <summary>
        /// Password reset Confirmation mail to user
        /// </summary>
        /// <param name="employeeDetails"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public async Task ResetPasswordNotificationsAndEmailAsync(Employee employeeDetails, long employeeId)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var template = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.PRC.ToString());
            string body = template.Body;
            var firstName = employeeDetails.FirstName;
            var lastName = employeeDetails.LastName;
            string subject = template.Subject;
            var loginUrl = settings.FrontEndUrl;
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = loginUrl + "?empId=" + employeeDetails.EmployeeId;
            }
            body = body.Replace("<user>", char.ToUpper(firstName[0]) + firstName.Substring(1) + " " + char.ToUpper(lastName[0]) + lastName.Substring(1)).Replace("topBar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar)
                    .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages).Replace("MFG", DateTime.Now.Year.ToString())
                    .Replace("tick", keyVault.BlobCdnCommonUrl + AppConstants.TickImages).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId)
                     .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employeeDetails.EmployeeId).Replace("<emailId>", AppConstants.UnlockSupportEmailId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
            MailRequest mailRequest = new MailRequest();
            if (template.Subject != "")
            {
                mailRequest.MailTo = employeeDetails.EmailId;
                mailRequest.Subject = subject;
                mailRequest.Body = body;
                await SentMailWithoutAuthenticationAsync(mailRequest);
            }

        }

        /// <summary>
        /// Mail will be send to organisation leader 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="organisation"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task AddOrganisationsNotificationsAndEmailsAsync(OrganisationRequest request, Organisation organisation, string jwtToken)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == request.OrganisationLeader);
            if (!(employee is null))
            {
                var template = await GetMailerTemplateAsync(TemplateCodes.NLO.ToString(), jwtToken);
                string body = template.Body;
                body = body.Replace("<organization>", organisation.OrganisationName).Replace("MFG", DateTime.Now.Year.ToString())
                    .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                MailRequest mailRequest = new MailRequest();
                if (employee.EmailId != null && template.Subject != "")
                {
                    mailRequest.MailTo = employee.EmailId;
                    mailRequest.Subject = template.Subject;
                    mailRequest.Body = body;
                    /// await SentMailAsync(mailRequest, jwtToken);
                }
            }
        }


        /// <summary>
        /// Mail will be send to child organisation leader
        /// </summary>
        /// <param name="request"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task AddChildOrganisationEmailAndNotificationsAsync(ChildRequest request, string jwtToken)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var employee = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == request.LeaderId);
            if (!(employee is null))
            {
                var template = await GetMailerTemplateAsync(TemplateCodes.NLO.ToString(), jwtToken);
                string body = template.Body;
                body = body.Replace("<organization>", request.ChildOrganisationName).Replace("MFG", DateTime.Now.Year.ToString())
                    .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                MailRequest mailRequest = new MailRequest();
                if (employee.EmailId != null && template.Subject != "")
                {
                    mailRequest.MailTo = employee.EmailId;
                    mailRequest.Subject = template.Subject;
                    mailRequest.Body = body;
                    /// await SentMailAsync(mailRequest, jwtToken);
                }
            }
        }

        /// <summary>
        /// When child company settings changed then mail to team leader and all the admins
        /// When child company settings cahnged then notification to team leader and all the admins
        /// While updation if child organisation leader gets changed then mail will be send to old leader and new leader
        /// Notification to all the users of that child organisation plus their reporting manager
        /// </summary>
        /// <param name="request"></param>
        /// <param name="organisation"></param>
        /// <param name="parentId"></param>
        /// <param name="oldOrganisationLeader"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task UpdateChildOrganisationMailAndNotificationsAsync(ChildRequest request, Organisation organisation, long parentId, Organisation oldOrganisationLeader, long updatedBy, OrganisationCycleResponse cycleResponse, string jwtToken)
        {
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var loginUrl = settings.FrontEndUrl;
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            //Mail to admin and organisation leader
            if (organisation != null)
            {
                var newLeader = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == organisation.OrganisationHead && x.IsActive);
                var settingDetails = string.Empty;
                var notificationsettings = string.Empty;
                var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldOrganisationLeader.OrganisationHead && x.IsActive);
                var adminId = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == AppConstants.AdminRole).RoleId;
                var adminList = employeeRepo.GetQueryable().Where(x => x.RoleId == adminId && x.IsActive).Select(x => new { x.FirstName, x.EmailId, x.EmployeeId }).ToList();
                var template = await GetMailerTemplateAsync(TemplateCodes.OSC.ToString(), jwtToken);

                if (organisation.OrganisationName != oldOrganisationLeader.OrganisationName)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization name has been changed from" + " " + oldOrganisationLeader.OrganisationName + " " + "to" + " " + organisation.OrganisationName + ".</td></tr>";
                }
                if (newLeader == null)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been removed for " + organisation.OrganisationName + ".</td></tr>";
                }
                else if (organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been changed:" + " " + newLeader.FirstName + " " + newLeader.LastName + ".</td></tr>";
                }

                ///Mail to admins
                if (adminList != null && template != null)
                {
                    string body = template.Body;
                    body = body.Replace("<company>", organisation.OrganisationName);
                    if (organisation.OrganisationName != oldOrganisationLeader.OrganisationName)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization name has been changed from" + " " + oldOrganisationLeader.OrganisationName + " " + "to" + " " + organisation.OrganisationName + ".</td></tr>";
                    }
                    if (newLeader == null)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been removed for " + organisation.OrganisationName + ".</td></tr>";
                    }
                    else if (organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been changed:" + " " + newLeader.FirstName + " " + newLeader.LastName + ".</td></tr>";
                    }
                    if(settingDetails != "")
                    {
                        body = body.Replace("<settings>", settingDetails).Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("MFG", DateTime.Now.Year.ToString())
                  .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                  .Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("<unlockOkrUrl>", loginUrl + "?redirectUrl=Organization/" + organisation.ParentId + "/" + organisation.OrganisationId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                  .Replace("unlockButton", keyVault.BlobCdnCommonUrl + AppConstants.UnlockButtonImage)
                  .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                  .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                  .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                        MailRequest mailRequest = new MailRequest();
                        foreach (var item in adminList)
                        {
                            var updatedBody = body;
                            updatedBody = updatedBody.Replace("teamLeader", item.FirstName).Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + item.EmployeeId);
                            mailRequest.MailTo = item.EmailId;
                            mailRequest.Subject = template.Subject.Replace("Company", oldOrganisationLeader.OrganisationName);
                            mailRequest.Body = updatedBody;
                            await SentMailAsync(mailRequest, jwtToken);
                        }
                    }
                 
                }

                var message = AppConstants.OrganisationSettingsChangesMessage.Replace("<organisationName>", organisation.OrganisationName).Replace("<settings>", notificationsettings);
                ///Mail to Organisation leader 
                var employee = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == organisation.OrganisationHead && x.RoleId != adminId && x.IsActive);
                var html = string.Empty;
                if (!(employee is null) && template != null)
                {
                    string body = template.Body;
                    body = body.Replace("<company>", organisation.OrganisationName);
                    body = body.Replace("teamLeader", employee.FirstName);
                    if (organisation.OrganisationName != oldOrganisationLeader.OrganisationName)
                    {
                        html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization name has been changed from" + " " + oldOrganisationLeader.OrganisationName + " " + "to" + " " + organisation.OrganisationName + ".</td></tr>";
                    }
                    if (organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead && employeeDetails != null)
                    {
                        html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been changed:" + " " + employeeDetails.FirstName + employeeDetails.LastName + ".</td></tr>";

                    }
                    if(html != "")
                    {
                        body = body.Replace("<settings>", html).Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar)
                        .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                        .Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("<unlockOkrUrl>", loginUrl + "?redirectUrl=Organization/" + organisation.ParentId + "/" + organisation.OrganisationId).Replace("login", Configuration.GetSection("AzureBlob:CdnUrl").Value + AppConstants.LoginButtonImage)
                        .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employee.EmployeeId).Replace("unlockButton", keyVault.BlobCdnCommonUrl + AppConstants.UnlockButtonImage)
                         .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                         .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                         .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);

                        MailRequest mailRequest = new MailRequest();
                        if (employee.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = employee.EmailId;
                            mailRequest.Subject = template.Subject;
                            mailRequest.Body = body;
                            await SentMailAsync(mailRequest, jwtToken);
                        }

                    }
                    if(notificationsettings != "")
                    {
                        await NotificationsAsync(updatedBy, message, AppConstants.AppIdForOkrService, (int)NotificationType.OrganisationSettingsChanges, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "Organization/" + parentId + "/" + organisation.OrganisationId, null, Convert.ToInt64(organisation.OrganisationHead));
                    }
                    ///Notification to team leader
                   
                }

                if(notificationsettings != "")
                {
                    List<long> to = new List<long>();
                    to.AddRange(from item in adminList select item.EmployeeId);
                    await NotificationsAsync(updatedBy, message, AppConstants.AppIdForAdmin, (int)NotificationType.OrganisationSettingsChanges, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "Organization/" + parentId + "/" + organisation.OrganisationId, to);
                }
                ///Notifications to all the admins
              
            }

            ///when child organisation leader changes while updation
            if (request.LeaderId > 0 && organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead)
            {
                var headOrganisationLeader = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == parentId && x.IsActive);

                if (headOrganisationLeader != null)
                {
                    ///Mail to parent organisation leader if the leader of any child organisation gets changed along with other settings
                    var user = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == headOrganisationLeader.OrganisationHead && x.IsActive);
                    if (!(user is null))
                    {
                        var template = await GetMailerTemplateAsync(TemplateCodes.OSC.ToString(), jwtToken);
                        string body = template.Body;
                        body = body.Replace("<company>", oldOrganisationLeader.OrganisationName)
                             .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);

                        MailRequest mailRequest = new MailRequest();
                        if (user.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = user.EmailId;
                            mailRequest.Subject = template.Subject;
                            mailRequest.Body = body;
                            /// await SentMailAsync(mailRequest, jwtToken);
                        }
                    }
                }

                ///Mail to Old Leader
                var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldOrganisationLeader.OrganisationHead && x.IsActive);
                var newLeader = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == organisation.OrganisationHead);
                if (!(employeeDetails is null))
                {
                    var template = await GetMailerTemplateAsync(TemplateCodes.OLO.ToString(), jwtToken);
                    string body = template.Body;
                    body = body.Replace("organisationName", oldOrganisationLeader.OrganisationName).Replace("newLeader", newLeader.FirstName + " " + newLeader.LastName).Replace("oldTeamLeader", employeeDetails.FirstName)
                            .Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("MFG", DateTime.Now.Year.ToString())
                    .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                    .Replace("leaderChangeImage", keyVault.BlobCdnCommonUrl + AppConstants.LeaderChangeImage).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                                        .Replace("<signIn>", loginUrl).Replace("<emailId>", AppConstants.UnlockSupportEmailId)
                                         .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                    MailRequest mailRequest = new MailRequest();
                    if (employeeDetails.EmailId != null && template.Subject != "")
                    {
                        mailRequest.MailTo = employeeDetails.EmailId;
                        mailRequest.Subject = template.Subject;
                        mailRequest.Body = body;
                        await SentMailAsync(mailRequest, jwtToken);
                    }

                    ///Notification To team memebers and their reportings
                    List<long> notificationTo = new List<long>();
                    var organisationEmployees = employeeRepo.GetQueryable().Where(x => x.OrganisationId == request.ChildOrganisationId && x.IsActive && x.ReportingTo > 0).GroupBy(x => x.ReportingTo).Select(x => Convert.ToInt64(x.Key)).ToList(); ///fetched reporting managers

                    var employees = employeeRepo.GetQueryable().Where(x => x.OrganisationId == request.ChildOrganisationId && x.IsActive).ToList();

                    notificationTo.AddRange(from emps in employees
                                            select emps.EmployeeId);
                    notificationTo.AddRange(from reportings in organisationEmployees
                                            select reportings);

                    var message = AppConstants.NewLeaderReplacedOldLeaderMessage.Replace("<new leader>", newLeader.FirstName + " " + newLeader.LastName).Replace("<old leader>", employeeDetails.FirstName + " " + employeeDetails.LastName).Replace("<organizationName>", organisation.OrganisationName);

                    await NotificationsAsync(request.LeaderId, message, AppConstants.AppIdForOkrService, (int)NotificationType.OrganisationLeaderChange, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, AppConstants.NotificationChangeleader, notificationTo);
                }

                if (request.LeaderId > 0)
                {
                    ///Mail to New Leader
                    var employeeDetail = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == request.LeaderId && x.IsActive);
                    if (!(employeeDetail is null))
                    {
                        var template = await GetMailerTemplateAsync(TemplateCodes.NLO.ToString(), jwtToken);
                        string body = template.Body;
                        body = body.Replace("organisationName", organisation.OrganisationName).Replace("newTeamLeader", employeeDetail.FirstName)
                                .Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar)
                    .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages).Replace("MFG", DateTime.Now.Year.ToString())
                    .Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("leaderChangeImage", keyVault.BlobCdnCommonUrl + AppConstants.LeaderChangeImage)
                     .Replace("<signIn>", loginUrl).Replace("<emailId>", AppConstants.UnlockSupportEmailId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                        MailRequest mailRequest = new MailRequest();
                        if (employeeDetail.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = employeeDetail.EmailId;
                            mailRequest.Subject = template.Subject;
                            mailRequest.Body = body;
                            await SentMailAsync(mailRequest, jwtToken);
                        }
                    }

                    var oldTeamGoalDetails = await TeamGoalAsync(oldOrganisationLeader.OrganisationId, oldOrganisationLeader.OrganisationHead.Value, cycleResponse.OrganisationCycleId, cycleResponse.CycleYear.Value, jwtToken);

                    if (oldTeamGoalDetails.Count > 0)
                    {
                        var sourceImportUsers = oldTeamGoalDetails.Select(x => x.EmployeeId).Distinct().ToList();
                        var oldEmployeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldOrganisationLeader.OrganisationHead && x.IsActive);
                        var template = await GetMailerTemplateAsync(TemplateCodes.CTL.ToString(), jwtToken);
                        string body = template.Body;
                        body = body.Replace("organisationName", oldOrganisationLeader.OrganisationName).Replace("newLeader", newLeader.FirstName + " " + newLeader.LastName).Replace("oldTeamLeader", oldEmployeeDetails.FirstName)
                                .Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("MFG", DateTime.Now.Year.ToString())
                        .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                        .Replace("leaderChangeImage", keyVault.BlobCdnCommonUrl + AppConstants.LeaderChangeImage).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                        .Replace("<signIn>", loginUrl).Replace("<emailId>", AppConstants.UnlockSupportEmailId).Replace("hand-shake2", keyVault.BlobCdnCommonUrl + AppConstants.HandShakeImage)
                         .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                         .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                         .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId);


                        if (sourceImportUsers.Count > 0)
                        {
                            foreach (var sourceuser in sourceImportUsers)
                            {
                                var counter = 0;
                                var summary = string.Empty;
                                var oKRlist = oldTeamGoalDetails.Where(x => x.EmployeeId == sourceuser).Take(3).ToList();
                                foreach (var itemkey in oKRlist)
                                {
                                    string kRresult = string.Empty;
                                    counter = counter + 1;
                                    kRresult = itemkey.ObjectiveName.Trim();
                                    summary = summary + "<tr><td valign =\"top\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;padding-right: 3px;\">" + " " + counter + "." + " </td><td valign =\"top\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;\">" + kRresult + "</td></tr>";

                                }

                                MailRequest mailRequest = new MailRequest();

                                var teamMemberDetail = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == sourceuser && x.IsActive);
                                if (teamMemberDetail.EmailId != null && template.Subject != "")
                                {
                                    mailRequest.MailTo = teamMemberDetail.EmailId;
                                    mailRequest.Subject = template.Subject;
                                    var updatedBody = body;
                                    updatedBody = updatedBody.Replace("<Gist>", summary).Replace("Team member name", teamMemberDetail.FirstName.Trim()).Replace("<RedirectOkR>", settings.FrontEndUrl + "?redirectUrl=unlock-me&empId=" + sourceuser);
                                    mailRequest.Body = updatedBody;
                                    await SentMailAsync(mailRequest, jwtToken);
                                }
                            }
                        }
                    }

                }
            }

        }

        /// <summary>
        /// Notification to admin
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task CreateRoleMailAndNotificationsAsync(RoleRequestModel roleRequestModel, string roleCode, string jwtToken)
        {
            var adminId = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == AppConstants.AdminRole).RoleId;
            var employeeId = employeeRepo.GetQueryable().FirstOrDefault(x => x.RoleId == adminId && x.IsActive).EmployeeId;
            var adminList = employeeRepo.GetQueryable().Where(x => x.RoleId == adminId && x.IsActive);
            if (roleCode == CreateEditCodes.CR.ToString())
            {
                if (roleRequestModel != null)
                {
                    List<long> to = new List<long>();

                    to.AddRange(from item in adminList /// Notification to admin
                                select item.EmployeeId);
                    var message = AppConstants.NewRoleCreationMessage.Replace("<roleName>", roleRequestModel.RoleName);
                    await NotificationsAsync(employeeId, message, AppConstants.AppIdForAdmin, (int)NotificationType.NewRoleCreation, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", to);
                }
            }
            else
            {
                List<long> to = new List<long>();
                var message = AppConstants.RoleUpdationMessage.Replace("<roleName>", roleRequestModel.RoleName);
                to.AddRange(from item in adminList /// Notification to admin
                            select item.EmployeeId);
                await NotificationsAsync(employeeId, message, AppConstants.AppIdForAdmin, (int)NotificationType.RoleUpdation, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", to);

            }
        }

        /// <summary>
        /// When company settings changed then mail to team leader and all the admins
        /// When company settings cahnged then notification to team leader and all the admins
        /// While updation if organisation leader gets changed then mail will be send to old leader and new leader
        /// Notification to all the users of that organisation plus their reporting manager
        ///  </summary>
        /// <param name="request"></param>
        /// <param name="organisation"></param>
        /// <param name="oldOrganisationLeader"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task UpdateOrganisationNotificationsAndEmailsAsync(OrganisationRequest request, Organisation organisation, Organisation oldOrganisationLeader, long updatedBy, OrganisationCycle organisationCycle, OrganizationObjective organizationObjective, string jwtToken)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var newLeader = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == organisation.OrganisationHead && x.IsActive);
            var loginUrl = settings.FrontEndUrl;
            var settingDetails = string.Empty;
            var notificationsettings = string.Empty;
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            ///Mail to admin and organisation ledaer when settings change
            if (organisation != null)
            {
                var adminId = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == AppConstants.AdminRole).RoleId;
                var adminList = employeeRepo.GetQueryable().Where(x => x.RoleId == adminId && x.IsActive).Select(x => new { x.FirstName, x.EmailId, x.EmployeeId }).ToList();
                var employeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldOrganisationLeader.OrganisationHead && x.IsActive);

                if (organisation.OrganisationName != oldOrganisationLeader.OrganisationName)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization name has been changed from" + " " + oldOrganisationLeader.OrganisationName + " " + "to" + " " + organisation.OrganisationName + ".</td></tr>";
                }
                if (newLeader == null)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been removed for " + organisation.OrganisationName + ".</td></tr>";
                }
                else if (organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been changed:" + " " + newLeader.FirstName + " " + newLeader.LastName + ".</td></tr>";
                }
                if (organisation.LogoName != oldOrganisationLeader.LogoName)
                {
                    notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + " Logo has been changed. " + "</td></tr>";

                }
                if (request.CycleDuration != organisationCycle.CycleDurationId)
                {
                    var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
                    if (request.CycleDuration == durationMap[CycleDurations.Quarterly])
                    {
                        notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: Quarterly." + "</td></tr>";

                    }
                    else if (request.CycleDuration == durationMap[CycleDurations.HalfYearly])
                    {
                        notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: HalfYearly." + "</td></tr>";

                    }
                    else if (request.CycleDuration == durationMap[CycleDurations.Annually])
                    {
                        notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: Annually." + "</td></tr>";

                    }
                    else if (request.CycleDuration == durationMap[CycleDurations.ThreeYears])
                    {
                        notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: ThreeYears." + "</td></tr>";

                    }
                }
                if (request.IsPrivate != organizationObjective.IsActive)
                {
                    if (request.IsPrivate)
                    {
                        notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Goal Type 'Private' has been enabled." + "</td></tr>";

                    }
                    else
                    {
                        notificationsettings = notificationsettings + "</br><tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Goal Type 'Private' has been disabled." + "</td></tr>";
                    }
                }

                ///Mail to admins
                if (adminList != null)
                {
                    var template = await GetMailerTemplateAsync(TemplateCodes.OSC.ToString(), jwtToken);
                    string body = template.Body;
                    body = body.Replace("<company>", organisation.OrganisationName).Replace("MFG", DateTime.Now.Year.ToString())
                        .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                    if (organisation.OrganisationName != oldOrganisationLeader.OrganisationName)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization name has been changed from" + " " + oldOrganisationLeader.OrganisationName + " " + "to" + " " + organisation.OrganisationName + ".</td></tr>";
                    }
                    if (newLeader == null)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been removed for " + organisation.OrganisationName + ".</td></tr>";
                    }
                    else if (organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been changed:" + " " + newLeader.FirstName + " " + newLeader.LastName + ".</td></tr>";

                    }
                    if (organisation.LogoName != oldOrganisationLeader.LogoName)
                    {
                        settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + " Logo has been changed. " + "</td></tr>";

                    }
                    if (request.CycleDuration != organisationCycle.CycleDurationId)
                    {
                        var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
                        if (request.CycleDuration == durationMap[CycleDurations.Quarterly])
                        {
                            settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: Quarterly." + "</td></tr>";

                        }
                        else if (request.CycleDuration == durationMap[CycleDurations.HalfYearly])
                        {
                            settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: HalfYearly." + "</td></tr>";

                        }
                        else if (request.CycleDuration == durationMap[CycleDurations.Annually])
                        {
                            settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: Annually." + "</td></tr>";

                        }
                        else if (request.CycleDuration == durationMap[CycleDurations.ThreeYears])
                        {
                            settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization cycle has been changed: ThreeYears." + "</td></tr>";

                        }
                    }
                    if (request.IsPrivate != organizationObjective.IsActive)
                    {
                        if (request.IsPrivate)
                        {
                            settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Goal Type 'Private' has been enabled." + "</td></tr>";

                        }
                        else
                        {
                            settingDetails = settingDetails + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Goal Type 'Private' has been disabled." + "</td></tr>";
                        }
                    }

                    if(settingDetails != "")
                    {
                        body = body.Replace("<settings>", settingDetails).Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar)
                   .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                   .Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("<unlockOkrUrl>", loginUrl + "?redirectUrl=Organization/" + 0 + "/" + organisation.OrganisationId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                    .Replace("unlockButton", keyVault.BlobCdnCommonUrl + AppConstants.UnlockButtonImage).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                    .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                    .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                        MailRequest mailRequest = new MailRequest();
                        foreach (var item in adminList)
                        {
                            var updatedBody = body;
                            updatedBody = updatedBody.Replace("teamLeader", item.FirstName).Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + item.EmployeeId);
                            mailRequest.MailTo = item.EmailId;
                            mailRequest.Subject = template.Subject.Replace("Company", oldOrganisationLeader.OrganisationName);
                            mailRequest.Body = updatedBody;
                            await SentMailAsync(mailRequest, jwtToken);
                        }
                    }

                }

                ///Mail to Organisation leader when settings change
                var employee = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == organisation.OrganisationHead && x.RoleId != adminId && x.IsActive);

                var html = string.Empty;
                if (!(employee is null))
                {
                    var template = await GetMailerTemplateAsync(TemplateCodes.OSC.ToString(), jwtToken);
                    string body = template.Body;
                    ///body = body.Replace("<company>", organisation.OrganisationName);
                    body = body.Replace("teamLeader", employee.FirstName).Replace("MFG", DateTime.Now.Year.ToString())
                        .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook)
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                    if (organisation.OrganisationName != oldOrganisationLeader.OrganisationName)
                    {
                        html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organization name has been changed from" + " " + oldOrganisationLeader.OrganisationName + " " + "to" + " " + organisation.OrganisationName + ".</td></tr>";
                    }
                    if (organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead && employeeDetails != null)
                    {
                        html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Leader has been changed:" + " " + employeeDetails.FirstName + employeeDetails.LastName + ".</td></tr>";

                    }
                    if (organisation.LogoName != oldOrganisationLeader.LogoName)
                    {
                        html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + " Logo has been changed. " + "</td></tr>";

                    }
                    if (request.CycleDuration != organisationCycle.CycleDurationId)
                    {
                        var durationMap = await cycleDurationMasterRepo.GetQueryable().ToDictionaryAsync(x => x.CycleDuration.ToEnum<CycleDurations>(), x => x.CycleDurationId);
                        if (request.CycleDuration == durationMap[CycleDurations.Quarterly])
                        {
                            html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organisation cycle has been changed: Quarterly." + "</td></tr>";
                        }
                        else if (request.CycleDuration == durationMap[CycleDurations.HalfYearly])
                        {
                            html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organisation cycle has been changed: HalfYearly." + "</td></tr>";
                        }
                        else if (request.CycleDuration == durationMap[CycleDurations.Annually])
                        {
                            html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organisation cycle has been changed: Annually." + "</td></tr>";

                        }
                        else if (request.CycleDuration == durationMap[CycleDurations.ThreeYears])
                        {
                            html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Organisation cycle has been changed: ThreeYears." + "</td></tr>";
                        }
                    }
                    if (request.IsPrivate != organizationObjective.IsActive)
                    {
                        if (request.IsPrivate)
                        {
                            html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Goal Type 'Private' has been enabled." + "</td></tr>";

                        }
                        html = html + "<tr><td cellpadding =\"0\"  cellspacing=\"0\" style=\"font-family: Calibri; padding-right: 10px; font-size: 16px;\" valign=\"top\">&#149;</td><td cellpadding =\"0\"  cellspacing=\"0\" style=\"padding-bottom: 10px; font-family: Calibri; font-size: 16px;\" valign=\"top\">" + "Goal Type 'Private' has been disabled." + "</td></tr>";

                    }
                    if(html != "")
                    {
                        body = body.Replace("<settings>", html).Replace("topbar", Configuration.GetSection("AzureBlob:CdnUrl").Value + AppConstants.TopBar)
                   .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                   .Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("<unlockOkrUrl>", loginUrl + "?redirectUrl=Organization/" + 0 + "/" + organisation.OrganisationId).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                    .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employee.EmployeeId).Replace("unlockButton", keyVault.BlobCdnCommonUrl + AppConstants.UnlockButtonImage).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                    .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                    .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                        MailRequest mailRequest = new MailRequest();
                        if (employee.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = employee.EmailId;
                            mailRequest.Subject = template.Subject.Replace("Company", oldOrganisationLeader.OrganisationName);
                            mailRequest.Body = body;
                            await SentMailAsync(mailRequest, jwtToken);
                        }
                    }
                   
                }

                ///Notifications to all the admins
                List<long> to = new List<long>();
                to.AddRange(from item in adminList
                            select item.EmployeeId);
                var message = AppConstants.OrganisationSettingsChangesMessage.Replace("<organisationName>", organisation.OrganisationName).Replace("<settings>", notificationsettings);

                if(notificationsettings != "")
                {
                    await NotificationsAsync(updatedBy, message, AppConstants.AppIdForAdmin, (int)NotificationType.OrganisationSettingsChanges, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "Organization/" + 0 + "/" + organisation.OrganisationId, to);

                    ///Notification to team leader

                    await NotificationsAsync(updatedBy, message, AppConstants.AppIdForOkrService, (int)NotificationType.OrganisationSettingsChanges, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "Organization/" + 0 + "/" + organisation.OrganisationId, null, Convert.ToInt64(organisation.OrganisationHead));
                }
              

                if (request.OrganisationLeader > 0 && organisation.OrganisationHead != oldOrganisationLeader.OrganisationHead)
                {
                    /// when leader of parent organisation gets changed notification to that organisation user and to old and new leader (in future notification will be send to all child organisation user also)
                    List<long> notificationTo = new List<long>();

                    ///Mail to Old Leader when leader changes
                    var employeesInfo = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldOrganisationLeader.OrganisationHead && x.IsActive);
                    if (!(employeesInfo is null))
                    {
                        var template = await GetMailerTemplateAsync(TemplateCodes.OLO.ToString(), jwtToken);
                        string body = template.Body;
                        body = body.Replace("organisationName", oldOrganisationLeader.OrganisationName).Replace("newLeader", newLeader.FirstName + " " + newLeader.LastName).Replace("oldTeamLeader", employeesInfo.FirstName)
                            .Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("MFG", DateTime.Now.Year.ToString())
                    .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                    .Replace("leaderChangeImage", keyVault.BlobCdnCommonUrl + AppConstants.LeaderChangeImage).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                                        .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employeesInfo.EmployeeId).Replace("<emailId>", AppConstants.UnlockSupportEmailId)
                                        .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                        MailRequest mailRequest = new MailRequest();
                        if (employeesInfo.EmailId != null && template.Subject != "")
                        {
                            mailRequest.MailTo = employeesInfo.EmailId;
                            mailRequest.Subject = template.Subject;
                            mailRequest.Body = body;
                            await SentMailAsync(mailRequest, jwtToken);
                        }

                        ///Notification To team memebers and to reporting managers and to new and old team leader
                        var organisationEmployees = employeeRepo.GetQueryable().Where(x => x.OrganisationId == request.OrganisationId && x.IsActive && x.ReportingTo > 0).GroupBy(x => x.ReportingTo).Select(x => Convert.ToInt64(x.Key)).ToList();

                        notificationTo.Add(employeesInfo.EmployeeId);
                        notificationTo.Add(newLeader.EmployeeId);

                        var employees = employeeRepo.GetQueryable().Where(x => x.OrganisationId == request.OrganisationId && x.IsActive).ToList();

                        notificationTo.AddRange(from emps in employees
                                                select emps.EmployeeId);
                        notificationTo.AddRange(from reportings in organisationEmployees
                                                select reportings);
                        var messageForTeamMembers = AppConstants.NewLeaderReplacedOldLeaderMessage.Replace("<new leader>", newLeader.FirstName + " " + newLeader.LastName).Replace("<old leader>", employeesInfo.FirstName + " " + employeesInfo.LastName).Replace("<organizationName>", organisation.OrganisationName);
                        await NotificationsAsync(request.OrganisationLeader, messageForTeamMembers, AppConstants.AppIdForOkrService, (int)NotificationType.OrganisationLeaderChange, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, AppConstants.NotificationChangeleader, notificationTo);
                    }



                    if (request.OrganisationLeader > 0)
                    {
                        ///Mail to New Leader

                        var employeeDetail = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == request.OrganisationLeader && x.IsActive);
                        if (!(employeeDetail is null))
                        {
                            var template = await GetMailerTemplateAsync(TemplateCodes.NLO.ToString(), jwtToken);
                            string body = template.Body;
                            body = body.Replace("organisationName", organisation.OrganisationName).Replace("newTeamLeader", employeeDetail.FirstName).Replace("MFG", DateTime.Now.Year.ToString())
                                .Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                    .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                    .Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId).Replace("leaderChangeImage", keyVault.BlobCdnCommonUrl + AppConstants.LeaderChangeImage)
                     .Replace("<signIn>", loginUrl + "?redirectUrl=unlock-me" + "&empId=" + employeeDetail.EmployeeId).Replace("<emailId>", AppConstants.UnlockSupportEmailId).Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
                            MailRequest mailRequest = new MailRequest();
                            if (employeeDetail.EmailId != null && template.Subject != "")
                            {
                                mailRequest.MailTo = employeeDetail.EmailId;
                                mailRequest.Subject = template.Subject;
                                mailRequest.Body = body;
                                await SentMailAsync(mailRequest, jwtToken);
                            }
                        }

                        var oldTeamGoalDetails = await TeamGoalAsync(oldOrganisationLeader.OrganisationId, oldOrganisationLeader.OrganisationHead.Value, organisationCycle.OrganisationCycleId, organisationCycle.CycleYear.Value, jwtToken);

                        if (oldTeamGoalDetails.Count > 0)
                        {
                            var sourceImportUsers = oldTeamGoalDetails.Select(x => x.EmployeeId).Distinct().ToList();
                            var oldEmployeeDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldOrganisationLeader.OrganisationHead && x.IsActive);
                            var template = await GetMailerTemplateAsync(TemplateCodes.CTL.ToString(), jwtToken);
                            string body = template.Body;
                            body = body.Replace("organisationName", oldOrganisationLeader.OrganisationName).Replace("newLeader", newLeader.FirstName + " " + newLeader.LastName).Replace("oldTeamLeader", oldEmployeeDetails.FirstName)
                                    .Replace("topbar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar).Replace("MFG", DateTime.Now.Year.ToString())
                            .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                            .Replace("leaderChangeImage", keyVault.BlobCdnCommonUrl + AppConstants.LeaderChangeImage).Replace("login", keyVault.BlobCdnCommonUrl + AppConstants.LoginButtonImage)
                            .Replace("<signIn>", loginUrl).Replace("<emailId>", AppConstants.UnlockSupportEmailId).Replace("hand-shake2", keyVault.BlobCdnCommonUrl + AppConstants.HandShakeImage)
                             .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                             .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                             .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl).Replace("<supportEmailId>", AppConstants.UnlockSupportEmailId);


                            if (sourceImportUsers.Count > 0)
                            {
                                foreach (var sourceuser in sourceImportUsers)
                                {
                                    var counter = 0;
                                    var summary = string.Empty;
                                    var oKRlist = oldTeamGoalDetails.Where(x => x.EmployeeId == sourceuser).Take(3).ToList();
                                    foreach (var itemkey in oKRlist)
                                    {
                                        string kRresult = string.Empty;
                                        counter = counter + 1;
                                        kRresult = itemkey.ObjectiveName.Trim();
                                        summary = summary + "<tr><td valign =\"top\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;padding-right: 3px;\">" + " " + counter + "." + " </td><td valign =\"top\" cellpadding=\"0\" cellspacing=\"0\" style=\"font-size:16px;line-height:24px;color:#292929;font-family: Calibri,Arial;\">" + kRresult + "</td></tr>";

                                    }

                                    MailRequest mailRequest = new MailRequest();

                                    var teamMemberDetail = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == sourceuser && x.IsActive);
                                    if (teamMemberDetail.EmailId != null && template.Subject != "")
                                    {
                                        mailRequest.MailTo = teamMemberDetail.EmailId;
                                        mailRequest.Subject = template.Subject;
                                        var updatedBody = body;
                                        updatedBody = updatedBody.Replace("<Gist>", summary).Replace("Team member name", teamMemberDetail.FirstName.Trim()).Replace("<RedirectOkR>", settings.FrontEndUrl + "?redirectUrl=unlock-me&empId=" + sourceuser);
                                        mailRequest.Body = updatedBody;
                                        await SentMailAsync(mailRequest, jwtToken);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        ///  When modifiable profile related changes occur mail and notification to user
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="loggedInUserId"></param>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public async Task AddUpdateUserContactNotificationsAndMailsAsync(Employee employee, long loggedInUserId, string jwtToken)
        {
            ///When profile related changes occur mail and notification to user
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;

            var employeeName = employee.FirstName;
            var template = await GetMailerTemplateAsync(TemplateCodes.PI.ToString(), jwtToken);
            string body = template.Body;
            body = body.Replace("<user>", employeeName)
                .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram).Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                     .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter).Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook).Replace("year", Convert.ToString(DateTime.Now.Year))
                     .Replace("fb", facebookUrl).Replace("terp", twitterUrl).Replace("lk", linkedInUrl).Replace("ijk", instagramUrl).Replace("<URL>", settings.FrontEndUrl);
            MailRequest mailRequest = new MailRequest();
            if (employee.EmailId != null && template.Subject != "")
            {
                mailRequest.MailTo = employee.EmailId;
                mailRequest.Subject = template.Subject;
                mailRequest.Body = body;
                /// await SentMailAsync(mailRequest, jwtToken);

            }
            await NotificationsAsync(loggedInUserId, AppConstants.ProfileChangesMessage, AppConstants.AppIdForOkrService, (int)NotificationType.ProfileChanges, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", null, loggedInUserId);

        }

        /// <summary>
        /// When profile picture changes then mail and notification will be send to user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="loggedInUser"></param>
        /// <returns></returns>
        public async Task UploadProfileImageNotificationsAndEmailsAsync(Employee employee, long loggedInUser)
        {
            ///Mail to user when profile image gets updated
            var template = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.PI.ToString());
            string body = template.Body;
            body = body.Replace("<user>", employee.FirstName);
            MailRequest mailRequest = new MailRequest();
            if (employee.EmailId != null && template.Subject != "")
            {
                mailRequest.MailTo = employee.EmailId;
                mailRequest.Subject = template.Subject;
                mailRequest.Body = body;
                ///await SentMailWithoutAuthenticationAsync(mailRequest);

            }

            await NotificationsAsync(loggedInUser, AppConstants.ProfileChangesMessage, AppConstants.AppIdForOkrService, (int)NotificationType.ProfileChanges, (int)MessageTypeForNotifications.NotificationsMessages, null, "", null, loggedInUser);
        }

        public async Task EditUserNotificationsAndEmailsAsync(List<long> oldOrganisationId, long newOrganisationId, List<string> firstNames, List<long> reportingIds, long updatedBy, List<string> emailIds, string jwtToken)
        {
            ///Mail to user when organisation changes
            var adminId = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == AppConstants.AdminRole).RoleId;
            var employeeId = employeeRepo.GetQueryable().FirstOrDefault(x => x.RoleId == adminId && x.IsActive).EmployeeId;
            var adminList = employeeRepo.GetQueryable().Where(x => x.RoleId == adminId && x.IsActive);

            foreach (var mail in emailIds)
            {
                var updatedByUserId = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == updatedBy && x.IsActive);

                var template = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.OC.ToString());
                string body = template.Body;
                body = body.Replace("<admin>", updatedByUserId.FirstName);
                body = body + string.Join(",", firstNames).TrimEnd(',');
                MailRequest mailRequest = new MailRequest();

                mailRequest.MailTo = mail;
                mailRequest.Subject = template.Subject;
                mailRequest.Body = body;
                ///await SentMailAsync(mailRequest, jwtToken);

            }
            var organisation = await organisationRepo.GetQueryable().FirstOrDefaultAsync(x => x.OrganisationId == newOrganisationId && x.IsActive);
            if (organisation.OrganisationHead > 0)
            {
                var organisationHeadDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == organisation.OrganisationHead && x.IsActive);

                ///mail to organisation leader that gains member
                var templateForManagers = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.OCM.ToString());
                string templateBody = templateForManagers.Body;
                templateBody = templateBody.Replace("<team leader>", organisationHeadDetails.FirstName);
                templateBody = templateBody + string.Join(",", firstNames).TrimEnd(',');
                MailRequest managerMailRequest = new MailRequest();
                if (organisationHeadDetails.EmailId != null && templateForManagers.Subject != "")
                {
                    managerMailRequest.MailTo = organisationHeadDetails.EmailId;
                    managerMailRequest.Subject = templateForManagers.Subject;
                    managerMailRequest.Body = templateBody;
                    /// await SentMailAsync(managerMailRequest, jwtToken);
                }

                ///Notification to new team leader 
                List<long> tonewTeamLeaders = new List<long>();

                if (organisationHeadDetails.RoleId != adminId)
                {
                    tonewTeamLeaders.Add(organisationHeadDetails.EmployeeId);
                }
                var newLeaderMessage = AppConstants.UserOrganisationChangeMessage;
                newLeaderMessage = newLeaderMessage.Replace("<organisationName>", organisation.OrganisationName);
                newLeaderMessage = string.Join(",", firstNames) + newLeaderMessage;
                await NotificationsAsync(organisationHeadDetails.EmployeeId, newLeaderMessage, AppConstants.AppIdForOkrService, (int)NotificationType.UserOrganisationChange, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", tonewTeamLeaders);
            }

            var reportingManagersIds = reportingIds.Distinct();

            ///mail to reporting manager
            foreach (var id in reportingManagersIds)
            {
                if (id > 0)
                {
                    var reportingManager = employeeRepo.GetQueryable().FirstOrDefault(x => x.EmployeeId == id && x.IsActive);
                    var templateForReportingManager = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.OCM.ToString());
                    string templateBodyForReportingManager = templateForReportingManager.Body;
                    templateBodyForReportingManager = templateBodyForReportingManager.Replace("<team leader>", reportingManager.FirstName);
                    templateBodyForReportingManager = templateBodyForReportingManager + string.Join(",", firstNames).TrimEnd(',');
                    MailRequest reportingManagerMailRequest = new MailRequest();
                    if (reportingManager.EmailId != null && templateForReportingManager.Subject != "")
                    {
                        reportingManagerMailRequest.MailTo = reportingManager.EmailId;
                        reportingManagerMailRequest.Subject = templateForReportingManager.Subject;
                        reportingManagerMailRequest.Body = templateBodyForReportingManager;
                        /// await SentMailAsync(reportingManagerMailRequest, jwtToken);

                    }
                }
            }

            ///mail to manager that lost team member 
            var distinctOldOrganisationId = oldOrganisationId.Distinct();
            foreach (var leader in distinctOldOrganisationId)
            {
                var oldLeader = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == leader && x.IsActive).OrganisationHead;
                if (oldLeader > 0)
                {
                    var oldLeaderDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldLeader && x.IsActive);
                    var templateForLeaderThatLostMember = await GetMailerTemplateWithoutAuthenticationAsync(TemplateCodes.OCML.ToString());
                    string templateBodyForManagerThatLostMember = templateForLeaderThatLostMember.Body;
                    templateBodyForManagerThatLostMember = templateBodyForManagerThatLostMember.Replace("<team leader>", oldLeaderDetails.FirstName);
                    templateBodyForManagerThatLostMember = templateBodyForManagerThatLostMember + string.Join(",", firstNames).TrimEnd(',');

                    MailRequest oldLeaderMailRequest = new MailRequest();
                    if (oldLeaderDetails.EmailId != null && templateForLeaderThatLostMember.Subject != "")
                    {
                        oldLeaderMailRequest.MailTo = oldLeaderDetails.EmailId;
                        oldLeaderMailRequest.Subject = templateForLeaderThatLostMember.Subject;
                        oldLeaderMailRequest.Body = templateBodyForManagerThatLostMember;
                        /// await SentMailAsync(oldLeaderMailRequest, jwtToken);

                    }
                }
            }

            ///Notification to all the admins
            List<long> to = new List<long>();
            to.AddRange(from ad in adminList
                        select ad.EmployeeId);
            var message = AppConstants.UserOrganisationChangeMessage;
            message = message.Replace("<organisationName>", organisation.OrganisationName);
            message = string.Join(',', firstNames) + message;
            await NotificationsAsync(employeeId, message, AppConstants.AppIdForAdmin, (int)NotificationType.UserOrganisationChange, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", to);

            ///Notification to old team leader 
            List<long> toOldTeamLeaders = new List<long>();
            /// var distinctOrganisationId = oldOrganisationId.Distinct();
            foreach (var organisationId in distinctOldOrganisationId)
            {
                var oldLeader = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == organisationId && x.IsActive).OrganisationHead;
                if (oldLeader > 0)
                {
                    var oldLeaderDetails = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == oldLeader && x.IsActive);
                    if (oldLeaderDetails.RoleId != adminId)
                    {
                        toOldTeamLeaders.Add(oldLeaderDetails.EmployeeId);
                    }
                }
            }
            var oldLeaderMessage = AppConstants.UserOrganisationChangeMessage;
            oldLeaderMessage = oldLeaderMessage.Replace("<organisationName>", organisation.OrganisationName);
            oldLeaderMessage = string.Join(',', firstNames) + oldLeaderMessage;
            await NotificationsAsync(updatedBy, oldLeaderMessage, AppConstants.AppIdForOkrService, (int)NotificationType.UserOrganisationChange, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", toOldTeamLeaders);

        }

        public async Task DeleteUserFromSystemNotificationsAndEmailsAsync(List<Employee> employees, string jwtToken)
        {
            List<long> organisations = new List<long>();
            var organisationIds = employees.GroupBy(x => x.OrganisationId);
            if (organisationIds != null)
            {
                organisations.AddRange(from org in organisationIds
                                       let organisationDetail = organisationRepo.GetQueryable().FirstOrDefault(x => x.OrganisationId == org.Key && x.IsActive)
                                       where organisationDetail.ParentId == 0
                                       select Convert.ToInt64(organisationDetail.OrganisationId));
            }
            var adminId = roleMasterRepo.GetQueryable().FirstOrDefault(x => x.RoleName == AppConstants.AdminRole).RoleId;
            var employeeId = employeeRepo.GetQueryable().FirstOrDefault(x => x.RoleId == adminId && x.IsActive).EmployeeId;
            var adminList = employeeRepo.GetQueryable().Where(x => x.RoleId == adminId && x.IsActive);

            ///Notification to all the admins
            List<long> to = new List<long>();
            to.AddRange(from ad in adminList
                        select ad.EmployeeId);
            var message = string.Empty;
            foreach (var org in organisations)
            {
                var users = employees.Where(x => x.OrganisationId == org).ToList();
                message = string.Join(',', users.Select(x => x.FirstName)) + " " + AppConstants.UserRemovalFromSystem;
            }
            if (employeeId > 0)
            {
                await NotificationsAsync(employeeId, message, AppConstants.AppIdForAdmin, (int)NotificationType.UserDeletionFromSystem, (int)MessageTypeForNotifications.NotificationsMessages, jwtToken, "", to);
            }
        }

        public async Task InviteAdUserEmailsAsync(UserRequestModel userRequestModel, string httpsSubDomain, string jwtToken)
        {
            var keyVault = await keyVaultService.GetAzureBlobKeysAsync();
            var settings = await keyVaultService.GetSettingsAndUrlsAsync();
            var facebookUrl = Configuration.GetSection("OkrFrontendURL:FacebookURL").Value;
            var twitterUrl = Configuration.GetSection("OkrFrontendURL:TwitterUrl").Value;
            var linkedInUrl = Configuration.GetSection("OkrFrontendURL:LinkedInUrl").Value;
            var instagramUrl = Configuration.GetSection("OkrFrontendURL:InstagramUrl").Value;
            var user = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmployeeId == userRequestModel.EmployeeId && x.IsActive);
            var logedInUser = await employeeRepo.GetQueryable().FirstOrDefaultAsync(x => x.EmailId == LoggedInUserEmail && x.IsActive);
            string logedInUserFirstName = string.Empty;
            string logedInUserLastName = string.Empty;
            if (logedInUser != null)
            {
                logedInUserFirstName = logedInUser.FirstName;
                logedInUserLastName = logedInUser.LastName;

            }
            var firstName = userRequestModel.FirstName;
            var lastName = userRequestModel.LastName;

            if (!(user is null))
            {
                var template = await GetMailerTemplateAsync(TemplateCodes.IADU.ToString(), jwtToken);
                string templateBody = template.Body;
                templateBody = templateBody.Replace("topBar", keyVault.BlobCdnCommonUrl + AppConstants.TopBar)
                                         .Replace("<name>", firstName + " " + lastName)
                                          .Replace("<invitebyuser>", logedInUserFirstName + " " + logedInUserLastName)
                                          .Replace("logo", keyVault.BlobCdnCommonUrl + AppConstants.LogoImages)
                                                 .Replace("<url>", httpsSubDomain)
                                                    .Replace("srcFacebook", keyVault.BlobCdnCommonUrl + AppConstants.Facebook)
                                                    .Replace("srcInstagram", keyVault.BlobCdnCommonUrl + AppConstants.Instagram)
                                                                              .Replace("srcTwitter", keyVault.BlobCdnCommonUrl + AppConstants.Twitter)
                                                                              .Replace("srcLinkedin", keyVault.BlobCdnCommonUrl + AppConstants.Linkedin)
                                                                              .Replace("ijk", instagramUrl).Replace("lk", linkedInUrl)
                                                                              .Replace("fb", facebookUrl).Replace("terp", twitterUrl)
                                                    .Replace("<credentials>", keyVault.BlobCdnCommonUrl + AppConstants.Credentials)
                                                      .Replace("domainurl", httpsSubDomain)
                                                         .Replace("<durl>", httpsSubDomain)
                                                         .Replace("<domainId>", userRequestModel.EmailId)
                                                      .Replace("<password>", "create PASSWORD by clicking below button")
                                                 .Replace("handshake", keyVault.BlobCdnCommonUrl + AppConstants.Handshake);

                MailRequest organisationHeadMailRequest = new MailRequest();
                if (user.EmailId != null && template.Subject != "")
                {
                    organisationHeadMailRequest.MailTo = user.EmailId;
                    organisationHeadMailRequest.Subject = template.Subject;
                    organisationHeadMailRequest.Body = templateBody;

                    await SentMailAsync(organisationHeadMailRequest, jwtToken);
                }




            }



        }

    }
}

