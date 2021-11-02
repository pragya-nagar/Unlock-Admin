
namespace OKRAdminService.ViewModels.Response
{
    public class MetricDataMasterResponse
    {
        public  int DataId { get; set; }
        public int MetricId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string Symbol { get; set; }
        public bool IsDefault { get; set; }
    }
}
