using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class CycleDurationSymbol {

        public CycleDurationSymbol()
        {
            this.OrganisationCycles = new List<OrganisationCycle>();
            OnCreated();
        }

        public virtual int Id
        {
            get;
            set;
        }

        public virtual long CycleDurationId
        {
            get;
            set;
        }

        public virtual string Symbol
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

        public virtual CycleDurationMaster CycleDurationMaster
        {
            get;
            set;
        }

        public virtual IList<OrganisationCycle> OrganisationCycles
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
