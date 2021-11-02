using System;

namespace OKRAdminService.ViewModels
{
    public class UserDetailResponse
    {
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long RoleId { get; set; }
        public long EmployeeId { get; set; }
        public string EmailId { get; set; }
        public DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public int Status { get; set; }
        public string RoleName { get; set; }
        public long ReportingTo { get; set; }
        public string ImagePath { get; set; }
    }
}
