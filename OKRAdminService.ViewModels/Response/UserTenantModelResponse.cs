using System;
namespace OKRAdminService.ViewModels.Response
{
    public class UserTenantModelResponse
    {
        public Guid TenantId { get; set; }
        public string DomainName { get; set; }
        public string UserEmail { get; set; }
    }
}
