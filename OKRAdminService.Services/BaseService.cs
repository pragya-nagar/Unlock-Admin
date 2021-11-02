using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OKRAdminService.Services
{
    public abstract class BaseService : IBaseService
    {
        public IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        public IOperationStatus OperationStatus { get; set; }
        public OkrAdminDbContext AdminDBContext { get; set; }
        public IConfiguration Configuration { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }
        protected IMapper Mapper { get; private set; }
        protected ILogger Logger { get; private set; }
        protected HttpContext HttpContext => new HttpContextAccessor().HttpContext;
        protected string LoggedInUserEmail => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        protected string UserToken => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "token")?.Value;
        protected string TenantId => HttpContext.User.Identities.FirstOrDefault()?.Claims.FirstOrDefault(x => x.Type == "tenantId")?.Value;
        protected bool IsTokenActive => (!string.IsNullOrEmpty(LoggedInUserEmail) && !string.IsNullOrEmpty(UserToken));
        private IKeyVaultService _keyVaultService;
        public IKeyVaultService KeyVaultService => _keyVaultService ??= HttpContext.RequestServices.GetRequiredService<IKeyVaultService>();
        public string ConnectionString
        {
            get => AdminDBContext?.Database.GetDbConnection().ConnectionString;
            set
            {
                if (AdminDBContext != null)
                    AdminDBContext.Database.GetDbConnection().ConnectionString = value;
            }
        }
        protected BaseService(IServicesAggregator servicesAggregateService)
        {
            UnitOfWorkAsync = servicesAggregateService.UnitOfWorkAsync;
            AdminDBContext = UnitOfWorkAsync.DataContext as OkrAdminDbContext;
            OperationStatus = servicesAggregateService.OperationStatus;
            Configuration = servicesAggregateService.Configuration;
            HostingEnvironment = servicesAggregateService.HostingEnvironment;
            Mapper = servicesAggregateService.Mapper;
            Logger = Log.Logger;
        }

        public HttpClient GetHttpClient(string jwtToken)
        {
            var settings = KeyVaultService.GetSettingsAndUrlsAsync().Result;
            var hasTenant = HttpContext.Request.Headers.TryGetValue("TenantId", out var tenantId);
            if ((!hasTenant && HttpContext.Request.Host.Value.Contains("localhost")))
                tenantId = Configuration.GetValue<string>("TenantId");
            string domain;
            var hasOrigin = HttpContext.Request.Headers.TryGetValue("OriginHost", out var origin);
            if (!hasOrigin && HttpContext.Request.Host.Value.Contains("localhost"))
                domain = Configuration.GetValue<string>("FrontEndUrl");
            else
                domain = string.IsNullOrEmpty(origin) ? string.Empty : origin.ToString();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.NotificationBaseAddress)
            };
            string token = !string.IsNullOrEmpty(jwtToken) ? jwtToken : UserToken;
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            httpClient.DefaultRequestHeaders.Add("TenantId", tenantId.ToString());
            httpClient.DefaultRequestHeaders.Add("OriginHost", domain);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
        protected GraphServiceClient GetGraphServiceClient()
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(Configuration.GetValue<string>("AppSecrets:ClientId"))
                .WithTenantId(Configuration.GetValue<string>("AppSecrets:TenantId"))
                .WithClientSecret(Configuration.GetValue<string>("AppSecrets:ClientSecret"))
                .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            GraphServiceClient graphClient = new GraphServiceClient(authProvider);
            return graphClient;
        }

        public async Task<MailerTemplate> GetMailerTemplateAsync(string templateCode, string jwtToken = null)
        {
            MailerTemplate template = new MailerTemplate();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/v2/OkrNotifications/GetTemplate?templateCode=" + templateCode);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetMailerTemplateAsync completed for template Id: {templateCode}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<MailerTemplate>>(await response.Content.ReadAsStringAsync());
                template = payload.Entity;
            }
            return template;
        }
        public async Task<bool> SentMailAsync(MailRequest mailRequest, string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            PayloadCustom<bool> payload = new PayloadCustom<bool>();
            var response = await httpClient.PostAsJsonAsync($"api/v2/OkrNotifications/SentMailAsync", mailRequest);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"SentMailAsync completed");
                payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
            }
            return payload.IsSuccess;
        }

        public async Task<PeopleResponse> GetEmployeeScoreDetails(long empId, int cycle, int year, string jwtToken)
        {
            PeopleResponse peopleResponse = new PeopleResponse();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            httpClient.BaseAddress = new Uri(settings.OkrBaseAddress);
            var response = await httpClient.GetAsync($"api/Dashboard/EmployeeScoreDetails?empId=" + empId + "&cycle=" + cycle + "&year=" + year);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetEmployeeViewAsync completed for employee Id: {empId}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<PeopleResponse>>(await response.Content.ReadAsStringAsync());
                peopleResponse = payload.Entity;
            }
            return peopleResponse;
        }

        public async Task SaveNotificationAsync(NotificationsRequest notificationsResponse, string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.PostAsJsonAsync($"api/v2/OkrNotifications/InsertNotificationsDetailsAsync", notificationsResponse);
            Logger.Information(response.IsSuccessStatusCode ? "Success" : "Error");
        }

        public async Task<MailerTemplate> GetMailerTemplateWithoutAuthenticationAsync(string templateCode, string jwtToken = null)
        {
            MailerTemplate template = new MailerTemplate();
            var httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.GetAsync($"api/Email/GetTemplateAsync?templateCode=" + templateCode);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetMailerTemplateAsync completed for template Id: {templateCode}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<MailerTemplate>>(await response.Content.ReadAsStringAsync());
                template = payload.Entity;
            }
            return template;
        }
        public async Task<bool> SentMailWithoutAuthenticationAsync(MailRequest mailRequest, string jwtToken = null)
        {
            var httpClient = GetHttpClient(jwtToken);
            PayloadCustom<bool> payload = new PayloadCustom<bool>();
            var response = await httpClient.PostAsJsonAsync($"api/Email/SentMailAsync", mailRequest);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"SentMailAsync completed");
                payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
            }
            return payload.IsSuccess;
        }

        public async Task SaveNotificationWithoutAuthenticationAsync(NotificationsRequest notificationsResponse, string jwtToken = null)
        {
            var httpClient = GetHttpClient(jwtToken);
            var response = await httpClient.PostAsJsonAsync($"api/OkrNotifications/InsertNotificationsDetailsAsync", notificationsResponse);
            if (response.IsSuccessStatusCode)
                Console.Write("Success");
            else
                Console.Write("Error");
        }

        public async Task NotificationsAsync(long by, string notificationText, int appId, long notificationType, int messageType, string jwtToken = null, string url = "", List<long> notificationToList = null, long to = 0)
        {
            var notificationTo = new List<long>();
            NotificationsRequest notificationsRequest = new NotificationsRequest();

            if (notificationToList == null)
            {
                notificationTo.Add(to);

                notificationsRequest.To = notificationTo;
            }
            else
            {
                notificationsRequest.To = notificationToList;
            }
            notificationsRequest.By = by;
            notificationsRequest.Url = url;
            notificationsRequest.Text = notificationText;
            notificationsRequest.AppId = appId;
            notificationsRequest.NotificationType = notificationType;
            notificationsRequest.MessageType = messageType;

            await (jwtToken == null ? SaveNotificationWithoutAuthenticationAsync(notificationsRequest) : SaveNotificationAsync(notificationsRequest, jwtToken));
        }

        public async Task<bool> UpdateTeamLeaderOkr(UpdateTeamLeaderOkrRequest updateTeamLeaderOkrRequest, string jwtToken)
        {
            bool result = false;
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            httpClient.BaseAddress = new Uri(settings.OkrBaseAddress);
            var response = await httpClient.PutAsJsonAsync($"api/MyGoals/UpdateTeamLeaderOkr", updateTeamLeaderOkrRequest);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"UpdateTeamLeaderOkr completed");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
                result = payload.Entity;
            }
            return result;
        }
        public async Task<PayloadCustom<UserTenantModelResponse>> AddUserTenantAsync(UserRequestDomainModel userRequestModel, string jwtToken)
        {
            PayloadCustom<UserTenantModelResponse> peopleResponse = new PayloadCustom<UserTenantModelResponse>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            httpClient.BaseAddress = new Uri(settings.TenantBaseAddress);
            var response = await httpClient.PostAsJsonAsync($"AddUser", userRequestModel);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"AddUserTenantAsync completed for EmailId : {userRequestModel.EmailId} and Domain Name : { userRequestModel.SubDomain}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<UserTenantModelResponse>>(await response.Content.ReadAsStringAsync());
                peopleResponse = payload;
            }
            else
            {
                peopleResponse.Status = (int)response.StatusCode;
                peopleResponse.MessageList.Add("Message", response.ReasonPhrase);

            }
            return peopleResponse;
        }

        public async Task<PayloadCustom<UserTenantModelResponse>> GetTenantAsync(string emailId, string subDomainName, string jwtToken)
        {
            PayloadCustom<UserTenantModelResponse> peopleResponse = new PayloadCustom<UserTenantModelResponse>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            httpClient.BaseAddress = new Uri(settings.TenantBaseAddress);
            var response = await httpClient.GetAsync($"GetTenant?subDomain=" + subDomainName + "&emailId=" + emailId);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetTenantAsync completed for EmailId : {emailId} and Domain Name : { subDomainName}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<UserTenantModelResponse>>(await response.Content.ReadAsStringAsync());
                peopleResponse = payload;
            }
            else
            {
                peopleResponse.Status = (int)response.StatusCode;
                peopleResponse.MessageList.Add("Message", response.ReasonPhrase);

            }
            return peopleResponse;

        }
        public async Task<List<EmailTeamLeaderResponse>> TeamGoalAsync(long teamId, long empId, long cycle, int year, string jwtToken)
        {
            List<EmailTeamLeaderResponse> goalResponse = new List<EmailTeamLeaderResponse>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            httpClient.BaseAddress = new Uri(settings.OkrBaseAddress);
            var response = await httpClient.GetAsync($"api/Dashboard/TeamGoals?teamId=" + teamId + "&empId=" + empId + "&cycle=" + cycle + "&year=" + year);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetEmployeeViewAsync completed for employee Id: {empId}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<List<EmailTeamLeaderResponse>>>(await response.Content.ReadAsStringAsync());
                goalResponse = payload.Entity;
            }
            return goalResponse;
        }

        public async Task<bool> SaveDataForOnBoarding(OnBoardingRequest onBoardingRequest, string jwtToken = null)
        {
            var httpClient = GetHttpClient(jwtToken);
            httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("OnBoarding:BaseUrl"));
            PayloadCustom<bool> payload = new PayloadCustom<bool>();
            var response = await httpClient.PostAsJsonAsync($"api/OnBoarding/OnBoardingActions", onBoardingRequest);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"Data Saved");
                payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
            }
            return payload.IsSuccess;
        }

        public async Task<OnBoardingControlResponse> OnBoardingControlDetailById(string jwtToken = null)
        {
            var onBoardingControlResponse = new OnBoardingControlResponse();
            if (jwtToken != "")
            {
                using var httpClient = GetHttpClient(jwtToken);
                httpClient.BaseAddress = new Uri(Configuration.GetValue<string>("OnBoarding:BaseUrl"));
                using var response = await httpClient.GetAsync($"api/OnBoarding/OnBoardingControlDetailById");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<OnBoardingControlResponse>>(await response.Content.ReadAsStringAsync());
                onBoardingControlResponse = payload.Entity;
            }

            return onBoardingControlResponse;
        }
        public async Task<bool> IsMyOkr(string jwtToken = null)
        {
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            PayloadCustom<bool> payload = new PayloadCustom<bool>();
            httpClient.BaseAddress = new Uri(settings.OkrBaseAddress);
            var response = await httpClient.GetAsync($"api/MyGoals/IsAnyOkr");
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"IsMyOkr completed");
                payload = JsonConvert.DeserializeObject<PayloadCustom<bool>>(await response.Content.ReadAsStringAsync());
            }
            return payload.Entity;
        }
        public async Task<PayloadCustom<TenantMaster>> GetTenantMaster(string tenantId, string jwtToken)
        {
            PayloadCustom<TenantMaster> tenantMaster = new PayloadCustom<TenantMaster>();
            HttpClient httpClient = GetHttpClient(jwtToken);
            var settings = await KeyVaultService.GetSettingsAndUrlsAsync();
            httpClient.BaseAddress = new Uri(settings.TenantBaseAddress);
            var response = await httpClient.GetAsync($"Tenant/"+tenantId);
            if (response.IsSuccessStatusCode)
            {
                Logger.Information($"GetTenantMaster completed for TenantId : {tenantId}");
                var payload = JsonConvert.DeserializeObject<PayloadCustom<TenantMaster>>(await response.Content.ReadAsStringAsync());
                tenantMaster = payload;
            }
            else
            {
                tenantMaster.Status = (int)response.StatusCode;
                tenantMaster.MessageList.Add("Message", response.ReasonPhrase);

            }
            return tenantMaster;

        }

       
    }
}

