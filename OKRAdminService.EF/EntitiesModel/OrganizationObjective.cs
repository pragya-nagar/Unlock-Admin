namespace OKRAdminService.EF
{
    public partial class OrganizationObjective {

        public OrganizationObjective()
        {
            OnCreated();
        }

        public virtual long Id
        {
            get;
            set;
        }

        public virtual long OrganisationId
        {
            get;
            set;
        }

        public virtual int ObjectiveId
        {
            get;
            set;
        }

        public virtual bool IsActive
        {
            get;
            set;
        }

        public virtual long CreatedBy
        {
            get;
            set;
        }

        public virtual System.DateTime CreatedOn
        {
            get;
            set;
        }

        public virtual long? UpdatedBy
        {
            get;
            set;
        }

        public virtual System.DateTime? UpdatedOn
        {
            get;
            set;
        }

        public virtual bool? IsDiscarded
        {
            get;
            set;
        }

        public virtual ObjectivesMaster ObjectivesMaster
        {
            get;
            set;
        }

        public virtual Organisation Organisation
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
