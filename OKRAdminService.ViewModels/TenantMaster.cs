using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.ViewModels
{
    public class TenantMaster
    {
        public Guid TenantId { get; set; }
        public string SubDomain { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool? IsLicensed { get; set; }
        public DateTime? DemoExpiryDate { get; set; }
        public DateTime? LicenseCreatedOn { get; set; }
        public int PurchaseLicense { get; set; }
        public int BufferLicense { get; set; }
    }
}
