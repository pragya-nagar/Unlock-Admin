using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class CycleDurationMaster {

        public CycleDurationMaster()
        {
            this.OrganisationCycles = new List<OrganisationCycle>();
            this.CycleDurationSymbols = new List<CycleDurationSymbol>();
            OnCreated();
        }

        public virtual long CycleDurationId
        {
            get;
            set;
        }

        public virtual string CycleDuration
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

        public virtual System.DateTime? CreatedOn
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

        public virtual IList<OrganisationCycle> OrganisationCycles
        {
            get;
            set;
        }

        public virtual IList<CycleDurationSymbol> CycleDurationSymbols
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
