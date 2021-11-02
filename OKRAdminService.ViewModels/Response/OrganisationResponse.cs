using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class OrganisationResponse
    {
        ///public long OrganisationId { get; set; }
        public IList<ObjectiveDetails> ObjectiveDetails { get; set; }
    }
}
