using OKRAdminService.ViewModels.Response;
using System.Collections.Generic;

namespace OKRAdminService.ViewModels
{
    public class OkrStatusMasterDetails
    {
        public IList<OkrStatusDetails> OkrStatusDetails { get; set; }
        public IList<ObjectiveDetails> ObjectiveDetails { get; set; }

    }
}
