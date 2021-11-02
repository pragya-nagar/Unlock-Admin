namespace OKRAdminService.EF
{
    public partial class OrganisationCycle {

        public OrganisationCycle()
        {
            OnCreated();
        }

        public virtual long OrganisationCycleId
        {
            get;
            set;
        }

        public virtual long CycleDurationId
        {
            get;
            set;
        }

        public virtual int SymbolId
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

        public virtual System.DateTime CycleStartDate
        {
            get;
            set;
        }

        public virtual System.DateTime? CycleEndDate
        {
            get;
            set;
        }

        public virtual int? CycleYear
        {
            get;
            set;
        }

        public virtual bool? IsDiscarded
        {
            get;
            set;
        }
        public virtual bool? IsProcessed
        {
            get;
            set;
        }


        public virtual CycleDurationMaster CycleDurationMaster
        {
            get;
            set;
        }

        public virtual Organisation Organisation
        {
            get;
            set;
        }

        public virtual CycleDurationSymbol CycleDurationSymbol
        {
            get;
            set;
        }

        #region Extensibility Method Definitions

        partial void OnCreated();

        #endregion
    }

}
