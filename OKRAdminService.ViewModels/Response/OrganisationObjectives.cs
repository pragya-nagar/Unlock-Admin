using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class OrganisationObjectives
    {
        public long OrganisationId { get; set; }
        public IList<ObjectiveDetails> ObjectiveDetails { get; set; }
    }
}
