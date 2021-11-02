using OKRAdminService.EF;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.Reflection;
using Xunit;

namespace OKRAdminService.UnitTests.Models
{
    public class ModelTestCases
    {
        [Fact]
        public void EmployeeModel()
        {
            Employee model = new Employee();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void EmployeeContactDetailModel()
        {
            EmployeeContactDetail model = new EmployeeContactDetail();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void CycleDurationMasterModel()
        {
            CycleDurationMaster model = new CycleDurationMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void CycleDurationSymbolModel()
        {
            CycleDurationSymbol model = new CycleDurationSymbol();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ErrorLogModel()
        {
            ErrorLog model = new ErrorLog();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ModuleMasterModel()
        {
            ModuleMaster model = new ModuleMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void ObjectivesMasterModel()
        {
            ObjectivesMaster model = new ObjectivesMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void OkrStatusMasterModel()
        {
            OkrStatusMaster model = new OkrStatusMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void OrganisationModel()
        {
            Organisation model = new Organisation();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void OrganisationCycleModel()
        {
            OrganisationCycle model = new OrganisationCycle();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void OrganisationObjectivesModel()
        {
            OrganisationObjectives model = new OrganisationObjectives();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PermissionMasterModel()
        {
            PermissionMaster model = new PermissionMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void PermissionRoleMappingModel()
        {
            PermissionRoleMapping model = new PermissionRoleMapping();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void RoleMasterModel()
        {
            RoleMaster model = new RoleMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UserTokenModel()
        {
            UserToken model = new UserToken();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void AssignmentTypeResponseModel()
        {
            AssignmentTypeResponse model = new AssignmentTypeResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MetricDataMasterResponseModel()
        {
            MetricDataMasterResponse model = new MetricDataMasterResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void MetricDataMasterModel()
        {
            MetricDataMaster model = new MetricDataMaster();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void GoalStatus()
        {
            GoalStatusResponse model = new GoalStatusResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void GoalType()
        {
            GoalTypeResponse model = new GoalTypeResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }
        [Fact]
        public void KrStatus()
        {
            KrStatusResponse model = new KrStatusResponse();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void TeamDetails()
        {
            TeamDetails model = new TeamDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void SubTeamDetails()
        {
            SubTeamDetails model = new SubTeamDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void TeamEmployeeDetails()
        {
            TeamEmployeeDetails model = new TeamEmployeeDetails();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        [Fact]
        public void UpdateTeamLeaderOkrRequest()
        {
            UpdateTeamLeaderOkrRequest model = new UpdateTeamLeaderOkrRequest();
            var resultGet = GetModelTestData(model);
            var resultSet = SetModelTestData(model);
            Assert.NotNull(resultGet);
            Assert.NotNull(resultSet);
        }

        private T GetModelTestData<T>(T newModel)
        {
            Type type = newModel.GetType();
            PropertyInfo[] properties = type.GetProperties();
            object value = null;
            foreach (var prop in properties)
            {
                var propTypeInfo = type.GetProperty(prop.Name.Trim());
                if (propTypeInfo.CanRead)
                    value = prop.GetValue(newModel);
            }
            return newModel;
        }
        private T SetModelTestData<T>(T newModel)
        {
            Type type = newModel.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var propTypeInfo = type.GetProperty(prop.Name.Trim());
                var propType = prop.GetType();

                if (propTypeInfo.CanWrite)
                {
                    if (prop.PropertyType.Name == "String")
                    {
                        prop.SetValue(newModel, String.Empty);
                    }
                    else if (propType.IsValueType)
                    {
                        prop.SetValue(newModel, Activator.CreateInstance(propType));
                    }
                    else
                    {
                        prop.SetValue(newModel, null);
                    }
                }
            }
            return newModel;
        }
    }
}
