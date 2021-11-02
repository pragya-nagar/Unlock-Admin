using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class UserRolePermission
    {
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public List<PermissionDetailModel> Permissions { get; set; }
    }
}
