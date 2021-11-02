using System.Collections.Generic;


namespace OKRAdminService.ViewModels.Response
{
    public class OrganisationDetails
    {
        public long OrganisationId { get; set; }
        public IList<ObjectiveDetails> ObjectiveDetails { get; set; }
    }
}
