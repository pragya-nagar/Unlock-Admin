namespace OKRAdminService.EF
{
    public partial class OkrStatusMaster {

        public OkrStatusMaster()
        {
            OnCreated();
        }

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

        public virtual System.DateTime? CreatedOn
        {
            get;
            set;
        }

        public virtual long CreatedBy
        {
            get;
            set;
        }

        public virtual System.DateTime? UpdatedOn
        {
            get;
            set;
        }

        public virtual long? UpdatedBy
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
