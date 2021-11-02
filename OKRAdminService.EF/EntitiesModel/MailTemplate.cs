namespace OKRAdminService.EF
{
    public partial class MailTemplate {

        public MailTemplate()
        {
            OnCreated();
        }

        public virtual int Id
        {
            get;
            set;
        }

        public virtual string TempleteSubject
        {
            get;
            set;
        }

        public virtual string TempleteBody
        {
            get;
            set;
        }

        public virtual int Status
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
