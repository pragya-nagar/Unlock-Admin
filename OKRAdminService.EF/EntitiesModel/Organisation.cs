using System.Collections.Generic;

namespace OKRAdminService.EF
{
    public partial class Organisation {

        public Organisation()
        {
            this.Employees = new List<Employee>();
            this.OrganisationCycles = new List<OrganisationCycle>();
            this.OrganizationObjectives = new List<OrganizationObjective>();
            OnCreated();
        }

        public virtual long OrganisationId
        {
            get;
            set;
        }

        public virtual string OrganisationName
        {
            get;
            set;
        }

        public virtual long? OrganisationHead
        {
            get;
            set;
        }

        public virtual string ImagePath
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

        public virtual bool? IsDeleted
        {
            get;
            set;
        }

        public virtual long? ParentId
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual string LogoName
        {
            get;
            set;
        }
        public virtual string ColorCode
        {
            get;
            set;
        }

        public virtual string BackGroundColorCode
        {
            get;
            set;
        }

        public virtual IList<Employee> Employees
        {
            get;
            set;
        }

        public virtual IList<OrganisationCycle> OrganisationCycles
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
