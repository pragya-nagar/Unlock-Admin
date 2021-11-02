using System;

namespace OKRAdminService.ViewModels
{
    public class OrganisationCycleResponse
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long OrganisationCycleId { get; set; }
        public string Symbol { get; set; }
        public int? CycleYear { get; set; }
        public string CycleDuration { get; set; }
        public DateTime CycleStartDate { get; set; }
        public DateTime? CycleEndDate { get; set; }
    }
}
