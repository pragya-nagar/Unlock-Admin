namespace OKRAdminService.EF
{
    public partial class JobsAudit {

        public JobsAudit()
        {
            OnCreated();
        }

        public virtual long AuditId
        {
            get;
            set;
        }

        public virtual string JobName
        {
            get;
            set;
        }

        public virtual string Status
        {
            get;
            set;
        }

        public virtual string Details
        {
            get;
            set;
        }

        public virtual System.DateTime ExecutionDate
        {
            get;
            set;
        }

        public virtual System.DateTime AuditDate
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
