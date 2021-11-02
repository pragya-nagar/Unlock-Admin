using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;


namespace OKRAdminService.EF
{

    public partial class OkrAdminDbContext : DataContext
    {

        public OkrAdminDbContext() :
            base()
        {
            OnCreated();
        }

        public OkrAdminDbContext(DbContextOptions<OkrAdminDbContext> options) :
            base(options)
        {
            OnCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured ||
                (!optionsBuilder.Options.Extensions.OfType<RelationalOptionsExtension>().Any(ext => !string.IsNullOrEmpty(ext.ConnectionString) || ext.Connection != null) &&
                 !optionsBuilder.Options.Extensions.Any(ext => !(ext is RelationalOptionsExtension) && !(ext is CoreOptionsExtension))))
            {
                optionsBuilder.UseSqlServer(@"Data Source=52.21.77.184;Initial Catalog=User_Management_Dev;Integrated Security=False;Persist Security Info=True;User ID=okr-admin;Password=abcd@1234");
            }
            CustomizeConfiguration(ref optionsBuilder);
            base.OnConfiguring(optionsBuilder);
        }

        partial void CustomizeConfiguration(ref DbContextOptionsBuilder optionsBuilder);

        public virtual DbSet<CycleDurationMaster> CycleDurationMasters
        {
            get;
            set;
        }

        public virtual DbSet<Employee> Employees
        {
            get;
            set;
        }

        public virtual DbSet<ErrorLog> ErrorLogs
        {
            get;
            set;
        }

        public virtual DbSet<GoalUnlockDate> GoalUnlockDates
        {
            get;
            set;
        }

        public virtual DbSet<MailTemplate> MailTemplates
        {
            get;
            set;
        }

        public virtual DbSet<ModuleMaster> ModuleMasters
        {
            get;
            set;
        }

        public virtual DbSet<ObjectivesMaster> ObjectivesMasters
        {
            get;
            set;
        }

        public virtual DbSet<OkrStatusMaster> OkrStatusMasters
        {
            get;
            set;
        }

        public virtual DbSet<OrganisationCycle> OrganisationCycles
        {
            get;
            set;
        }

        public virtual DbSet<Organisation> Organisations
        {
            get;
            set;
        }

        public virtual DbSet<OrganizationObjective> OrganizationObjectives
        {
            get;
            set;
        }

        public virtual DbSet<PermissionMaster> PermissionMasters
        {
            get;
            set;
        }

        public virtual DbSet<PermissionRoleMapping> PermissionRoleMappings
        {
            get;
            set;
        }

        public virtual DbSet<RoleMaster> RoleMasters
        {
            get;
            set;
        }

        public virtual DbSet<UserDetail> UserDetails
        {
            get;
            set;
        }

        public virtual DbSet<UserToken> UserTokens
        {
            get;
            set;
        }

        public virtual DbSet<CycleDurationSymbol> CycleDurationSymbols
        {
            get;
            set;
        }

        public virtual DbSet<EmployeeContactDetail> EmployeeContactDetails
        {
            get;
            set;
        }

        public virtual DbSet<AssignmentTypeMaster> AssignmentTypeMasters
        {
            get;
            set;
        }

        public virtual DbSet<JobsAudit> JobsAudits
        {
            get;
            set;
        }

        public virtual DbSet<MetricDataMaster> MetricDataMasters
        {
            get;
            set;
        }

        public virtual DbSet<MetricMaster> MetricMasters
        {
            get;
            set;
        }

        public virtual DbSet<OkrTypeFilter> OkrTypeFilters
        {
            get;
            set;
        }

        public virtual DbSet<DirectReporteesFilter> DirectReporteesFilters
        {
            get;
            set;
        }

        public virtual DbSet<ColorCodeMaster> ColorCodeMaster
        {
            get;
            set;
        }

        #region Methods

        public void SpDeleteImage(long? employeeId)
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.sp_DeleteImage";

