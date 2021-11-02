using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.EF
{
    public partial class KrStatusMaster
    {
        public int KrStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
    }
}
