namespace OKRAdminService.EF
{
    public partial class PermissionRoleMapping {

        public PermissionRoleMapping()
        {
            OnCreated();
        }

        public virtual long PermissionMappingId
        {
            get;
            set;
        }

        public virtual long RoleId
        {
            get;
            set;
        }

        public virtual long PermissionId
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

        public virtual RoleMaster RoleMaster
        {
            get;
            set;
        }

        public virtual PermissionMaster PermissionMaster
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
