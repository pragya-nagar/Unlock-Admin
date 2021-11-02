using System;

namespace OKRAdminService.ViewModels.Response
{
    public class GlobalSearchList
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long? RoleId { get; set; }
        public string RoleName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmailId { get; set; }
        public long? ReportingTo { get; set; }
        public string ReportingName { get; set; }
        public string ReportingToDesignation { get; set; }
        public string ImagePath { get; set; }
        public long CycleId { get; set; }
        public string CycleDuration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Year { get; set; }
        public long SearchType { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
    }
}
