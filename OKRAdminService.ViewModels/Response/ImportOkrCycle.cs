using System;

namespace OKRAdminService.ViewModels.Response
{
    public class ImportOkrCycle
    {
        public long OrganizationCycleId { get; set; }
        public string Symbol { get; set; }
        public int CycleYear { get; set; }
        public DateTime? CycleStartDate { get; set; }
        public DateTime? CycleEndDate { get; set; }
    }
}
