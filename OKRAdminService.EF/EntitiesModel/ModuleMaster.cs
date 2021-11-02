using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class ModuleMaster {

        public ModuleMaster()
        {
            this.PermissionMasters = new List<PermissionMaster>();
            OnCreated();
        }

        public virtual long ModuleId
        {
            get;
            set;
        }

        public virtual string ModuleName
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

        public virtual IList<PermissionMaster> PermissionMasters
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
