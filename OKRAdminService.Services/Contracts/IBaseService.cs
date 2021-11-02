using OKRAdminService.EF;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System.Net.Http;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface IBaseService
    {
        IUnitOfWorkAsync UnitOfWorkAsync { get; set; }
        IOperationStatus OperationStatus { get; set; }
        OkrAdminDbContext AdminDBContext { get; set; }
        string ConnectionString { get; set; }
        HttpClient GetHttpClient(string jwtToken);
        Task<MailerTemplate> GetMailerTemplateAsync(string templateCode, string jwtToken = null);
        Task<bool> SentMailAsync(MailRequest mailRequest, string jwtToken = null);
        Task<MailerTemplate> GetMailerTemplateWithoutAuthenticationAsync(string templateCode, string jwtToken = null);
        Task<bool> SentMailWithoutAuthenticationAsync(MailRequest mailRequest, string jwtToken = null);
        Task SaveNotificationWithoutAuthenticationAsync(NotificationsRequest notificationsResponse, string jwtToken = null);
        Task<PeopleResponse> GetEmployeeScoreDetails(long empId, int cycle, int year, string jwtToken);
        Task<PayloadCustom<UserTenantModelResponse>> AddUserTenantAsync(UserRequestDomainModel userRequestModel, string jwtToken);
        Task<PayloadCustom<UserTenantModelResponse>> GetTenantAsync(string emailId, string subDomainName, string jwtToken);
        Task<bool> IsMyOkr(string jwtToken = null);
    }
}
