using System.Threading.Tasks;
using OKRAdminService.ViewModels.Response;

namespace OKRAdminService.Services.Contracts
{
    public interface IKeyVaultService
    {
        Task<BlobVaultResponse> GetAzureBlobKeysAsync();
        Task<ServiceSettingUrlResponse> GetSettingsAndUrlsAsync();
    }
}
