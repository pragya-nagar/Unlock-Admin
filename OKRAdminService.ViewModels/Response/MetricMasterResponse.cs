
using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class MetricMasterResponse
    {
        public int MetricId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<MetricDataMasterResponse> MetricDataMaster { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }
}
