using System.Collections.Generic;

namespace OKRAdminService.ViewModels
{
    public class PermissionRoleResponseModel
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool Status { get; set; }
        public long TotalAssignedUsers { get; set; }
        public List<PermissionDetailModel> Permission { get; set; }
        public List<EmployeeInformation> AssignUsers { get; set; }
    }
}
