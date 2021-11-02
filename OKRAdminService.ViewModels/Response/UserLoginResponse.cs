using System;
using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class UserLoginResponse
    {
        public string TokenId { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Designation { get; set; }
        public long EmployeeId { get; set; }
        public string EmailId { get; set; }
        public string EmployeeCode { get; set; }
        public string ImagePath { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string LoggedInAs { get; set; }
        public List<UserRolePermission> RolePermissions { get; set; }
        public bool IsActive { get; set; }
        public long? ReportingTo { get; set; }
        public string ReportingName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public int? LoginCount { get; set; }
        public int? LoginFailCount { get; set; }
        public bool SsoLogin { get; set; }
        public int ExpireTime { get; set; }
        public string Version { get; set; }
        public string ProductID { get; set; }
        public string License { get; set; }
        public string BelongsTo { get; set; }
        public bool DirectReports { get; set; }
        public bool IsTeamLeader { get; set; } = false;
        public string DomainName { get; set; }
        public int SkipCount { get; set; }
        public int ReadyCount { get; set; }
        public bool IsAnyOkr { get; set; }
    }
}
