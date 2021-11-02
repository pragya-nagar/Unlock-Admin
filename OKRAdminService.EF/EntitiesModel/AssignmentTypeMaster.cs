namespace OKRAdminService.EF
{
    public partial class AssignmentTypeMaster {

        public AssignmentTypeMaster()
        {
            OnCreated();
        }

        public virtual int AssignmentTypeId
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
