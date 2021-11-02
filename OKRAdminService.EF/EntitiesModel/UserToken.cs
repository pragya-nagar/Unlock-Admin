namespace OKRAdminService.EF
{
    public partial class UserToken {

        public UserToken()
        {
            OnCreated();
        }

        public virtual long Id
        {
            get;
            set;
        }

        public virtual long EmployeeId
        {
            get;
            set;
        }

        public virtual string Token
        {
            get;
            set;
        }

        public virtual System.DateTime ExpireTime
        {
            get;
            set;
        }

        public virtual int? TokenType
        {
            get;
            set;
        }
        public virtual System.DateTime? LastLoginDate
        {
            get;
            set;
        }

        public virtual System.DateTime? CurrentLoginDate
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
