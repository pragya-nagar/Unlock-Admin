using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Requests
{
    public class RoleUpdateRequest
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public bool Status { get; set; }
        public IList<EmployeeDetailsModel> AssignUsers { get; set; }
    }
}
