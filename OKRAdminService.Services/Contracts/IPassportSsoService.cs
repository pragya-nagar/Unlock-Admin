using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface IPassportSsoService
    {
        Task<UserLoginResponse> SsoLoginAsync(SsoLoginRequest ssoLoginRequest);
        Task<List<PassportEmployeeResponse>> ActiveUserAsync();
        Task<List<PassportEmployeeResponse>> InActiveUserAsync();
        Task<List<PassportEmployeeResponse>> GetAllPassportUsersAsync();
    }
}
