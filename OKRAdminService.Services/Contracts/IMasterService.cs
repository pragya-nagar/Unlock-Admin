using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface IMasterService
    {
        Task<MasterResponse> GetAllMasterDetailsAsync();
        Task<OkrStatusMasterDetails> GetOkrFiltersMasterAsync(long organisationId);
        Task<List<AssignmentTypeResponse>> GetAssignmentTypeMasterAsync();
        Task<List<MetricMasterResponse>> GetAllMetricMasterAsync();
        Task<GetAllOkrMaster> GetAllOkrMaster();
    }
}
