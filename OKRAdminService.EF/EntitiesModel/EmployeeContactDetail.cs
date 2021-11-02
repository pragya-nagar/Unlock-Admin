
namespace OKRAdminService.EF
{
    public partial class EmployeeContactDetail {

        public EmployeeContactDetail()
        {
            OnCreated();
        }

        public virtual long ContactId
        {
            get;
            set;
        }

        public virtual long EmployeeId
        {
            get;
            set;
        }

        public virtual string PhoneNumber
        {
            get;
            set;
        }

        public virtual string DeskPhoneNumber
        {
            get;
            set;
        }
        public virtual string CountryStdCode
        {
            get;
            set;
        }

        public virtual string SkypeUrl
        {
            get;
            set;
        }

        public virtual string TwitterUrl
        {
            get;
            set;
        }

        public virtual string LinkedInUrl
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

        public virtual Employee Employee
        {
            get;
            set;
        }


        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
