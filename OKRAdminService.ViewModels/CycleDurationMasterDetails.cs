using System.Collections.Generic;

namespace OKRAdminService.ViewModels
{
    public class CycleDurationMasterDetails
    {
        public long CycleDurationId { get; set; }
        public string CycleDuration { get; set; }
        public bool IsActive { get; set; }
       public List<OrganisationCycleResponse> OrganisationCycleDetails { get; set; }
    }
}
