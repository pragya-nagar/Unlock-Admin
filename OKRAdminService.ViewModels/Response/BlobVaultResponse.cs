using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.ViewModels.Response
{
    public class BlobVaultResponse
    {
       
            public string BlobAccountName { get; set; }
            public string BlobAccountKey { get; set; }
            public string BlobCdnUrl { get; set; }
            public string BlobContainerName { get; set; }
            public string BlobCdnCommonUrl { get; set; }
        


    }
}
