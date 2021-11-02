using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Requests
{
    public class EditUserReportingRequest
    {
        public long NewReportingToId { get; set; }
        public List<long> EmployeeIds { get; set; }
    }
}
