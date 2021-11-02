namespace OKRAdminService.ViewModels
{
    public class RoleResponseModel
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool Status { get; set; }
        public long TotalAssignedUsers { get; set; }
    }
}
