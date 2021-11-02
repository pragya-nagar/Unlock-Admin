using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Response;
using OKRAdminService.WebCore.Controllers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OKRAdminService.UnitTests.Controller
{
    public class RoleControllerTestCases
    {
        private readonly Mock<IRoleService> _roleService;
        private readonly Mock<IIdentityService> _identityService;
        private readonly RoleController _roleController;
        public RoleControllerTestCases()
        {
            _roleService = new Mock<IRoleService>();
            _identityService = new Mock<IIdentityService>();
            _roleController = new RoleController(_identityService.Object, _roleService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task CreateRoleAsync_InValidToken()
        {
            ///arrange
            RoleRequestModel roleRequestModel = new RoleRequestModel() { RoleName = "User" };
            RoleMaster roleMaster = new RoleMaster();
            ///act
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.CreateRoleAsync(It.IsAny<RoleRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(roleRequestModel);
            ///assert
            var result = await _roleController.CreateRoleAsync(roleRequestModel) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task CreateRoleAsync_Error()
        {
            ///arrange
            RoleRequestModel roleRequestModel = new RoleRequestModel();
            RoleMaster roleMaster = new RoleMaster();
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.CreateRoleAsync(It.IsAny<RoleRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(roleRequestModel);
            ///assert
            var result = await _roleController.CreateRoleAsync(roleRequestModel);
            PayloadCustom<RoleRequestModel> requData = ((PayloadCustom<RoleRequestModel>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task CreateRoleAsync_RoleNameExists()
        {
            ///arrange
            RoleRequestModel roleRequestModel = new RoleRequestModel() { RoleName = "User" };
            RoleMaster roleMaster = new RoleMaster();
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.CreateRoleAsync(It.IsAny<RoleRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(roleRequestModel);
            ///assert
            var result = await _roleController.CreateRoleAsync(roleRequestModel);
            PayloadCustom<RoleRequestModel> requData = ((PayloadCustom<RoleRequestModel>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task CreateRoleAsync_ValidToken()
        {
            ///arrange
            RoleRequestModel roleRequestModel = new RoleRequestModel() { RoleName = "User" };
            RoleMaster roleMaster = null;
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.CreateRoleAsync(It.IsAny<RoleRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(roleRequestModel);
            ///assert
            var result = await _roleController.CreateRoleAsync(roleRequestModel);
            PayloadCustom<RoleRequestModel> requData = ((PayloadCustom<RoleRequestModel>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task CreateRoleAsync_NotSuccess()
        {
            ///arrange
            RoleRequestModel roleRequestModel = new RoleRequestModel() { RoleName = "User" };
            RoleRequestModel roleRequestModels = null;
            RoleMaster roleMaster = null;
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.CreateRoleAsync(It.IsAny<RoleRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(roleRequestModels);
            ///assert
            var result = await _roleController.CreateRoleAsync(roleRequestModel);
            PayloadCustom<RoleRequestModel> requData = ((PayloadCustom<RoleRequestModel>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_InValidToken()
        {
            ///arrange
            AssignUserRequest assignUserRequest = new AssignUserRequest() { RoleId = 1, AssignUsers = new List<EmployeeDetailsModel>() { new EmployeeDetailsModel { EmployeeId = 1 } } };
            ///act
            _roleService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<AssignUserRequest>(), It.IsAny<long>())).ReturnsAsync(assignUserRequest);
            ///assert
            var result = await _roleController.AssignRoleToUserAsync(assignUserRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_Error()
        {
            ///arrange
            AssignUserRequest assignUserRequest = new AssignUserRequest() { AssignUsers = new List<EmployeeDetailsModel>() };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<AssignUserRequest>(), It.IsAny<long>())).ReturnsAsync(assignUserRequest);
            ///assert
            var result = await _roleController.AssignRoleToUserAsync(assignUserRequest);
            PayloadCustom<AssignUserRequest> requData = ((PayloadCustom<AssignUserRequest>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ValidToken()
        {
            ///arrange
            AssignUserRequest assignUserRequest = new AssignUserRequest() { RoleId = 1, AssignUsers = new List<EmployeeDetailsModel>() { new EmployeeDetailsModel { EmployeeId = 1 } } };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<AssignUserRequest>(), It.IsAny<long>())).ReturnsAsync(assignUserRequest);
            ///assert
            var result = await _roleController.AssignRoleToUserAsync(assignUserRequest);
            PayloadCustom<AssignUserRequest> requData = ((PayloadCustom<AssignUserRequest>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_NotSuccess()
        {
            ///arrange
            AssignUserRequest assignUserRequest = new AssignUserRequest() { RoleId = 1, AssignUsers = new List<EmployeeDetailsModel>() { new EmployeeDetailsModel { EmployeeId = 1 } } };
            AssignUserRequest assignUserRequests = null;
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.AssignRoleToUserAsync(It.IsAny<AssignUserRequest>(), It.IsAny<long>())).ReturnsAsync(assignUserRequests);
            ///assert
            var result = await _roleController.AssignRoleToUserAsync(assignUserRequest);
            PayloadCustom<AssignUserRequest> requData = ((PayloadCustom<AssignUserRequest>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task EditRoleAsync_InValidToken()
        {
            ///arrange
            RoleMaster roleMaster = new RoleMaster();
            RoleRequestModel roleRequestModel = new RoleRequestModel()
            {
                RoleId = 1,
                RoleName = "Admin",
                RoleDescription = "Admin",
                Status = true,
                AssignUsers = new List<EmployeeDetailsModel>() { new EmployeeDetailsModel() { EmployeeId = 1 } }
            };
            ///act
            _roleService.Setup(x => x.GetRoleNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.EditRoleAsync(It.IsAny<RoleRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(roleRequestModel);
            ///assert
            var result = await _roleController.EditRoleAsync(roleRequestModel) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ActiveInactiveRoleAsync_InValidToken()
        {
            ///arrange
            long roleId = 1;
            bool isActive = true;
            RoleMaster roleMaster = new RoleMaster() { RoleName = "Admin" };
            ///act
            _roleService.Setup(x => x.ActiveInactiveRoleAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(roleMaster);
            ///assert
            var result = await _roleController.ActiveInactiveRoleAsync(roleId, isActive) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ActiveInactiveRoleAsync_Error()
        {
            ///arrange
            long roleId = 0;
            bool isActive = false;
            RoleMaster roleMaster = new RoleMaster() { RoleId = 1 };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.ActiveInactiveRoleAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            ///assert
            var result = await _roleController.ActiveInactiveRoleAsync(roleId, isActive);
            PayloadCustom<RoleMaster> requData = ((PayloadCustom<RoleMaster>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ActiveInactiveRoleAsync_DefaultRole()
        {
            ///arrange
            long roleId = 1;
            bool isActive = false;
            RoleMaster roleMaster = new RoleMaster() { RoleId = 1 };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.ActiveInactiveRoleAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(roleMaster);
            _roleService.Setup(x => x.GetRoleByRoleNameAsync(It.IsAny<string>())).ReturnsAsync(roleMaster);
            ///assert
            var result = await _roleController.ActiveInactiveRoleAsync(roleId, isActive);
            PayloadCustom<RoleMaster> requData = ((PayloadCustom<RoleMaster>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ActiveInactiveRoleAsync_ValidToken()
        {
            ///arrange
            long roleId = 1;
            bool isActive = true;
            RoleMaster roleMaster = new RoleMaster() { RoleId = 2 };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.ActiveInactiveRoleAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(roleMaster);
            ///assert
            var result = await _roleController.ActiveInactiveRoleAsync(roleId, isActive);
            PayloadCustom<RoleMaster> requData = ((PayloadCustom<RoleMaster>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllRoleAsync_InValidToken()
        {
            ///act
            _roleService.Setup(x => x.GetAllRoleAsync());
            ///assert
            var result = await _roleController.GetAllRoleAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllRoleAsync_ValidToken()
        {
            ///arrange
            List<RoleResponseModel> roleResponseModel = new List<RoleResponseModel>() { new RoleResponseModel() { RoleId = 1, RoleName = "Admin" } };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetAllRoleAsync()).ReturnsAsync(roleResponseModel);
            ///assert
            var result = await _roleController.GetAllRoleAsync();
            PayloadCustom<RoleResponseModel> requData = ((PayloadCustom<RoleResponseModel>)((ObjectResult)result).Value);
            var finalList = requData.EntityList;
            Assert.NotNull(finalList);
        }

        [Fact]
        public async Task GetAllRoleAsync_NotSuccess()
        {
            ///arrange
            List<RoleResponseModel> roleResponseModel = new List<RoleResponseModel>();
            ///act
           // _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetAllRoleAsync()).ReturnsAsync(roleResponseModel);
            ///assert
            var result = await _roleController.GetAllRoleAsync();
            PayloadCustom<RoleResponseModel> requData = ((PayloadCustom<RoleResponseModel>)((ObjectResult)result).Value);
            var finalList = requData.EntityList;
            Assert.NotNull(finalList);
        }

        [Fact]
        public async Task GetRoleByUserIdAsync_InValidToken()
        {
            ///arrange
            long userId = 238;
            UserRoleDetail userRoleDetail = new UserRoleDetail();
            ///act
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);
            ///assert
            var result = await _roleController.GetRoleByUserIdAsync(userId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetRoleByUserIdAsync_Error()
        {
            ///arrange
            long userId = 0;
            UserRoleDetail userRoleDetail = new UserRoleDetail();
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);
            ///assert
            var result = await _roleController.GetRoleByUserIdAsync(userId);
            PayloadCustom<UserRoleDetail> requData = ((PayloadCustom<UserRoleDetail>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetRoleByUserIdAsync_ValidToken()
        {
            ///arrange
            long userId = 238;
            UserRoleDetail userRoleDetail = new UserRoleDetail();
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);
            ///assert
            var result = await _roleController.GetRoleByUserIdAsync(userId);
            PayloadCustom<UserRoleDetail> requData = ((PayloadCustom<UserRoleDetail>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetRoleByUserIdAsync_NotSuccess()
        {
            ///arrange
            long userId = 238;
            UserRoleDetail userRoleDetail = null;
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);
            ///assert
            var result = await _roleController.GetRoleByUserIdAsync(userId);
            PayloadCustom<UserRoleDetail> requData = ((PayloadCustom<UserRoleDetail>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task DeleteAssignUserAsync_InValidToken()
        {
            ///arrange
            long roleId = 1;
            long employeeId = 238;
            IOperationStatus operationStatus = new OperationStatus();
            ///act
            _roleService.Setup(x => x.DeleteAssignUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            ///assert
            var result = await _roleController.DeleteAssignUserAsync(roleId, employeeId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteAssignUserAsync_Error()
        {
            ///arrange
            long roleId = 0;
            long employeeId = 0;
            IOperationStatus operationStatus = new OperationStatus();
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.DeleteAssignUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            ///assert
            var result = await _roleController.DeleteAssignUserAsync(roleId, employeeId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteAssignUserAsync_DefaultRole()
        {
            ///arrange
            long roleId = 3;
            long employeeId = 238;
            IOperationStatus operationStatus = new OperationStatus();
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.DeleteAssignUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            ///assert
            var result = await _roleController.DeleteAssignUserAsync(roleId, employeeId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteAssignUserAsync_ValidToken()
        {
            ///arrange
            long roleId = 1;
            long employeeId = 238;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            ///act
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _roleService.Setup(x => x.DeleteAssignUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            ///assert
            var result = await _roleController.DeleteAssignUserAsync(roleId, employeeId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.Entity.Success);
        }

        #region Private region
        private void SetUserClaimsAndRequest()
        {
            _roleController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.Role, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108"),
                new Claim(ClaimTypes.Email, "abcd@gmail.com")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _roleController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };

            string sampleAuthToken = Guid.NewGuid().ToString();
            _roleController.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + sampleAuthToken;
        }
        #endregion
    }
}
