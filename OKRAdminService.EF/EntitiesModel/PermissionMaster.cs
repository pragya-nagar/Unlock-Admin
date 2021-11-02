using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class PermissionMaster {

        public PermissionMaster()
        {
            this.PermissionRoleMappings = new List<PermissionRoleMapping>();
            OnCreated();
        }

        public virtual long PermissionId
        {
            get;
            set;
        }

        public virtual long ModuleId
        {
            get;
            set;
        }

        public virtual string Permission
        {
            get;
            set;
        }

        public virtual string PermissionDescription
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

        public virtual ModuleMaster ModuleMaster
        {
            get;
            set;
        }

        public virtual IList<PermissionRoleMapping> PermissionRoleMappings
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
