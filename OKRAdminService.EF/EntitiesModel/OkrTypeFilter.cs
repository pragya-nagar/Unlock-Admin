

namespace OKRAdminService.EF
{
    public partial class OkrTypeFilter
    {
        public virtual int Id
        {
            get;
            set;
        }

        public virtual string StatusName
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual string Code
        {
            get;
            set;
        }

        public virtual string Color
        {
            get;
            set;
        }

        public virtual bool IsActive
        {
            get;
            set;
        }

    }
}
