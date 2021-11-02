using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class OrganisationCycleDetails
    {
        public string OrganisationName { get; set; }
        public string OrganisationId { get; set; }
        public long CycleDurationId { get; set; }
        public string CycleDuration { get; set; }
        public string CycleStart { get; set; }
        public List<CycleDetails> CycleDetails { get; set; }
    }
}
