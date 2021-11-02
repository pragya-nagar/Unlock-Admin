namespace OKRAdminService.EF
{
    public partial class ErrorLog {

        public ErrorLog()
        {
            OnCreated();
        }

        public virtual long LogId
        {
            get;
            set;
        }

        public virtual System.DateTime CreatedOn
        {
            get;
            set;
        }

        public virtual string PageName
        {
            get;
            set;
        }

        public virtual string FunctionName
        {
            get;
            set;
        }

        public virtual string ErrorDetail
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
