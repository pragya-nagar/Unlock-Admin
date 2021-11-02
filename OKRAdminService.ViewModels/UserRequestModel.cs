namespace OKRAdminService.ViewModels
{
    public class UserRequestModel
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public string Designation { get; set; }
        public long RoleId { get; set; }
        public long OrganizationId { get; set; }
        public long? ReportingTo { get; set; }
    }
}
