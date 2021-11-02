using System.Collections.Generic;

namespace OKRAdminService.ViewModels
{
    public class AssignUserRequest
    {
        public long RoleId { get; set; }
        public IList<EmployeeDetailsModel> AssignUsers { get; set; }
    }
}
