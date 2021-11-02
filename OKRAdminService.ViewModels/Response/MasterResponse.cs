using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class MasterResponse
    {
        public List<OrganizationObjectivesDetails> OrganizationObjectivesDetails { get; set; }
        public IEnumerable<RoleMasterDetails> RoleMasterDetails { get; set; }
        public IEnumerable<CycleDurationMasterDetails> CycleDurationMasterDetails { get; set; }
        public IEnumerable<PermissionMasterDetails> PermissionMasters { get; set; }
        public IEnumerable<CycleDurationDetails> CycleDurationDetails { get; set; }
    }
}
