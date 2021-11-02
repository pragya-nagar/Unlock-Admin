using System;

namespace OKRAdminService.ViewModels.Response
{
    public class TenantResponse
    {
        public string DomainName { get; set; }
        public string UserEmail { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; }
    }
}
