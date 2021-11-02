using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.ViewModels.Response
{
    public class DirectReportsDetails
    {
        public long EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long OrganisationId { get; set; }
        public string OrganizationName { get; set; }
        public string ImagePath { get; set; }
        public string Designation { get; set; }
        public string ColorCode { get; set; }
        public string BackGroundColorCode { get; set; }
    }
}
