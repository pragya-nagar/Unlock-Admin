using System;
using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class LoginUserDetails
    {
        public long EmployeeId { get; set; }
        public long OrganisationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? RoleId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public bool IsActive { get; set; }
        public long? ReportingTo { get; set; }
        public string ImageDetail { get; set; }
        public DateTime? LastLoginDateTime { get; set; }
        public List<UserRolePermission> RolePermissions { get; set; }
    }
}
