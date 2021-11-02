namespace OKRAdminService.ViewModels.Response
{
    public class PermissionMasterDetails
    {
        public long PermissionId { get; set; }
        public long ModuleId { get; set; }
        public string Permission { get; set; }
        public bool IsActive { get; set; }
    }
}
