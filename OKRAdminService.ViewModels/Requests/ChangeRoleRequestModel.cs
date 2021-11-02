using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Requests
{
    public class ChangeRoleRequestModel
    {
        public long NewRoleId { get; set; } 
        public List<long> EmployeeIds { get; set; }
    }
}
