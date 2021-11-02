using System;

namespace OKRAdminService.ViewModels.Response
{
    public class CycleResponse
    {
        public long OrganisationCycleId { get; set; }
        public string Symbol { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
