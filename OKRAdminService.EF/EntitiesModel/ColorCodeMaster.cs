using System;

namespace OKRAdminService.EF
{
    public partial class ColorCodeMaster
    {
        public virtual int Id
        {
            get;
            set;
        }

        public virtual string ColorCode
        {
            get;
            set;
        }

        public virtual DateTime CreatedOn
        {
            get;
            set;
        }

        public virtual long CreatedBy
        {
            get;
            set;
        }

        public virtual bool IsActive
        {
            get;
            set;
        }
        public virtual string BackGroundColorCode
        {
            get;
            set;
        }

    }
}
