namespace OKRAdminService.EF
{
    public partial class GoalUnlockDate {

        public GoalUnlockDate()
        {
            OnCreated();
        }

        public virtual long Id
        {
            get;
            set;
        }

        public virtual long OrganisationCycleId
        {
            get;
            set;
        }

        public virtual int Type
        {
            get;
            set;
        }

        public virtual bool IsActive
        {
            get;
            set;
        }

        public virtual System.DateTime SubmitDate
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
