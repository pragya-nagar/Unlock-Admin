using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class CycleDetails
    {
        public string Year { get; set; }
        public bool IsCurrentYear { get; set; }
        public List<QuarterDetails> QuarterDetails { get; set; }
    }
}
