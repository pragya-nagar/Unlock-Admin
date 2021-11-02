using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Requests
{
    public class ChangeUserOrganisationRequest
    {
        public long NewOrganisationId { get; set; }
        public List<long> EmployeeIds { get; set; }
    }
}
