namespace OKRAdminService.EF
{
    public partial class UserDetail {

        public UserDetail()
        {
            OnCreated();
        }

        public virtual long UserId
        {
            get;
            set;
        }

        public virtual string FirstName
        {
            get;
            set;
        }

        public virtual string LastName
        {
            get;
            set;
        }

        public virtual long RoleId
        {
            get;
            set;
        }

        public virtual long EmployeeId
        {
            get;
            set;
        }

        public virtual string EmailId
        {
            get;
            set;
        }

        public virtual string Password
        {
            get;
            set;
        }

        public virtual string Salt
        {
            get;
            set;
        }

        public virtual System.DateTime CreatedOn
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

        public virtual int? Status
        {
            get;
            set;
        }

        public virtual long? ReportingTo
        {
            get;
            set;
        }

        public virtual string ImagePath
        {
            get;
            set;
        }

        public virtual System.DateTime? LastLoginTime
        {
            get;
            set;
        }

        public virtual int? LoginCount
        {
            get;
            set;
        }

        public virtual string ImageDetails
        {
            get;
            set;
        }

        public virtual long? OrganisationId
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
