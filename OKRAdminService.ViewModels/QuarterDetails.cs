using System;

namespace OKRAdminService.ViewModels
{
    public class QuarterDetails
    {
        public long OrganisationCycleId { get; set; }
        public bool IsCurrentQuarter { get; set; }
        public string Symbol { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
