namespace OKRAdminService.EF
{
    public partial class MetricDataMaster {

        public MetricDataMaster()
        {
            OnCreated();
        }

        public virtual int DataId
        {
            get;
            set;
        }

        public virtual int MetricId
        {
            get;
            set;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual bool IsActive
        {
            get;
            set;
        }

        public virtual string Symbol
        {
            get;
            set;
        }

        public virtual bool IsDefault
        {
            get;
            set;
        }



        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