                    DbParameter employeeIdParameter = cmd.CreateParameter();
                    employeeIdParameter.ParameterName = "employeeId";
                    employeeIdParameter.Direction = ParameterDirection.Input;
                    employeeIdParameter.DbType = DbType.Int64;
                    employeeIdParameter.Precision = 19;
                    employeeIdParameter.Scale = 0;
                    if (employeeId.HasValue)
                    {
                        employeeIdParameter.Value = employeeId.Value;
                    }
                    else
                    {
                        employeeIdParameter.Size = -1;
                        employeeIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(employeeIdParameter);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public async Task SpDeleteImageAsync(long? employeeId)
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.sp_DeleteImage";

                    DbParameter employeeIdParameter = cmd.CreateParameter();
                    employeeIdParameter.ParameterName = "employeeId";
                    employeeIdParameter.Direction = ParameterDirection.Input;
                    employeeIdParameter.DbType = DbType.Int64;
                    employeeIdParameter.Precision = 19;
                    employeeIdParameter.Scale = 0;
                    if (employeeId.HasValue)
                    {
                        employeeIdParameter.Value = employeeId.Value;
                    }
                    else
                    {
                        employeeIdParameter.Size = -1;
                        employeeIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(employeeIdParameter);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public void SpUpdateImagePath(string Url, long? EmployeeId, string imageDetail)
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.sp_UpdateImagePath";

                    DbParameter UrlParameter = cmd.CreateParameter();
                    UrlParameter.ParameterName = "Url";
                    UrlParameter.Direction = ParameterDirection.Input;
                    UrlParameter.DbType = DbType.String;
                    UrlParameter.Size = 2147483647;
                    if (Url != null)
                    {
                        UrlParameter.Value = Url;
                    }
                    else
                    {
                        UrlParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(UrlParameter);

                    DbParameter EmployeeIdParameter = cmd.CreateParameter();
                    EmployeeIdParameter.ParameterName = "EmployeeId";
                    EmployeeIdParameter.Direction = ParameterDirection.Input;
                    EmployeeIdParameter.DbType = DbType.Int64;
                    EmployeeIdParameter.Precision = 19;
                    EmployeeIdParameter.Scale = 0;
                    if (EmployeeId.HasValue)
                    {
                        EmployeeIdParameter.Value = EmployeeId.Value;
                    }
                    else
                    {
                        EmployeeIdParameter.Size = -1;
                        EmployeeIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(EmployeeIdParameter);

                    DbParameter imageDetailParameter = cmd.CreateParameter();
                    imageDetailParameter.ParameterName = "imageDetail";
                    imageDetailParameter.Direction = ParameterDirection.Input;
                    imageDetailParameter.DbType = DbType.String;
                    imageDetailParameter.Size = 2147483647;
                    if (imageDetail != null)
                    {
                        imageDetailParameter.Value = imageDetail;
                    }
                    else
                    {
                        imageDetailParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(imageDetailParameter);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public async Task SpUpdateImagePathAsync(string Url, long? EmployeeId, string imageDetail)
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.sp_UpdateImagePath";

                    DbParameter UrlParameter = cmd.CreateParameter();
                    UrlParameter.ParameterName = "Url";
                    UrlParameter.Direction = ParameterDirection.Input;
                    UrlParameter.DbType = DbType.String;
                    UrlParameter.Size = 2147483647;
                    if (Url != null)
                    {
                        UrlParameter.Value = Url;
                    }
                    else
                    {
                        UrlParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(UrlParameter);

                    DbParameter EmployeeIdParameter = cmd.CreateParameter();
                    EmployeeIdParameter.ParameterName = "EmployeeId";
                    EmployeeIdParameter.Direction = ParameterDirection.Input;
                    EmployeeIdParameter.DbType = DbType.Int64;
                    EmployeeIdParameter.Precision = 19;
                    EmployeeIdParameter.Scale = 0;
                    if (EmployeeId.HasValue)
                    {
                        EmployeeIdParameter.Value = EmployeeId.Value;
                    }
                    else
                    {
                        EmployeeIdParameter.Size = -1;
                        EmployeeIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(EmployeeIdParameter);

                    DbParameter imageDetailParameter = cmd.CreateParameter();
                    imageDetailParameter.ParameterName = "imageDetail";
                    imageDetailParameter.Direction = ParameterDirection.Input;
                    imageDetailParameter.DbType = DbType.String;
                    imageDetailParameter.Size = 2147483647;
                    if (imageDetail != null)
                    {
                        imageDetailParameter.Value = imageDetail;
                    }
                    else
                    {
                        imageDetailParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(imageDetailParameter);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public void SpImportOkrResult(long? OrgCycleId, long? OrgId, int? CycleYear)
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.sp_ImportOkrResult";

                    DbParameter OrgCycleIdParameter = cmd.CreateParameter();
                    OrgCycleIdParameter.ParameterName = "OrgCycleId";
                    OrgCycleIdParameter.Direction = ParameterDirection.Input;
                    OrgCycleIdParameter.DbType = DbType.Int64;
                    OrgCycleIdParameter.Precision = 19;
                    OrgCycleIdParameter.Scale = 0;
                    if (OrgCycleId.HasValue)
                    {
                        OrgCycleIdParameter.Value = OrgCycleId.Value;
                    }
                    else
                    {
                        OrgCycleIdParameter.Size = -1;
                        OrgCycleIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(OrgCycleIdParameter);

                    DbParameter OrgIdParameter = cmd.CreateParameter();
                    OrgIdParameter.ParameterName = "OrgId";
                    OrgIdParameter.Direction = ParameterDirection.Input;
                    OrgIdParameter.DbType = DbType.Int64;
                    OrgIdParameter.Precision = 19;
                    OrgIdParameter.Scale = 0;
                    if (OrgId.HasValue)
                    {
                        OrgIdParameter.Value = OrgId.Value;
                    }
                    else
                    {
                        OrgIdParameter.Size = -1;
                        OrgIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(OrgIdParameter);

                    DbParameter CycleYearParameter = cmd.CreateParameter();
                    CycleYearParameter.ParameterName = "CycleYear";
                    CycleYearParameter.Direction = ParameterDirection.Input;
                    CycleYearParameter.DbType = DbType.Int32;
                    CycleYearParameter.Precision = 10;
                    CycleYearParameter.Scale = 0;
                    if (CycleYear.HasValue)
                    {
                        CycleYearParameter.Value = CycleYear.Value;
                    }
                    else
                    {
                        CycleYearParameter.Size = -1;
                        CycleYearParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(CycleYearParameter);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public async Task SpImportOkrResultAsync(long? OrgCycleId, long? OrgId, int? CycleYear)
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.sp_ImportOkrResult";

                    DbParameter OrgCycleIdParameter = cmd.CreateParameter();
                    OrgCycleIdParameter.ParameterName = "OrgCycleId";
                    OrgCycleIdParameter.Direction = ParameterDirection.Input;
                    OrgCycleIdParameter.DbType = DbType.Int64;
                    OrgCycleIdParameter.Precision = 19;
                    OrgCycleIdParameter.Scale = 0;
                    if (OrgCycleId.HasValue)
                    {
                        OrgCycleIdParameter.Value = OrgCycleId.Value;
                    }
                    else
                    {
                        OrgCycleIdParameter.Size = -1;
                        OrgCycleIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(OrgCycleIdParameter);

                    DbParameter OrgIdParameter = cmd.CreateParameter();
                    OrgIdParameter.ParameterName = "OrgId";
                    OrgIdParameter.Direction = ParameterDirection.Input;
                    OrgIdParameter.DbType = DbType.Int64;
                    OrgIdParameter.Precision = 19;
                    OrgIdParameter.Scale = 0;
                    if (OrgId.HasValue)
                    {
                        OrgIdParameter.Value = OrgId.Value;
                    }
                    else
                    {
                        OrgIdParameter.Size = -1;
                        OrgIdParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(OrgIdParameter);

                    DbParameter CycleYearParameter = cmd.CreateParameter();
                    CycleYearParameter.ParameterName = "CycleYear";
                    CycleYearParameter.Direction = ParameterDirection.Input;
                    CycleYearParameter.DbType = DbType.Int32;
                    CycleYearParameter.Precision = 10;
                    CycleYearParameter.Scale = 0;
                    if (CycleYear.HasValue)
                    {
                        CycleYearParameter.Value = CycleYear.Value;
                    }
                    else
                    {
                        CycleYearParameter.Size = -1;
                        CycleYearParameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(CycleYearParameter);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public void SpGetAllOrganisation()
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.Sp_GetAllOrganisations";
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        public async Task SpGetAllOrganisationAsync()
        {

            DbConnection connection = this.Database.GetDbConnection();
            bool needClose = false;
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
                needClose = true;
            }

            try
            {
                using (DbCommand cmd = connection.CreateCommand())
                {
                    if (this.Database.GetCommandTimeout().HasValue)
                        cmd.CommandTimeout = this.Database.GetCommandTimeout().Value;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = @"dbo.Sp_GetAllOrganisations";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (needClose)
                    connection.Close();
            }
        }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.CycleDurationMasterMapping(modelBuilder);
            this.CustomizeCycleDurationMasterMapping(modelBuilder);

            this.EmployeeMapping(modelBuilder);
            this.CustomizeEmployeeMapping(modelBuilder);

            this.ErrorLogMapping(modelBuilder);
            this.CustomizeErrorLogMapping(modelBuilder);


            this.GoalUnlockDateMapping(modelBuilder);
            this.CustomizeGoalUnlockDateMapping(modelBuilder);

            this.MailTemplateMapping(modelBuilder);
            this.CustomizeMailTemplateMapping(modelBuilder);

            this.ModuleMasterMapping(modelBuilder);
            this.CustomizeModuleMasterMapping(modelBuilder);

            this.ObjectivesMasterMapping(modelBuilder);
            this.CustomizeObjectivesMasterMapping(modelBuilder);

            this.OkrStatusMasterMapping(modelBuilder);
            this.CustomizeOkrStatusMasterMapping(modelBuilder);

            this.OrganisationCycleMapping(modelBuilder);
            this.CustomizeOrganisationCycleMapping(modelBuilder);

            this.OrganisationMapping(modelBuilder);
            this.CustomizeOrganisationMapping(modelBuilder);

            this.OrganizationObjectiveMapping(modelBuilder);
            this.CustomizeOrganizationObjectiveMapping(modelBuilder);

            this.PermissionMasterMapping(modelBuilder);
            this.CustomizePermissionMasterMapping(modelBuilder);

            this.PermissionRoleMappingMapping(modelBuilder);
            this.CustomizePermissionRoleMappingMapping(modelBuilder);

            this.RoleMasterMapping(modelBuilder);
            this.CustomizeRoleMasterMapping(modelBuilder);

            this.UserDetailMapping(modelBuilder);
            this.CustomizeUserDetailMapping(modelBuilder);

            this.UserTokenMapping(modelBuilder);
            this.CustomizeUserTokenMapping(modelBuilder);

            this.CycleDurationSymbolMapping(modelBuilder);
            this.CustomizeCycleDurationSymbolMapping(modelBuilder);

            this.EmployeeContactDetailMapping(modelBuilder);
            this.CustomizeEmployeeContactDetailMapping(modelBuilder);

            this.AssignmentTypeMasterMapping(modelBuilder);
            this.CustomizeAssignmentTypeMasterMapping(modelBuilder);

            this.JobsAuditMapping(modelBuilder);
            this.CustomizeJobsAuditMapping(modelBuilder);

            this.MetricDataMasterMapping(modelBuilder);
            this.CustomizeMetricDataMasterMapping(modelBuilder);

            this.MetricMasterMapping(modelBuilder);
            this.CustomizeMetricMasterMapping(modelBuilder);

            this.KrStatusMasterMapping(modelBuilder);
            this.CustomizeKrStatusMasterMapping(modelBuilder);

            this.GoalStatusMasterMapping(modelBuilder);
            this.CustomizeGoalStatusMasterMapping(modelBuilder);

            this.GoalTypeMasterMapping(modelBuilder);
            this.CustomizeGoalTypeMasterMapping(modelBuilder);

            this.OkrTypeFilterMapping(modelBuilder);
            this.CustomizeOkrTypeFilterMapping(modelBuilder);


            this.DirectReporteesFilterMapping(modelBuilder);
            this.CustomizeDirectReporteesFilterMapping(modelBuilder);


            RelationshipsMapping(modelBuilder);
            CustomizeMapping(ref modelBuilder);
        }

        #region CycleDurationMaster Mapping

        private void CycleDurationMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CycleDurationMaster>().ToTable(@"CycleDurationMaster", @"dbo");
            modelBuilder.Entity<CycleDurationMaster>().Property<long>(x => x.CycleDurationId).HasColumnName(@"CycleDurationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<CycleDurationMaster>().Property<string>(x => x.CycleDuration).HasColumnName(@"CycleDuration").HasColumnType(@"varchar(20)").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<CycleDurationMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<CycleDurationMaster>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<CycleDurationMaster>().Property<System.DateTime?>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<CycleDurationMaster>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<CycleDurationMaster>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<CycleDurationMaster>().HasKey(@"CycleDurationId");
        }

        partial void CustomizeCycleDurationMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region Employee Mapping

        private void EmployeeMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable(@"Employees", @"dbo");
            modelBuilder.Entity<Employee>().Property<long>(x => x.EmployeeId).HasColumnName(@"EmployeeId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<Employee>().Property<string>(x => x.EmployeeCode).HasColumnName(@"EmployeeCode").HasColumnType(@"varchar(30)").ValueGeneratedNever().HasMaxLength(30);
            modelBuilder.Entity<Employee>().Property<string>(x => x.FirstName).HasColumnName(@"FirstName").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<Employee>().Property<string>(x => x.LastName).HasColumnName(@"LastName").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<Employee>().Property<string>(x => x.Password).HasColumnName(@"Password").HasColumnType(@"text").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<Employee>().Property<string>(x => x.PasswordSalt).HasColumnName(@"PasswordSalt").HasColumnType(@"text").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<Employee>().Property<string>(x => x.Designation).HasColumnName(@"Designation").HasColumnType(@"nvarchar(200)").ValueGeneratedNever().HasMaxLength(200);
            modelBuilder.Entity<Employee>().Property<string>(x => x.EmailId).HasColumnName(@"EmailId").HasColumnType(@"varchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<Employee>().Property<long?>(x => x.ReportingTo).HasColumnName(@"ReportingTo").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(30);
            modelBuilder.Entity<Employee>().Property<string>(x => x.ImagePath).HasColumnName(@"ImagePath").HasColumnType(@"text").ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<Employee>().Property<long>(x => x.OrganisationId).HasColumnName(@"OrganisationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<Employee>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<Employee>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property<long>(x => x.RoleId).HasColumnName(@"RoleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property<int?>(x => x.LoginFailCount).HasColumnName(@"LoginFailCount").HasColumnType(@"int").ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property<string>(x => x.ProfileImageFile).HasColumnName(@"ProfileImageFile").HasColumnType(@"nvarchar(500)").ValueGeneratedNever();
         
            modelBuilder.Entity<Employee>().HasKey(@"EmployeeId");
            modelBuilder.Entity<Employee>().HasIndex(@"EmailId").IsUnique(true);
        }

        partial void CustomizeEmployeeMapping(ModelBuilder modelBuilder);

        #endregion

        #region ErrorLog Mapping

        private void ErrorLogMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ErrorLog>().ToTable(@"ErrorLog", @"dbo");
            modelBuilder.Entity<ErrorLog>().Property<long>(x => x.LogId).HasColumnName(@"LogId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<ErrorLog>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ErrorLog>().Property<string>(x => x.PageName).HasColumnName(@"PageName").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<ErrorLog>().Property<string>(x => x.FunctionName).HasColumnName(@"FunctionName").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<ErrorLog>().Property<string>(x => x.ErrorDetail).HasColumnName(@"ErrorDetail").HasColumnType(@"nvarchar(max)").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ErrorLog>().HasKey(@"LogId");
        }

        partial void CustomizeErrorLogMapping(ModelBuilder modelBuilder);

        #endregion

        #region GoalUnlockDate Mapping

        private void GoalUnlockDateMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalUnlockDate>().ToTable(@"GoalUnlockDate", @"dbo");
            modelBuilder.Entity<GoalUnlockDate>().Property<long>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<GoalUnlockDate>().Property<long>(x => x.OrganisationCycleId).HasColumnName(@"OrganisationCycleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<GoalUnlockDate>().Property<int>(x => x.Type).HasColumnName(@"Type").HasColumnType(@"int").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<GoalUnlockDate>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<GoalUnlockDate>().Property<System.DateTime>(x => x.SubmitDate).HasColumnName(@"SubmitDate").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<GoalUnlockDate>().HasKey(@"Id");
        }

        partial void CustomizeGoalUnlockDateMapping(ModelBuilder modelBuilder);

        #endregion

        #region MailTemplate Mapping

        private void MailTemplateMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MailTemplate>().ToTable(@"MailTemplate", @"dbo");
            modelBuilder.Entity<MailTemplate>().Property<int>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<MailTemplate>().Property<string>(x => x.TempleteSubject).HasColumnName(@"TempleteSubject").HasColumnType(@"text").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<MailTemplate>().Property<string>(x => x.TempleteBody).HasColumnName(@"TempleteBody").HasColumnType(@"text").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<MailTemplate>().Property<int>(x => x.Status).HasColumnName(@"Status").HasColumnType(@"int").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<MailTemplate>().HasKey(@"Id");
        }

        partial void CustomizeMailTemplateMapping(ModelBuilder modelBuilder);

        #endregion

        #region ModuleMaster Mapping

        private void ModuleMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleMaster>().ToTable(@"ModuleMaster", @"dbo");
            modelBuilder.Entity<ModuleMaster>().Property<long>(x => x.ModuleId).HasColumnName(@"ModuleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<ModuleMaster>().Property<string>(x => x.ModuleName).HasColumnName(@"ModuleName").HasColumnType(@"varchar(30)").IsRequired().ValueGeneratedNever().HasMaxLength(30);
            modelBuilder.Entity<ModuleMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"varchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<ModuleMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ModuleMaster>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<ModuleMaster>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ModuleMaster>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<ModuleMaster>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<ModuleMaster>().HasKey(@"ModuleId");
        }

        partial void CustomizeModuleMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region ObjectivesMaster Mapping

        private void ObjectivesMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObjectivesMaster>().ToTable(@"ObjectivesMaster", @"dbo");
            modelBuilder.Entity<ObjectivesMaster>().Property<int>(x => x.ObjectiveId).HasColumnName(@"ObjectiveId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<ObjectivesMaster>().Property<string>(x => x.ObjectiveName).HasColumnName(@"ObjectiveName").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<ObjectivesMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(200)").ValueGeneratedNever().HasMaxLength(200);
            modelBuilder.Entity<ObjectivesMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ObjectivesMaster>().HasKey(@"ObjectiveId");
        }

        partial void CustomizeObjectivesMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region OkrStatusMaster Mapping

        private void OkrStatusMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OkrStatusMaster>().ToTable(@"OkrStatusMaster", @"dbo");
            modelBuilder.Entity<OkrStatusMaster>().Property<int>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<OkrStatusMaster>().Property<string>(x => x.StatusName).HasColumnName(@"StatusName").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<OkrStatusMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(100)").ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<OkrStatusMaster>().Property<string>(x => x.Code).HasColumnName(@"Code").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<OkrStatusMaster>().Property<string>(x => x.Color).HasColumnName(@"Color").HasColumnType(@"varchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<OkrStatusMaster>().Property<System.DateTime?>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<OkrStatusMaster>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<OkrStatusMaster>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<OkrStatusMaster>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<OkrStatusMaster>().HasKey(@"Id");
        }

        partial void CustomizeOkrStatusMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region OrganisationCycle Mapping

        private void OrganisationCycleMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganisationCycle>().ToTable(@"OrganisationCycle", @"dbo");
            modelBuilder.Entity<OrganisationCycle>().Property<long>(x => x.OrganisationCycleId).HasColumnName(@"OrganisationCycleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<OrganisationCycle>().Property<long>(x => x.CycleDurationId).HasColumnName(@"CycleDurationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<int>(x => x.SymbolId).HasColumnName(@"SymbolId").HasColumnType(@"int").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<long>(x => x.OrganisationId).HasColumnName(@"OrganisationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<OrganisationCycle>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<OrganisationCycle>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<System.DateTime>(x => x.CycleStartDate).HasColumnName(@"CycleStartDate").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<System.DateTime?>(x => x.CycleEndDate).HasColumnName(@"CycleEndDate").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<int?>(x => x.CycleYear).HasColumnName(@"CycleYear").HasColumnType(@"int").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<OrganisationCycle>().Property<bool?>(x => x.IsDiscarded).HasColumnName(@"IsDiscarded").HasColumnType(@"bit").ValueGeneratedNever();
            modelBuilder.Entity<OrganisationCycle>().Property<bool?>(x => x.IsProcessed).HasColumnName(@"IsProcessed").HasColumnType(@"bit").ValueGeneratedOnAdd().HasDefaultValueSql(@"0");
            modelBuilder.Entity<OrganisationCycle>().HasKey(@"OrganisationCycleId");
        }

        partial void CustomizeOrganisationCycleMapping(ModelBuilder modelBuilder);

        #endregion

        #region Organisation Mapping

        private void OrganisationMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organisation>().ToTable(@"Organisations", @"dbo");
            modelBuilder.Entity<Organisation>().Property<long>(x => x.OrganisationId).HasColumnName(@"OrganisationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<Organisation>().Property<string>(x => x.OrganisationName).HasColumnName(@"OrganisationName").HasColumnType(@"varchar(200)").IsRequired().ValueGeneratedNever().HasMaxLength(200);
            modelBuilder.Entity<Organisation>().Property<long?>(x => x.OrganisationHead).HasColumnName(@"OrganisationHead").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<Organisation>().Property<string>(x => x.ImagePath).HasColumnName(@"ImagePath").HasColumnType(@"text").ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<Organisation>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Organisation>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<Organisation>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Organisation>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<Organisation>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<Organisation>().Property<bool?>(x => x.IsDeleted).HasColumnName(@"IsDeleted").HasColumnType(@"bit").ValueGeneratedNever();
            modelBuilder.Entity<Organisation>().Property<long?>(x => x.ParentId).HasColumnName(@"ParentId").HasColumnType(@"bigint").ValueGeneratedNever();
            modelBuilder.Entity<Organisation>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"varchar(500)").ValueGeneratedNever().HasMaxLength(500);
            modelBuilder.Entity<Organisation>().Property<string>(x => x.LogoName).HasColumnName(@"LogoName").HasColumnType(@"varchar(100)").ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<Organisation>().Property<string>(x => x.ColorCode).HasColumnName(@"ColorCode").HasColumnType(@"varchar(100)").ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<Organisation>().Property<string>(x => x.BackGroundColorCode).HasColumnName(@"BackGroundColorCode").HasColumnType(@"varchar(100)").ValueGeneratedNever().HasMaxLength(100);


            modelBuilder.Entity<Organisation>().HasKey(@"OrganisationId");
        }

        partial void CustomizeOrganisationMapping(ModelBuilder modelBuilder);

        #endregion

        #region OrganizationObjective Mapping

        private void OrganizationObjectiveMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrganizationObjective>().ToTable(@"OrganizationObjectives", @"dbo");
            modelBuilder.Entity<OrganizationObjective>().Property<long>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<OrganizationObjective>().Property<long>(x => x.OrganisationId).HasColumnName(@"OrganisationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganizationObjective>().Property<int>(x => x.ObjectiveId).HasColumnName(@"ObjectiveId").HasColumnType(@"int").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganizationObjective>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganizationObjective>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<OrganizationObjective>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<OrganizationObjective>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<OrganizationObjective>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<OrganizationObjective>().Property<bool?>(x => x.IsDiscarded).HasColumnName(@"IsDiscarded").HasColumnType(@"bit").ValueGeneratedNever();
            modelBuilder.Entity<OrganizationObjective>().HasKey(@"Id");
        }

        partial void CustomizeOrganizationObjectiveMapping(ModelBuilder modelBuilder);

        #endregion

        #region PermissionMaster Mapping

        private void PermissionMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermissionMaster>().ToTable(@"PermissionMaster", @"dbo");
            modelBuilder.Entity<PermissionMaster>().Property<long>(x => x.PermissionId).HasColumnName(@"PermissionId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<PermissionMaster>().Property<long>(x => x.ModuleId).HasColumnName(@"ModuleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionMaster>().Property<string>(x => x.Permission).HasColumnName(@"Permission").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<PermissionMaster>().Property<string>(x => x.PermissionDescription).HasColumnName(@"PermissionDescription").HasColumnType(@"varchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<PermissionMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionMaster>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<PermissionMaster>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionMaster>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<PermissionMaster>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<PermissionMaster>().HasKey(@"PermissionId");
        }

        partial void CustomizePermissionMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region PermissionRoleMapping Mapping

        private void PermissionRoleMappingMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermissionRoleMapping>().ToTable(@"PermissionRoleMapping", @"dbo");
            modelBuilder.Entity<PermissionRoleMapping>().Property<long>(x => x.PermissionMappingId).HasColumnName(@"PermissionMappingId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<PermissionRoleMapping>().Property<long>(x => x.RoleId).HasColumnName(@"RoleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionRoleMapping>().Property<long>(x => x.PermissionId).HasColumnName(@"PermissionId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionRoleMapping>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionRoleMapping>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<PermissionRoleMapping>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PermissionRoleMapping>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<PermissionRoleMapping>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<PermissionRoleMapping>().HasKey(@"PermissionMappingId");
        }

        partial void CustomizePermissionRoleMappingMapping(ModelBuilder modelBuilder);

        #endregion

        #region RoleMaster Mapping

        private void RoleMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RoleMaster>().ToTable(@"RoleMaster", @"dbo");
            modelBuilder.Entity<RoleMaster>().Property<long>(x => x.RoleId).HasColumnName(@"RoleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<RoleMaster>().Property<string>(x => x.RoleName).HasColumnName(@"RoleName").HasColumnType(@"varchar(20)").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<RoleMaster>().Property<string>(x => x.RoleDescription).HasColumnName(@"RoleDescription").HasColumnType(@"varchar(50)").ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<RoleMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<RoleMaster>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<RoleMaster>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<RoleMaster>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<RoleMaster>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<RoleMaster>().HasKey(@"RoleId");
            modelBuilder.Entity<RoleMaster>().HasIndex(@"RoleName").IsUnique(true);
        }

        partial void CustomizeRoleMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region UserDetail Mapping

        private void UserDetailMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDetail>().ToTable(@"UserDetail", @"dbo");
            modelBuilder.Entity<UserDetail>().Property<long>(x => x.UserId).HasColumnName(@"UserId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.FirstName).HasColumnName(@"FirstName").HasColumnType(@"varchar(20)").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.LastName).HasColumnName(@"LastName").HasColumnType(@"varchar(20)").ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<UserDetail>().Property<long>(x => x.RoleId).HasColumnName(@"RoleId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<long>(x => x.EmployeeId).HasColumnName(@"EmployeeId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.EmailId).HasColumnName(@"EmailId").HasColumnType(@"nvarchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.Password).HasColumnName(@"Password").HasColumnType(@"text").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.Salt).HasColumnName(@"Salt").HasColumnType(@"text").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<UserDetail>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<int?>(x => x.Status).HasColumnName(@"Status").HasColumnType(@"int").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<long?>(x => x.ReportingTo).HasColumnName(@"ReportingTo").HasColumnType(@"bigint").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.ImagePath).HasColumnName(@"ImagePath").HasColumnType(@"text").ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<UserDetail>().Property<System.DateTime?>(x => x.LastLoginTime).HasColumnName(@"LastLoginTime").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<int?>(x => x.LoginCount).HasColumnName(@"LoginCount").HasColumnType(@"int").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<string>(x => x.ImageDetails).HasColumnName(@"ImageDetails").HasColumnType(@"nvarchar(max)").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().Property<long?>(x => x.OrganisationId).HasColumnName(@"OrganisationId").HasColumnType(@"bigint").ValueGeneratedNever();
            modelBuilder.Entity<UserDetail>().HasKey(@"UserId");
            modelBuilder.Entity<UserDetail>().HasIndex(@"EmployeeId").IsUnique(true);
            modelBuilder.Entity<UserDetail>().HasIndex(@"EmailId").IsUnique(true);
        }

        partial void CustomizeUserDetailMapping(ModelBuilder modelBuilder);

        #endregion

        #region UserToken Mapping

        private void UserTokenMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserToken>().ToTable(@"UserToken", @"dbo");
            modelBuilder.Entity<UserToken>().Property<long>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<UserToken>().Property<long>(x => x.EmployeeId).HasColumnName(@"EmployeeId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<UserToken>().Property<string>(x => x.Token).HasColumnName(@"Token").HasColumnType(@"nvarchar(MAX)").IsRequired().ValueGeneratedNever().HasMaxLength(2147483647);
            modelBuilder.Entity<UserToken>().Property<System.DateTime>(x => x.ExpireTime).HasColumnName(@"ExpireTime").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<UserToken>().Property<int?>(x => x.TokenType).HasColumnName(@"TokenType").HasColumnType(@"int").ValueGeneratedNever();
            modelBuilder.Entity<UserToken>().Property<System.DateTime?>(x => x.LastLoginDate).HasColumnName(@"LastLoginDate").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<UserToken>().Property<System.DateTime?>(x => x.CurrentLoginDate).HasColumnName(@"CurrentLoginDate").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<UserToken>().HasKey(@"Id");
        }

        partial void CustomizeUserTokenMapping(ModelBuilder modelBuilder);

        #endregion

        #region CycleDurationSymbol Mapping

        private void CycleDurationSymbolMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CycleDurationSymbol>().ToTable(@"CycleDurationSymbols", @"dbo");
            modelBuilder.Entity<CycleDurationSymbol>().Property<int>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<CycleDurationSymbol>().Property<long>(x => x.CycleDurationId).HasColumnName(@"CycleDurationId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<CycleDurationSymbol>().Property<string>(x => x.Symbol).HasColumnName(@"Symbol").HasColumnType(@"varchar(20)").IsRequired().ValueGeneratedNever().HasMaxLength(20);
            modelBuilder.Entity<CycleDurationSymbol>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"varchar(50)").ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<CycleDurationSymbol>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<CycleDurationSymbol>().HasKey(@"Id");
        }

        partial void CustomizeCycleDurationSymbolMapping(ModelBuilder modelBuilder);

        #endregion

        #region EmployeeContactDetail Mapping

        private void EmployeeContactDetailMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeContactDetail>().ToTable(@"EmployeeContactDetail", @"dbo");
            modelBuilder.Entity<EmployeeContactDetail>().Property<long>(x => x.ContactId).HasColumnName(@"ContactId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<EmployeeContactDetail>().Property<long>(x => x.EmployeeId).HasColumnName(@"EmployeeId").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<EmployeeContactDetail>().Property<string>(x => x.PhoneNumber).HasColumnName(@"PhoneNumber").HasColumnType(@"nvarchar(25)").ValueGeneratedNever().HasMaxLength(25);
            modelBuilder.Entity<EmployeeContactDetail>().Property<string>(x => x.CountryStdCode).HasColumnName(@"CountryStdCode").HasColumnType(@"nvarchar(10)").ValueGeneratedNever().HasMaxLength(10);
            modelBuilder.Entity<EmployeeContactDetail>().Property<string>(x => x.DeskPhoneNumber).HasColumnName(@"DeskPhoneNumber").HasColumnType(@"nvarchar(25)").ValueGeneratedNever().HasMaxLength(25);
            modelBuilder.Entity<EmployeeContactDetail>().Property<string>(x => x.SkypeUrl).HasColumnName(@"SkypeUrl").HasColumnType(@"nvarchar(250)").ValueGeneratedNever().HasMaxLength(250);
            modelBuilder.Entity<EmployeeContactDetail>().Property<string>(x => x.TwitterUrl).HasColumnName(@"TwitterUrl").HasColumnType(@"nvarchar(250)").ValueGeneratedNever().HasMaxLength(250);
            modelBuilder.Entity<EmployeeContactDetail>().Property<string>(x => x.LinkedInUrl).HasColumnName(@"LinkedInUrl").HasColumnType(@"nvarchar(250)").ValueGeneratedNever().HasMaxLength(250);
            modelBuilder.Entity<EmployeeContactDetail>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<EmployeeContactDetail>().Property<System.DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<EmployeeContactDetail>().Property<long?>(x => x.UpdatedBy).HasColumnName(@"UpdatedBy").HasColumnType(@"bigint").ValueGeneratedNever();
            modelBuilder.Entity<EmployeeContactDetail>().Property<System.DateTime?>(x => x.UpdatedOn).HasColumnName(@"UpdatedOn").HasColumnType(@"datetime").ValueGeneratedNever();
            modelBuilder.Entity<EmployeeContactDetail>().HasKey(@"ContactId");
        }

        partial void CustomizeEmployeeContactDetailMapping(ModelBuilder modelBuilder);

        #endregion

        #region AssignmentTypeMaster Mapping

        private void AssignmentTypeMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssignmentTypeMaster>().ToTable(@"AssignmentTypeMaster", @"dbo");
            modelBuilder.Entity<AssignmentTypeMaster>().Property<int>(x => x.AssignmentTypeId).HasColumnName(@"AssignmentTypeId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<AssignmentTypeMaster>().Property<string>(x => x.Name).HasColumnName(@"Name").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<AssignmentTypeMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(200)").ValueGeneratedNever().HasMaxLength(200);
            modelBuilder.Entity<AssignmentTypeMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<AssignmentTypeMaster>().Property<bool>(x => x.IsDefault).HasColumnName(@"IsDefault").HasColumnType(@"bit").ValueGeneratedOnAdd();
            modelBuilder.Entity<AssignmentTypeMaster>().HasKey(@"AssignmentTypeId");
        }

        partial void CustomizeAssignmentTypeMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region JobsAudit Mapping

        private void JobsAuditMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobsAudit>().ToTable(@"JobsAudit", @"dbo");
            modelBuilder.Entity<JobsAudit>().Property<long>(x => x.AuditId).HasColumnName(@"AuditId").HasColumnType(@"bigint").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<JobsAudit>().Property<string>(x => x.JobName).HasColumnName(@"JobName").HasColumnType(@"nvarchar(500)").IsRequired().ValueGeneratedNever().HasMaxLength(500);
            modelBuilder.Entity<JobsAudit>().Property<string>(x => x.Status).HasColumnName(@"Status").HasColumnType(@"nvarchar(100)").ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<JobsAudit>().Property<string>(x => x.Details).HasColumnName(@"Details").HasColumnType(@"nvarchar(500)").ValueGeneratedNever().HasMaxLength(500);
            modelBuilder.Entity<JobsAudit>().Property<System.DateTime>(x => x.ExecutionDate).HasColumnName(@"ExecutionDate").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<JobsAudit>().Property<System.DateTime>(x => x.AuditDate).HasColumnName(@"AuditDate").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<JobsAudit>().HasKey(@"AuditId");
        }

        partial void CustomizeJobsAuditMapping(ModelBuilder modelBuilder);

        #endregion

        #region MetricDataMaster Mapping

        private void MetricDataMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetricDataMaster>().ToTable(@"MetricDataMaster", @"dbo");
            modelBuilder.Entity<MetricDataMaster>().Property<int>(x => x.DataId).HasColumnName(@"DataId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<MetricDataMaster>().Property<int>(x => x.MetricId).HasColumnName(@"MetricId").HasColumnType(@"int").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<MetricDataMaster>().Property<string>(x => x.Name).HasColumnName(@"Name").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<MetricDataMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(200)").ValueGeneratedNever().HasMaxLength(200);
            modelBuilder.Entity<MetricDataMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<MetricDataMaster>().Property<string>(x => x.Symbol).HasColumnName(@"Symbol").HasColumnType(@"nchar(1)").ValueGeneratedNever();
            modelBuilder.Entity<MetricDataMaster>().Property<bool>(x => x.IsDefault).HasColumnName(@"IsDefault").HasColumnType(@"bit").ValueGeneratedOnAdd();
            modelBuilder.Entity<MetricDataMaster>().HasKey(@"DataId");
        }

        partial void CustomizeMetricDataMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region MetricMaster Mapping

        private void MetricMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetricMaster>().ToTable(@"MetricMaster", @"dbo");
            modelBuilder.Entity<MetricMaster>().Property<int>(x => x.MetricId).HasColumnName(@"MetricId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<MetricMaster>().Property<string>(x => x.Name).HasColumnName(@"Name").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<MetricMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(200)").ValueGeneratedNever().HasMaxLength(200);
            modelBuilder.Entity<MetricMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<MetricMaster>().HasKey(@"MetricId");
        }

        partial void CustomizeMetricMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region KrStatusMaster Mapping

        private void KrStatusMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KrStatusMaster>().ToTable(@"KrStatusMaster", @"dbo");
            modelBuilder.Entity<KrStatusMaster>().Property<int>(x => x.KrStatusId).HasColumnName(@"KrStatusId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();

            modelBuilder.Entity<KrStatusMaster>().Property<string>(x => x.Name).HasColumnName(@"Name").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<KrStatusMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<KrStatusMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<KrStatusMaster>().Property<bool>(x => x.IsDefault).HasColumnName(@"IsDefault").HasColumnType(@"bit").ValueGeneratedOnAdd();
            modelBuilder.Entity<KrStatusMaster>().HasKey(@"KrStatusId");
        }

        partial void CustomizeKrStatusMasterMapping(ModelBuilder modelBuilder);

        #endregion



        #region GoalStatusMaster Mapping

        private void GoalStatusMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalStatusMaster>().ToTable(@"GoalStatusMaster", @"dbo");
            modelBuilder.Entity<GoalStatusMaster>().Property<int>(x => x.GoalStatusId).HasColumnName(@"GoalStatusId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();

            modelBuilder.Entity<GoalStatusMaster>().Property<string>(x => x.Name).HasColumnName(@"Name").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<GoalStatusMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);

            modelBuilder.Entity<GoalStatusMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<GoalStatusMaster>().Property<bool>(x => x.IsDefault).HasColumnName(@"IsDefault").HasColumnType(@"bit").ValueGeneratedOnAdd();
            modelBuilder.Entity<GoalStatusMaster>().HasKey(@"GoalStatusId");
        }

        partial void CustomizeGoalStatusMasterMapping(ModelBuilder modelBuilder);

        #endregion


        #region GoalTypeMaster Mapping

        private void GoalTypeMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoalTypeMaster>().ToTable(@"GoalTypeMaster", @"dbo");
            modelBuilder.Entity<GoalTypeMaster>().Property<int>(x => x.GoalTypeId).HasColumnName(@"GoalTypeId").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();

            modelBuilder.Entity<GoalTypeMaster>().Property<string>(x => x.Name).HasColumnName(@"Name").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<GoalTypeMaster>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);

            modelBuilder.Entity<GoalTypeMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");
            modelBuilder.Entity<GoalTypeMaster>().Property<bool>(x => x.IsDefault).HasColumnName(@"IsDefault").HasColumnType(@"bit").ValueGeneratedOnAdd();
            modelBuilder.Entity<GoalTypeMaster>().HasKey(@"GoalTypeId");
        }

        partial void CustomizeGoalTypeMasterMapping(ModelBuilder modelBuilder);

        #endregion

        #region OkrTypeFilter Mapping

        private void OkrTypeFilterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OkrTypeFilter>().ToTable(@"OkrTypeFilter", @"dbo");
            modelBuilder.Entity<OkrTypeFilter>().Property<int>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();

            modelBuilder.Entity<OkrTypeFilter>().Property<string>(x => x.StatusName).HasColumnName(@"StatusName").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<OkrTypeFilter>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<OkrTypeFilter>().Property<string>(x => x.Code).HasColumnName(@"Code").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<OkrTypeFilter>().Property<string>(x => x.Color).HasColumnName(@"Color").HasColumnType(@"varchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<OkrTypeFilter>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");

        }

        partial void CustomizeOkrTypeFilterMapping(ModelBuilder modelBuilder);

        #endregion

        #region DirectReporteesMapping

        private void DirectReporteesFilterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DirectReporteesFilter>().ToTable(@"DirectReporteesFilter", @"dbo");
            modelBuilder.Entity<DirectReporteesFilter>().Property<int>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();

            modelBuilder.Entity<DirectReporteesFilter>().Property<string>(x => x.StatusName).HasColumnName(@"StatusName").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<DirectReporteesFilter>().Property<string>(x => x.Description).HasColumnName(@"Description").HasColumnType(@"nvarchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<DirectReporteesFilter>().Property<string>(x => x.Code).HasColumnName(@"Code").HasColumnType(@"varchar(50)").IsRequired().ValueGeneratedNever().HasMaxLength(50);
            modelBuilder.Entity<DirectReporteesFilter>().Property<string>(x => x.Color).HasColumnName(@"Color").HasColumnType(@"varchar(100)").IsRequired().ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<DirectReporteesFilter>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedOnAdd().HasDefaultValueSql(@"1");

        }

        partial void CustomizeDirectReporteesFilterMapping(ModelBuilder modelBuilder);

        #endregion

        #region ColorCodeMapping

        private void ColorCodeMasterMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ColorCodeMaster>().ToTable(@"ColorCodeMaster", @"dbo");
            modelBuilder.Entity<ColorCodeMaster>().Property<int>(x => x.Id).HasColumnName(@"Id").HasColumnType(@"int").IsRequired().ValueGeneratedOnAdd();

            modelBuilder.Entity<ColorCodeMaster>().Property<string>(x => x.ColorCode).HasColumnName(@"ColorCode").HasColumnType(@"varchar(100)").ValueGeneratedNever().HasMaxLength(100);
            modelBuilder.Entity<ColorCodeMaster>().Property<DateTime>(x => x.CreatedOn).HasColumnName(@"CreatedOn").HasColumnType(@"datetime").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ColorCodeMaster>().Property<long>(x => x.CreatedBy).HasColumnName(@"CreatedBy").HasColumnType(@"bigint").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ColorCodeMaster>().Property<bool>(x => x.IsActive).HasColumnName(@"IsActive").HasColumnType(@"bit").IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<ColorCodeMaster>().Property<string>(x => x.BackGroundColorCode).HasColumnName(@"BackGroundColorCode").HasColumnType(@"varchar(100)").ValueGeneratedNever().HasMaxLength(100);

        }

        partial void CustomizeColorCodeMasterMapping(ModelBuilder modelBuilder);

        #endregion


        private void RelationshipsMapping(ModelBuilder modelBuilder)
        {

            #region CycleDurationMaster Navigation properties

            modelBuilder.Entity<CycleDurationMaster>().HasMany(x => x.OrganisationCycles).WithOne(op => op.CycleDurationMaster).IsRequired(true).HasForeignKey(@"CycleDurationId");
            modelBuilder.Entity<CycleDurationMaster>().HasMany(x => x.CycleDurationSymbols).WithOne(op => op.CycleDurationMaster).IsRequired(true).HasForeignKey(@"CycleDurationId");

            #endregion

            #region Employee Navigation properties

            modelBuilder.Entity<Employee>().HasOne(x => x.Organisation).WithMany(op => op.Employees).IsRequired(true).HasForeignKey(@"OrganisationId");
            modelBuilder.Entity<Employee>().HasOne(x => x.RoleMaster).WithMany(op => op.Employees).IsRequired(true).HasForeignKey(@"RoleId");
            modelBuilder.Entity<Employee>().HasMany(x => x.EmployeeContactDetails).WithOne(op => op.Employee).IsRequired(true).HasForeignKey(@"EmployeeId");

            #endregion

            #region ModuleMaster Navigation properties

            modelBuilder.Entity<ModuleMaster>().HasMany(x => x.PermissionMasters).WithOne(op => op.ModuleMaster).IsRequired(true).HasForeignKey(@"ModuleId");

            #endregion

            #region ObjectivesMaster Navigation properties

            modelBuilder.Entity<ObjectivesMaster>().HasMany(x => x.OrganizationObjectives).WithOne(op => op.ObjectivesMaster).IsRequired(true).HasForeignKey(@"ObjectiveId");

            #endregion

            #region OrganisationCycle Navigation properties

            modelBuilder.Entity<OrganisationCycle>().HasOne(x => x.CycleDurationMaster).WithMany(op => op.OrganisationCycles).IsRequired(true).HasForeignKey(@"CycleDurationId");
            modelBuilder.Entity<OrganisationCycle>().HasOne(x => x.Organisation).WithMany(op => op.OrganisationCycles).IsRequired(true).HasForeignKey(@"OrganisationId");
            modelBuilder.Entity<OrganisationCycle>().HasOne(x => x.CycleDurationSymbol).WithMany(op => op.OrganisationCycles).IsRequired(true).HasForeignKey(@"SymbolId");

            #endregion

            #region Organisation Navigation properties

            modelBuilder.Entity<Organisation>().HasMany(x => x.Employees).WithOne(op => op.Organisation).IsRequired(true).HasForeignKey(@"OrganisationId");
            modelBuilder.Entity<Organisation>().HasMany(x => x.OrganisationCycles).WithOne(op => op.Organisation).IsRequired(true).HasForeignKey(@"OrganisationId");
            modelBuilder.Entity<Organisation>().HasMany(x => x.OrganizationObjectives).WithOne(op => op.Organisation).IsRequired(true).HasForeignKey(@"OrganisationId");

            #endregion

            #region OrganizationObjective Navigation properties

            modelBuilder.Entity<OrganizationObjective>().HasOne(x => x.ObjectivesMaster).WithMany(op => op.OrganizationObjectives).IsRequired(true).HasForeignKey(@"ObjectiveId");
            modelBuilder.Entity<OrganizationObjective>().HasOne(x => x.Organisation).WithMany(op => op.OrganizationObjectives).IsRequired(true).HasForeignKey(@"OrganisationId");

            #endregion

            #region PermissionMaster Navigation properties

            modelBuilder.Entity<PermissionMaster>().HasOne(x => x.ModuleMaster).WithMany(op => op.PermissionMasters).IsRequired(true).HasForeignKey(@"ModuleId");
            modelBuilder.Entity<PermissionMaster>().HasMany(x => x.PermissionRoleMappings).WithOne(op => op.PermissionMaster).IsRequired(true).HasForeignKey(@"PermissionId");

            #endregion

            #region PermissionRoleMapping Navigation properties

            modelBuilder.Entity<PermissionRoleMapping>().HasOne(x => x.RoleMaster).WithMany(op => op.PermissionRoleMappings).IsRequired(true).HasForeignKey(@"RoleId");
            modelBuilder.Entity<PermissionRoleMapping>().HasOne(x => x.PermissionMaster).WithMany(op => op.PermissionRoleMappings).IsRequired(true).HasForeignKey(@"PermissionId");

            #endregion

            #region RoleMaster Navigation properties

            modelBuilder.Entity<RoleMaster>().HasMany(x => x.PermissionRoleMappings).WithOne(op => op.RoleMaster).IsRequired(true).HasForeignKey(@"RoleId");
            modelBuilder.Entity<RoleMaster>().HasMany(x => x.Employees).WithOne(op => op.RoleMaster).IsRequired(true).HasForeignKey(@"RoleId");

            #endregion

            #region CycleDurationSymbol Navigation properties

            modelBuilder.Entity<CycleDurationSymbol>().HasOne(x => x.CycleDurationMaster).WithMany(op => op.CycleDurationSymbols).IsRequired(true).HasForeignKey(@"CycleDurationId");
            modelBuilder.Entity<CycleDurationSymbol>().HasMany(x => x.OrganisationCycles).WithOne(op => op.CycleDurationSymbol).IsRequired(true).HasForeignKey(@"SymbolId");

            #endregion

            #region EmployeeContactDetail Navigation properties

            modelBuilder.Entity<EmployeeContactDetail>().HasOne(x => x.Employee).WithMany(op => op.EmployeeContactDetails).IsRequired(true).HasForeignKey(@"EmployeeId");
            #endregion
        }

        partial void CustomizeMapping(ref ModelBuilder modelBuilder);

        public bool HasChanges()
        {
            return ChangeTracker.Entries().Any(e => e.State == Microsoft.EntityFrameworkCore.EntityState.Added || e.State == Microsoft.EntityFrameworkCore.EntityState.Modified || e.State == Microsoft.EntityFrameworkCore.EntityState.Deleted);
        }

        partial void OnCreated();
    }
}
