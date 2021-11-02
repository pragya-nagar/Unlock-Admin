namespace OKRAdminService.ViewModels.Response
{
    public class ActiveOrganisations
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationHead { get; set; }
        public int EmployeeCount { get; set; }
        public string Designation { get; set; }
        public string HeadImage { get; set; }
        public string HeadCode { get; set; }
        public string OrgLogo { get; set; }
        public long ParentId { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
    }
}
