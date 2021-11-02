using System;
using System.Collections.Generic;
using System.Text;

namespace OKRAdminService.EF
{
    public partial class GoalTypeMaster
    {
        public int GoalTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public virtual bool IsDefault
        {
            get;
            set;
        }

    }
}
