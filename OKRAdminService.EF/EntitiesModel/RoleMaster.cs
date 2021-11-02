using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class RoleMaster {

        public RoleMaster()
        {
            this.PermissionRoleMappings = new List<PermissionRoleMapping>();
            this.Employees = new List<Employee>();
            OnCreated();
        }

        public virtual long RoleId
        {
            get;
            set;
        }

        public virtual string RoleName
        {
            get;
            set;
        }

        public virtual string RoleDescription
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

        public virtual IList<PermissionRoleMapping> PermissionRoleMappings
        {
            get;
            set;
        }

        public virtual IList<Employee> Employees
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
