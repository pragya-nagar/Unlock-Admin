using System;

namespace OKRAdminService.ViewModels.Requests
{
    public class ParentRequest
    {
        public long OrganisationId { get; set; }
        public string ParentName { get; set; }
        public long LeaderId{ get; set; }
        public string ImagePath { get; set; }
        public string LogoName { get; set; }
        public long CycleDurationId { get; set; }
        public DateTime CycleStartDate { get; set; }
        public bool IsPrivate { get; set; }
    }
}
