using System;

namespace OKRAdminService.ViewModels.Response
{
    public class AllUsersResponse
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public long? RoleId { get; set; }
        public string RoleName { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long HeadOrganisationId { get; set; }
        public string HeadOrganisationName { get; set; }
        public bool IsActive { get; set; }
        public long? ReportingTo { get; set; }
        public string ReportingToFirstName { get; set; }
        public string ReportingToLastName { get; set; }
        public string ReportingToImagePath { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public string ReportingToDesignation { get; set; }
        public int LockStatus { get; set; }
        public long? CurrentCycleId { get; set; }
        public int? CurrentCycleYear { get; set; }
        public DateTime? CurrentCycleStartDate { get; set; }
    }
}
