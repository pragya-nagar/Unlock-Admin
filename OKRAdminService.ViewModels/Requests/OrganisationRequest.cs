using System;

namespace OKRAdminService.ViewModels.Requests
{
    public class OrganisationRequest
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public long OrganisationLeader { get; set; }
        public string ImagePath { get; set; }
        public DateTime CycleStartDate { get; set; }
        public int CycleDuration { get; set; }
        public string LogoName { get; set; }
        public bool IsPrivate { get; set; }
    }
}
