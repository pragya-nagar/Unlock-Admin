namespace OKRAdminService.ViewModels.Response
{
    public class UserDetails
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public bool IsActive { get; set; }
        public long? ReportingTo { get; set; }
        public string ReportingName { get; set; }
        public string ImagePath { get; set; }
        public long? RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
