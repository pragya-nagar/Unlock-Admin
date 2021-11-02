
namespace OKRAdminService.ViewModels.Response
{
    public class AssignmentTypeResponse
    {
        public int AssignmentTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }
}
