using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class ObjectivesMaster {

        public ObjectivesMaster()
        {
            this.OrganizationObjectives = new List<OrganizationObjective>();
            OnCreated();
        }

        public virtual int ObjectiveId
        {
            get;
            set;
        }

        public virtual string ObjectiveName
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

        public virtual IList<OrganizationObjective> OrganizationObjectives
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
