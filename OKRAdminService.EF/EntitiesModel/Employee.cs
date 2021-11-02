using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class Employee
    {

        public Employee()
        {
            this.EmployeeContactDetails = new List<EmployeeContactDetail>();
            OnCreated();
        }

        public virtual long EmployeeId
        {
            get;
            set;
        }

        public virtual string EmployeeCode
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

        public virtual string Password
        {
            get;
            set;
        }

        public virtual string PasswordSalt
        {
            get;
            set;
        }

        public virtual string Designation
        {
            get;
            set;
        }

        public virtual string EmailId
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

        public virtual long OrganisationId
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

        public virtual long RoleId
        {
            get;
            set;
        }

        public virtual int? LoginFailCount
        {
            get;
            set;
        }

        public virtual string ProfileImageFile
        {
            get;
            set;
        }


        public virtual Organisation Organisation
        {
            get;
            set;
        }

        public virtual RoleMaster RoleMaster
        {
            get;
            set;
        }

        public virtual IList<EmployeeContactDetail> EmployeeContactDetails
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
