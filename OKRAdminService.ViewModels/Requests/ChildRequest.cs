using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Requests
{
    public class ChildRequest
    {
        public long ChildOrganisationId { get; set; }
        public long ParentOrganisationId { get; set; }
        public string ChildOrganisationName { get; set; }
        public string Description { get; set; }
        public long LeaderId { get; set; }
        public string LogoImage { get; set; }
        public string LogoName { get; set; }
        public List<long> EmployeeList { get; set; }
    }
}
