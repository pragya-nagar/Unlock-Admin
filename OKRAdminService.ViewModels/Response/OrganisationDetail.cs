using System;
using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class OrganisationDetail
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long OrganisationLeader { get; set; }
        public string OrganisationLogo { get; set; }
        public string LogoName { get; set; }
        public string LeaderName { get; set; }
        public string LeaderProfileImage { get; set; }
        public long ParentOrganisationId { get; set; }
        public string ParentName { get; set; }
        public string Description { get; set; }
        public DateTime CycleStartDate { get; set; }
        public long CycleDurationId { get; set; }
        public string CycleDuration { get; set; }
        public bool IsPrivate { get; set; }
        public long TotalEmployees { get; set; }
        public long HeadParentId { get; set; }
        public List<EmployeeInformation> EmployeeList { get; set; }
    }
}
