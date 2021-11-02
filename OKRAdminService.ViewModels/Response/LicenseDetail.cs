using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.ViewModels.Response
{
    public class LicenseDetail
    {
        public int PurchaseLicense { get; set; } = 0;
        public int BufferLicense { get; set; } = 0;
        public int ActiveUser { get; set; } 
        public int AvailableLicense { get; set; } = 0;
        public bool IsAddUserAllow { get; set; } = false;
        public string Note { get; set; }

    }
}
