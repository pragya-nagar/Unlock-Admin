using System.Collections.Generic;

namespace OKRAdminService.ViewModels.Response
{
    public class SwitchRoleLoginReponse
    {
        public string TokenId { get; set; }
        public long RoleId { get; set; }
        public string RoleName { get; set; }
        public string LoggedInAs { get; set; }
        public string Message { get; set; }
        public List<PermissionDetailModel> Permissions { get; set; }
    }
}
