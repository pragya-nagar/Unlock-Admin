using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using OKRAdminService.WebCore.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace OKRAdminService.UnitTests.Controller
{
    public class UserControllerTestCases
    {
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IRoleService> _roleService;
        private readonly Mock<IIdentityService> _identityService;
        private readonly UserController _userController;
        private readonly Mock<IOrganisationService> _organisationService;
        private readonly Mock<IConfiguration> _configuration;
        protected readonly Mock<IDistributedCache> _distributedCache;
        public UserControllerTestCases()
        {
            _userService = new Mock<IUserService>();
            _roleService = new Mock<IRoleService>();
            _identityService = new Mock<IIdentityService>();
            _organisationService = new Mock<IOrganisationService>();
            _configuration = new Mock<IConfiguration>();
            _userController = new UserController(_identityService.Object, _roleService.Object, _organisationService.Object, _userService.Object, _configuration.Object);
            SetUserClaimsAndRequest();
        }
        ///  int tokenType = 1;

        [Fact]
        public async Task AddUserAsync_InValidToken()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { FirstName = "Pragya" };
            

            _userService.Setup(x => x.AddUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>(),It.IsAny<string>())).ReturnsAsync(userRequestModel);

            var result = await _userController.AddUserAsync(userRequestModel) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddUserAsync_ValidToken()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { FirstName = "Pragya", EmployeeCode = "234", Designation = "developer", EmailId = "pragya.nagar@gmail.com", LastName = "Nagar", OrganizationId = 24, ReportingTo = 331, RoleId = 3, EmployeeId = 21 };
            Employee employee = null;
            UserDetails userDetails = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(userRequestModel);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _userService.Setup(x => x.GetUserByEmployeeCodeAsync(It.IsAny<string>())).ReturnsAsync(userDetails);

            var result = await _userController.AddUserAsync(userRequestModel);
            PayloadCustom<UserRequestModel> requData = ((PayloadCustom<UserRequestModel>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AddUserAsync_Error()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { FirstName = "Pragya" };

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(userRequestModel);

            var result = await _userController.AddUserAsync(userRequestModel);
            PayloadCustom<UserRequestModel> requData = ((PayloadCustom<UserRequestModel>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddUserAsync_NotSuccess()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { FirstName = "Pragya", EmployeeCode = "234", Designation = "developer", EmailId = "pragya.nagar@gmail.com", LastName = "Nagar", OrganizationId = 24, ReportingTo = 331, RoleId = 3, EmployeeId = 21 };
            Employee employee = null;
            UserDetails userDetails = null;
            UserRequestModel userRequestModels = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(userRequestModels);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _userService.Setup(x => x.GetUserByEmployeeCodeAsync(It.IsAny<string>())).ReturnsAsync(userDetails);

            var result = await _userController.AddUserAsync(userRequestModel);
            PayloadCustom<UserRequestModel> requData = ((PayloadCustom<UserRequestModel>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task AddUserAsync_UserExists()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { FirstName = "Pragya", EmployeeCode = "234", Designation = "developer", EmailId = "pragya.nagar@gmail.com", LastName = "Nagar", OrganizationId = 24, ReportingTo = 331, RoleId = 3, EmployeeId = 21 };
            Employee employee = new Employee() { EmailId = "pragya.nagar@gmail.com" };
            UserDetails userDetails = new UserDetails() { EmployeeCode = "234" };

            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _userService.Setup(x => x.GetUserByEmployeeCodeAsync(It.IsAny<string>())).ReturnsAsync(userDetails);
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(userRequestModel);

            var result = await _userController.AddUserAsync(userRequestModel);
            PayloadCustom<UserRequestModel> requData = ((PayloadCustom<UserRequestModel>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task EditUserAsync_InValidToken()
        {
            UserRequestModel userRequestModel = new UserRequestModel();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.EditUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.EditUserAsync(userRequestModel) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task EditUserAsync_Error()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { EmployeeId = 23, EmployeeCode = "456", EmailId = "xxx@gmail.com" };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;
            Employee employees = new Employee() { EmployeeCode = "456", EmailId = "xxx@gmail.com" };

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.EditUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _userService.Setup(x => x.GetUserByEmployeeCodeAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(employees);
            _userService.Setup(x => x.GetUserByEmailIdAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(employees);

            var result = await _userController.EditUserAsync(userRequestModel);
            PayloadCustom<Employee> requData = ((PayloadCustom<Employee>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task EditUserAsync_ValidationError()
        {
            UserRequestModel userRequestModel = new UserRequestModel();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.EditUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.EditUserAsync(userRequestModel);
            PayloadCustom<Employee> requData = ((PayloadCustom<Employee>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task EditUserAsync_ValidToken()
        {
            UserRequestModel userRequestModel = new UserRequestModel() { EmployeeId = 23, FirstName = "xxx", LastName = "xxxx", RoleId = 3, Designation = "developer", ReportingTo = 331, OrganizationId = 24, EmployeeCode = "456", EmailId = "xxx@gmail.com" };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();
            Employee employees = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.EditUserAsync(It.IsAny<UserRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _userService.Setup(x => x.GetUserByEmployeeCodeAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(employees);
            _userService.Setup(x => x.GetUserByEmailIdAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(employees);

            var result = await _userController.EditUserAsync(userRequestModel);
            PayloadCustom<Employee> requData = ((PayloadCustom<Employee>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task MultiSearchUserListAsync_InValidToken()
        {
            List<string> list = new List<string>();
            var pageIndex = 0;
            var pageSize = 10;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>();
            _userService.Setup(x => x.MultiSearchUserListAsync("JwtToken", list, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.MultiSearchUserListAsync(list, pageIndex, pageSize) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);

        }

        [Fact]
        public async Task MultiSearchUserListAsync_Error()
        {
            List<string> list = new List<string>();
            var pageIndex = 0;
            var pageSize = 0;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.MultiSearchUserListAsync("JwtToken", list, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.MultiSearchUserListAsync(list, pageIndex, pageSize);
            PayloadCustom<PageResults<AllUsersResponse>> requData = ((PayloadCustom<PageResults<AllUsersResponse>>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task MultiSearchUserListAsync_ValidToken()
        {
            List<string> list = new List<string>();
            var pageIndex = 0;
            var pageSize = 10;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>() { TotalRecords = 1, PageIndex = 0, PageSize = 10, TotalPages = 1, Records = new List<AllUsersResponse>() { new AllUsersResponse() { FirstName = "xxx", RoleId = 3, EmployeeCode = "23" } } };

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.MultiSearchUserListAsync("JwtToken", It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.MultiSearchUserListAsync(list, pageIndex, pageSize);
            PayloadCustom<PageResults<AllUsersResponse>> requData = ((PayloadCustom<PageResults<AllUsersResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetUserByIdAsync_InValidToken()
        {
            long employeeId = 238;
            UserDetails userDetails = new UserDetails();
            _userService.Setup(x => x.GetUserByEmpIdAsync(It.IsAny<long>())).ReturnsAsync(userDetails);

            var result = await _userController.GetUserByIdAsync(employeeId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);

        }

        [Fact]
        public async Task GetUserByIdAsync_ValidToken()
        {
            long employeeId = 238;
            UserDetails userDetails = new UserDetails();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetUserByEmpIdAsync(It.IsAny<long>())).ReturnsAsync(userDetails);

            var result = await _userController.GetUserByIdAsync(employeeId);
            PayloadCustom<UserDetails> requData = ((PayloadCustom<UserDetails>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetUserByIdAsync_NotSuccess()
        {
            long employeeId = 238;
            UserDetails userDetails = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetUserByEmpIdAsync(It.IsAny<long>())).ReturnsAsync(userDetails);

            var result = await _userController.GetUserByIdAsync(employeeId);
            PayloadCustom<UserDetails> requData = ((PayloadCustom<UserDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task LoginAsync_Error()
        {
            LoginRequest loginRequest = new LoginRequest();
            UserLoginResponse userLoginResponse = new UserLoginResponse();

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_UserNotExists()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse();
            Employee employee = null;

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_RoleNotAssigned()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse();
            Employee employee = new Employee() { EmployeeId = 3 };
            UserRoleDetail userRoleDetail = null;

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_AccountLockError()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse();
            Employee employee = new Employee() { EmployeeId = 3, LoginFailCount = 5 };
            UserRoleDetail userRoleDetail = new UserRoleDetail();

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_NotSuccess()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse() { LoginFailCount = 4 };
            Employee employee = new Employee() { EmployeeId = 3 };
            UserRoleDetail userRoleDetail = new UserRoleDetail();

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_InValidUsername()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse();
            Employee employee = new Employee() { EmployeeId = 3 };
            UserRoleDetail userRoleDetail = new UserRoleDetail();

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_MaxLoginCount()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse() { LoginFailCount = 5 };
            Employee employee = new Employee() { EmployeeId = 3 };
            UserRoleDetail userRoleDetail = new UserRoleDetail();

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task LoginAsync_Success()
        {
            LoginRequest loginRequest = new LoginRequest() { UserName = "xx@gmail.com", Password = "Abcd@1234" };
            UserLoginResponse userLoginResponse = new UserLoginResponse() { LoginFailCount = 0, TokenId = "jwtToken" };
            Employee employee = new Employee() { EmployeeId = 3 };
            UserRoleDetail userRoleDetail = new UserRoleDetail();

            _userService.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>())).ReturnsAsync(userLoginResponse);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);
            _roleService.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<long>())).ReturnsAsync(userRoleDetail);

            var result = await _userController.LoginAsync(loginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public void LogOut_InValidToken()
        {
            _userService.Setup(x => x.Logout(It.IsAny<string>(), It.IsAny<long>()));

            var result = _userController.LogOut() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public void LogOut_ValidToken()
        {
            _userService.Setup(x => x.Logout(It.IsAny<string>(), It.IsAny<long>()));
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);

            var result = _userController.LogOut();
            PayloadCustom<TokenResponse> requData = ((PayloadCustom<TokenResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllUsersAsync_InValidToken()
        {
            var pageIndex = 0;
            var pageSize = 10;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>();

            _userService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.GetAllUsersAsync(pageIndex, pageSize) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllUsersAsync_Error()
        {
            var pageIndex = 0;
            var pageSize = 0;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.GetAllUsersAsync(pageIndex, pageSize);
            PayloadCustomList<PageResults<AllUsersResponse>> requData = ((PayloadCustomList<PageResults<AllUsersResponse>>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllUsersAsync_ValidToken()
        {
            var pageIndex = 0;
            var pageSize = 10;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>() { Records = new List<AllUsersResponse>() { new AllUsersResponse() { FirstName = "xxx", EmployeeId = 23 } } };

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.GetAllUsersAsync(pageIndex, pageSize);
            PayloadCustomList<PageResults<AllUsersResponse>> requData = ((PayloadCustomList<PageResults<AllUsersResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllUsersAsync_EmployeeNotExists()
        {
            var pageIndex = 0;
            var pageSize = 10;
            PageResults<AllUsersResponse> pageResults = new PageResults<AllUsersResponse>();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pageResults);

            var result = await _userController.GetAllUsersAsync(pageIndex, pageSize);
            PayloadCustomList<PageResults<AllUsersResponse>> requData = ((PayloadCustomList<PageResults<AllUsersResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public void SearchEmployee_InValidToken()
        {
            string finder = "name";
            PageResult<SearchUserList> searchUserList = new PageResult<SearchUserList>();
            var pageIndex = 0;
            var pageSize = 10;
            _userService.Setup(x => x.SearchEmployee(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).ReturnsAsync(searchUserList);

            var result = _userController.SearchEmployee(finder, pageIndex, pageSize).Result as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public void SearchEmployee_EmployeeNotExists()
        {
            string finder = "name";
            PageResult<SearchUserList> list = new PageResult<SearchUserList>() { Records = new List<SearchUserList>() };
            var pageIndex = 0;
            var pageSize = 10;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.SearchEmployee(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).ReturnsAsync(list);

            var result = _userController.SearchEmployee(finder, pageIndex, pageSize).Result;
            PayloadCustomGenric<SearchUserList> requData = (PayloadCustomGenric<SearchUserList>)((ObjectResult)result).Value;
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public void SearchEmployee_ValidToken()
        {
            string finder = "name";
            PageResult<SearchUserList> list = new PageResult<SearchUserList>() { Records = new List<SearchUserList>() { new SearchUserList() { FirstName = "xxx" } } };
            var pageIndex = 0;
            var pageSize = 10;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.SearchEmployee(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).ReturnsAsync(list);

            var result = _userController.SearchEmployee(finder, pageIndex, pageSize).Result;
            PayloadCustomGenric<SearchUserList> requData = ((PayloadCustomGenric<SearchUserList>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task Identity_InValidToken()
        {
            LoginUserDetails loginUserDetails = new LoginUserDetails();

            //_userService.Setup(x => x.Identity(It.IsAny<string>())).ReturnsAsync(loginUserDetails);

            var result = await _userController.Identity() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Identity_ValidToken()
        {
            LoginUserDetails loginUserDetails = new LoginUserDetails();

            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
           _userService.Setup(x => x.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);

            var result = await _userController.Identity();
            PayloadCustom<LoginUserDetails> requData = ((PayloadCustom<LoginUserDetails>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task Identity_EmployeeNotExists()
        {
            LoginUserDetails loginUserDetails = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);

            var result = await _userController.Identity();
            PayloadCustom<LoginUserDetails> requData = ((PayloadCustom<LoginUserDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task DeleteUserAsync_InValidToken()
        {
            List<long> list = new List<long>();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.DeleteUserAsync(list, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.DeleteUserAsync(list) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteUserAsync_Error()
        {
            List<long> list = new List<long>();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteUserAsync(list, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.DeleteUserAsync(list);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task DeleteUserAsync_LoggedInUserError()
        {
            List<long> list = new List<long>() { 108 };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteUserAsync(list, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetReportingToOrganisationHeadAsync(list)).ReturnsAsync(employee);

            var result = await _userController.DeleteUserAsync(list);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task DeleteUserAsync_OrganisationHeadOrReportingUsersError()
        {
            List<long> list = new List<long>() { 106 };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee() { FirstName = "xxx", LastName = "xxxx", EmployeeCode = "144" };

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteUserAsync(list, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetReportingToOrganisationHeadAsync(list)).ReturnsAsync(employee);

            var result = await _userController.DeleteUserAsync(list);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task DeleteUserAsync_NotSuccess()
        {
            List<long> list = new List<long>() { 106 };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteUserAsync(list, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetReportingToOrganisationHeadAsync(list)).ReturnsAsync(employee);

            var result = await _userController.DeleteUserAsync(list);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteUserAsync_ValidToken()
        {
            List<long> list = new List<long>() { 106 };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteUserAsync(list, It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetReportingToOrganisationHeadAsync(list)).ReturnsAsync(employee);

            var result = await _userController.DeleteUserAsync(list);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadBulkUserAsync_InValidToken()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.xlsx");

            _userService.Setup(x => x.UploadBulkUserAsync(It.IsAny<IFormFile>(), It.IsAny<long>(), It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.UploadBulkUserAsync(file) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UploadBulkUserAsync_Error()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.xlsx");

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadBulkUserAsync(It.IsAny<IFormFile>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.UploadBulkUserAsync(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadBulkUserAsync_VaidToken()
        {
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.xlsx");

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadBulkUserAsync(It.IsAny<IFormFile>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.UploadBulkUserAsync(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadBulkUserAsync_NotSuccess()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.xlsx");

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadBulkUserAsync(It.IsAny<IFormFile>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.UploadBulkUserAsync(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task UploadBulkUserAsync_InvalidFile()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.txt");

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadBulkUserAsync(It.IsAny<IFormFile>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.UploadBulkUserAsync(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task DownloadCSVAsync_InValidToken()
        {
            _userService.Setup(x => x.DownloadCsvAsync()).ReturnsAsync("dummmy");

            var result = await _userController.DownloadCsvAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ChangeRoleAsync_InValidToken()
        {
            ChangeRoleRequestModel changeRoleRequestModel = new ChangeRoleRequestModel();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.ChangeRoleAsync(It.IsAny<ChangeRoleRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeRoleAsync(changeRoleRequestModel) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ChangeRoleAsync_Error()
        {
            ChangeRoleRequestModel changeRoleRequestModel = null;
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeRoleAsync(It.IsAny<ChangeRoleRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeRoleAsync(changeRoleRequestModel);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeRoleAsync_InValidRole()
        {
            ChangeRoleRequestModel changeRoleRequestModel = new ChangeRoleRequestModel();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeRoleAsync(It.IsAny<ChangeRoleRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeRoleAsync(changeRoleRequestModel);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeRoleAsync_ValidToken()
        {
            ChangeRoleRequestModel changeRoleRequestModel = new ChangeRoleRequestModel() { EmployeeIds = new List<long>() { 106 }, NewRoleId = 3 };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeRoleAsync(It.IsAny<ChangeRoleRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeRoleAsync(changeRoleRequestModel);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangeRoleAsync_NotSuccess()
        {
            ChangeRoleRequestModel changeRoleRequestModel = new ChangeRoleRequestModel() { EmployeeIds = new List<long>() { 106 }, NewRoleId = 3 };
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeRoleAsync(It.IsAny<ChangeRoleRequestModel>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeRoleAsync(changeRoleRequestModel);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangeUserReporting_InValidToken()
        {
            EditUserReportingRequest editUserReportingRequest = new EditUserReportingRequest();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.ChangeUserReportingAsync(It.IsAny<EditUserReportingRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeUserReporting(editUserReportingRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ChangeUserReporting_Error()
        {
            EditUserReportingRequest editUserReportingRequest = null;
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeUserReportingAsync(It.IsAny<EditUserReportingRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeUserReporting(editUserReportingRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeUserReporting_InValidReporting()
        {
            EditUserReportingRequest editUserReportingRequest = new EditUserReportingRequest();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeUserReportingAsync(It.IsAny<EditUserReportingRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeUserReporting(editUserReportingRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeUserReporting_ReportingNotExists()
        {
            EditUserReportingRequest editUserReportingRequest = new EditUserReportingRequest() { NewReportingToId = 3 };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeUserReportingAsync(It.IsAny<EditUserReportingRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ChangeUserReporting(editUserReportingRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeUserReporting_ValidToken()
        {
            EditUserReportingRequest editUserReportingRequest = new EditUserReportingRequest() { EmployeeIds = new List<long>() { 106 }, NewReportingToId = 3 };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeUserReportingAsync(It.IsAny<EditUserReportingRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ChangeUserReporting(editUserReportingRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangeUserReporting_NotSuccess()
        {
            EditUserReportingRequest editUserReportingRequest = new EditUserReportingRequest() { EmployeeIds = new List<long>() { 106 }, NewReportingToId = 3 };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangeUserReportingAsync(It.IsAny<EditUserReportingRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ChangeUserReporting(editUserReportingRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangeUserOrganisation_InValidToken()
        {
            ChangeUserOrganisationRequest changeUserOrganisationRequest = new ChangeUserOrganisationRequest();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.ChangeUserOrganisationAsync(It.IsAny<ChangeUserOrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeUserOrganisation(changeUserOrganisationRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ChangeUserOrganisation_Error()
        {
            ChangeUserOrganisationRequest changeUserOrganisationRequest = null;
            IOperationStatus operationStatus = new OperationStatus();
            LoginUserDetails loginUserDetails = new LoginUserDetails();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(p => p.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);
            _userService.Setup(x => x.ChangeUserOrganisationAsync(It.IsAny<ChangeUserOrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeUserOrganisation(changeUserOrganisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeUserOrganisation_InValidOrganisation()
        {
            ChangeUserOrganisationRequest changeUserOrganisationRequest = new ChangeUserOrganisationRequest();
            IOperationStatus operationStatus = new OperationStatus();
            LoginUserDetails loginUserDetails = new LoginUserDetails();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(p => p.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);
            _userService.Setup(x => x.ChangeUserOrganisationAsync(It.IsAny<ChangeUserOrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangeUserOrganisation(changeUserOrganisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeUserOrganisation_OrganisationNotExists()
        {
            ChangeUserOrganisationRequest changeUserOrganisationRequest = new ChangeUserOrganisationRequest() { NewOrganisationId = 24 };
            IOperationStatus operationStatus = new OperationStatus();
            LoginUserDetails loginUserDetails = new LoginUserDetails();
            Organisation organisation = null;

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(p => p.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);
            _userService.Setup(x => x.ChangeUserOrganisationAsync(It.IsAny<ChangeUserOrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(p => p.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);

            var result = await _userController.ChangeUserOrganisation(changeUserOrganisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangeUserOrganisation_ValidToken()
        {
            ChangeUserOrganisationRequest changeUserOrganisationRequest = new ChangeUserOrganisationRequest() { NewOrganisationId = 24, EmployeeIds = new List<long>() { 106 } };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            LoginUserDetails loginUserDetails = new LoginUserDetails();
            Organisation organisation = new Organisation();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(p => p.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);
            _userService.Setup(x => x.ChangeUserOrganisationAsync(It.IsAny<ChangeUserOrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(p => p.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);

            var result = await _userController.ChangeUserOrganisation(changeUserOrganisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangeUserOrganisation_NotSuccess()
        {
            ChangeUserOrganisationRequest changeUserOrganisationRequest = new ChangeUserOrganisationRequest() { NewOrganisationId = 24, EmployeeIds = new List<long>() { 106 } };
            IOperationStatus operationStatus = new OperationStatus() { Success = false };
            LoginUserDetails loginUserDetails = new LoginUserDetails();
            Organisation organisation = new Organisation();

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(p => p.Identity(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(loginUserDetails);
            _userService.Setup(x => x.ChangeUserOrganisationAsync(It.IsAny<ChangeUserOrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(p => p.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);

            var result = await _userController.ChangeUserOrganisation(changeUserOrganisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetDesignationAsync_InValidToken()
        {
            List<string> designationList = new List<string>() { "Software Engineer", "Senior Software Engineer" };
            string designation = "Software";

            _userService.Setup(x => x.GetDesignationAsync(It.IsAny<string>())).ReturnsAsync(designationList);

            var result = await _userController.GetDesignationAsync(designation) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetDesignationAsync_ValidToken()
        {
            List<string> designationList = new List<string>() { "Software Engineer", "Senior Software Engineer" };
            string designation = "Software";

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetDesignationAsync(It.IsAny<string>())).ReturnsAsync(designationList);

            var result = await _userController.GetDesignationAsync(designation);
            PayloadCustom<string> requData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            var finalData = requData.EntityList;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task GetDesignationAsync_DesignationNotExists()
        {
            List<string> designationList = new List<string>();
            string designation = "Software";

            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetDesignationAsync(It.IsAny<string>())).ReturnsAsync(designationList);

            var result = await _userController.GetDesignationAsync(designation);
            PayloadCustom<string> requData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task ResetPasswordAsync_InValidToken()
        {
            ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.ResetPasswordAsync(It.IsAny<long>(), It.IsAny<ResetPasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ResetPasswordAsync(resetPasswordRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidToken()
        {
            ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest() { NewPassword = "Abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ResetPasswordAsync(It.IsAny<long>(), It.IsAny<ResetPasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ResetPasswordAsync(resetPasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ResetPasswordAsync_NewPasswordNotExists()
        {
            ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ResetPasswordAsync(It.IsAny<long>(), It.IsAny<ResetPasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ResetPasswordAsync(resetPasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ResetPasswordAsync_NotSuccess()
        {
            ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest() { NewPassword = "Abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ResetPasswordAsync(It.IsAny<long>(), It.IsAny<ResetPasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ResetPasswordAsync(resetPasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ResetPasswordAsync_InValidNewPassword()
        {
            ResetPasswordRequest resetPasswordRequest = new ResetPasswordRequest() { NewPassword = "abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ResetPasswordAsync(It.IsAny<long>(), It.IsAny<ResetPasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ResetPasswordAsync(resetPasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ResetPasswordAsync_Error()
        {
            ResetPasswordRequest resetPasswordRequest = null;
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ResetPasswordAsync(It.IsAny<long>(), It.IsAny<ResetPasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ResetPasswordAsync(resetPasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task SendResetPasswordMailAsync_Error()
        {
            SendResetPasswordMailRequest sendResetPasswordMailRequest = null;

            _userService.Setup(x => x.SendResetPasswordMailAsync(It.IsAny<SendResetPasswordMailRequest>())).ReturnsAsync(true);

            var result = await _userController.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task SendResetPasswordMailAsync_EmailNotExists()
        {
            SendResetPasswordMailRequest sendResetPasswordMailRequest = new SendResetPasswordMailRequest();

            _userService.Setup(x => x.SendResetPasswordMailAsync(It.IsAny<SendResetPasswordMailRequest>())).ReturnsAsync(true);

            var result = await _userController.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task SendResetPasswordMailAsync_InValidEmail()
        {
            SendResetPasswordMailRequest sendResetPasswordMailRequest = new SendResetPasswordMailRequest() { EmailId = "xxx" };
            Employee employee = null;

            _userService.Setup(x => x.SendResetPasswordMailAsync(It.IsAny<SendResetPasswordMailRequest>())).ReturnsAsync(true);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);

            var result = await _userController.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task SendResetPasswordMailAsync_EmailExists()
        {
            SendResetPasswordMailRequest sendResetPasswordMailRequest = new SendResetPasswordMailRequest() { EmailId = "xxx@gmail.com" };
            Employee employee = null;

            _userService.Setup(x => x.SendResetPasswordMailAsync(It.IsAny<SendResetPasswordMailRequest>())).ReturnsAsync(true);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);

            var result = await _userController.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task SendResetPasswordMailAsync_Success()
        {
            SendResetPasswordMailRequest sendResetPasswordMailRequest = new SendResetPasswordMailRequest() { EmailId = "xxx@gmail.com" };
            Employee employee = new Employee();

            _userService.Setup(x => x.SendResetPasswordMailAsync(It.IsAny<SendResetPasswordMailRequest>())).ReturnsAsync(true);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);

            var result = await _userController.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task SendResetPasswordMailAsync_NotSuccess()
        {
            SendResetPasswordMailRequest sendResetPasswordMailRequest = new SendResetPasswordMailRequest() { EmailId = "xxx@gmail.com" };
            Employee employee = new Employee();

            _userService.Setup(x => x.SendResetPasswordMailAsync(It.IsAny<SendResetPasswordMailRequest>())).ReturnsAsync(false);
            _userService.Setup(x => x.GetUserByMailIdAsync(It.IsAny<string>())).ReturnsAsync(employee);

            var result = await _userController.SendResetPasswordMailAsync(sendResetPasswordMailRequest);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddUpdateUserContactDetailAsync_InValidToken()
        {
            UserContactDetail userContactDetail = new UserContactDetail();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.AddUpdateUserContactAsync(It.IsAny<UserContactDetail>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.AddUpdateUserContactDetailAsync(userContactDetail) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddUpdateUserContactDetailAsync_UserContactNotExists()
        {
            UserContactDetail userContactDetail = null;
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUpdateUserContactAsync(It.IsAny<UserContactDetail>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            var result = await _userController.AddUpdateUserContactDetailAsync(userContactDetail);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AddUpdateUserContactDetailAsync_Error()
        {
            UserContactDetail userContactDetail = new UserContactDetail() { PhoneNumber = "243543", CountryStdCode = "343123423553", DeskPhoneNumber = "625271", LinkedInUrl = "www.google.com", SkypeUrl = "xxx", TwitterUrl = "www.google.com" };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUpdateUserContactAsync(It.IsAny<UserContactDetail>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.AddUpdateUserContactDetailAsync(userContactDetail);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task AddUpdateUserContactDetailAsync_ValidToken()
        {
            UserContactDetail userContactDetail = new UserContactDetail();
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUpdateUserContactAsync(It.IsAny<UserContactDetail>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.AddUpdateUserContactDetailAsync(userContactDetail);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AddUpdateUserContactDetailAsync_NotSuccess()
        {
            UserContactDetail userContactDetail = new UserContactDetail();
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.AddUpdateUserContactAsync(It.IsAny<UserContactDetail>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.AddUpdateUserContactDetailAsync(userContactDetail);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetEmployeeProfileByEmployeeIdAsync_InValidToken()
        {
            EmployeeProfileResponse employeeProfileResponse = new EmployeeProfileResponse();

            _userService.Setup(x => x.GetEmployeeProfileByEmployeeIdAsync(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(employeeProfileResponse);

            var result = await _userController.GetEmployeeProfileByEmployeeIdAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeProfileByEmployeeIdAsync_Error()
        {
            EmployeeProfileResponse employeeProfileResponse = new EmployeeProfileResponse();
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetEmployeeProfileByEmployeeIdAsync(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(employeeProfileResponse);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.GetEmployeeProfileByEmployeeIdAsync();
            PayloadCustom<EmployeeProfileResponse> requData = ((PayloadCustom<EmployeeProfileResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetEmployeeProfileByEmployeeIdAsync_ValidToken()
        {
            EmployeeProfileResponse employeeProfileResponse = new EmployeeProfileResponse();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetEmployeeProfileByEmployeeIdAsync(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(employeeProfileResponse);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.GetEmployeeProfileByEmployeeIdAsync();
            PayloadCustom<EmployeeProfileResponse> requData = ((PayloadCustom<EmployeeProfileResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetEmployeeProfileByEmployeeIdAsync_NotSuccess()
        {
            EmployeeProfileResponse employeeProfileResponse = null;
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetEmployeeProfileByEmployeeIdAsync(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(employeeProfileResponse);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.GetEmployeeProfileByEmployeeIdAsync();
            PayloadCustom<EmployeeProfileResponse> requData = ((PayloadCustom<EmployeeProfileResponse>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadProfileImage_InValidToken()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.jpg");

            _userService.Setup(x => x.UploadProfileImageAsync(It.IsAny<IFormFile>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.UploadProfileImage(file) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UploadProfileImage_Error()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.jpg");
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadProfileImageAsync(It.IsAny<IFormFile>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.UploadProfileImage(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task UploadProfileImage_ValidToken()
        {
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.jpg");
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadProfileImageAsync(It.IsAny<IFormFile>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.UploadProfileImage(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadProfileImage_NotSuccess()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.jpg");
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadProfileImageAsync(It.IsAny<IFormFile>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.UploadProfileImage(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadProfileImage_InValidFile()
        {
            IOperationStatus operationStatus = new OperationStatus();
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.csv");
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.UploadProfileImageAsync(It.IsAny<IFormFile>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.UploadProfileImage(file);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteProfileImage_InValidToken()
        {
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.DeleteProfileImageAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);

            var result = await _userController.DeleteProfileImage() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteProfileImage_Error()
        {
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteProfileImageAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.DeleteProfileImage();
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task DeleteProfileImage_ValidToken()
        {
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteProfileImageAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.DeleteProfileImage();
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteProfileImage_NotSuccess()
        {
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.DeleteProfileImageAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.DeleteProfileImage();
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangePasswordAsync_InValidToken()
        {
            ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest();
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangePasswordAsync(changePasswordRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task ChangePasswordAsync_Error()
        {
            ChangePasswordRequest changePasswordRequest = null;
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangePasswordAsync(changePasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangePasswordAsync_InValidNewPassword()
        {
            ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest() { OldPassword = "abcd@1234", NewPassword = "abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);

            var result = await _userController.ChangePasswordAsync(changePasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangePasswordAsync_EmployeeNotExists()
        {
            ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest() { OldPassword = "abcd@1234", NewPassword = "Abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ChangePasswordAsync(changePasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangePasswordAsync_InValidOldPassword()
        {
            ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest() { OldPassword = "abcd@1234", NewPassword = "Abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ChangePasswordAsync(changePasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ChangePasswordAsync_NotSuccess()
        {
            ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest() { OldPassword = "abcd@1234", NewPassword = "Abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ChangePasswordAsync(changePasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidToken()
        {
            ChangePasswordRequest changePasswordRequest = new ChangePasswordRequest() { OldPassword = "abcd@1234", NewPassword = "Abcd@1234" };
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ChangePasswordAsync(It.IsAny<long>(), It.IsAny<ChangePasswordRequest>())).ReturnsAsync(operationStatus);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            //_userService.Setup(x => x.DecryptRijndael(It.IsAny<string>(), It.IsAny<string>())).Returns("abcd@1234");

            var result = await _userController.ChangePasswordAsync(changePasswordRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ReSendResetPasswordMailAsync_Error()
        {
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ReSendResetPasswordMailAsync(It.IsAny<long>())).ReturnsAsync(true);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ReSendResetPasswordMailAsync();
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ReSendResetPasswordMailAsync_NotSuccess()
        {
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ReSendResetPasswordMailAsync(It.IsAny<long>())).ReturnsAsync(false);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ReSendResetPasswordMailAsync();
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task ReSendResetPasswordMailAsync_Success()
        {
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.ReSendResetPasswordMailAsync(It.IsAny<long>())).ReturnsAsync(true);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.ReSendResetPasswordMailAsync();
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task RefreshToken_InValidToken()
        {
            RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse();

            _userService.Setup(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(refreshTokenResponse);

            var result = await _userController.RefreshToken() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_Error()
        {
            RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse();
            Employee employee = null;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(refreshTokenResponse);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.RefreshToken();
            PayloadCustom<RefreshTokenResponse> requData = ((PayloadCustom<RefreshTokenResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task RefreshToken_NotSuccess()
        {
            RefreshTokenResponse refreshTokenResponse = null;
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(refreshTokenResponse);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.RefreshToken();
            PayloadCustom<RefreshTokenResponse> requData = ((PayloadCustom<RefreshTokenResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task RefreshToken_ValidToken()
        {
            RefreshTokenResponse refreshTokenResponse = new RefreshTokenResponse();
            Employee employee = new Employee();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(refreshTokenResponse);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);

            var result = await _userController.RefreshToken();
            PayloadCustom<RefreshTokenResponse> requData = ((PayloadCustom<RefreshTokenResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetGoalLockDateAsync_InValidToken()
        {
            long organisationCycleId = 1;
            List<GoalUnlockDate> goalUnlockDates = new List<GoalUnlockDate>();

            _userService.Setup(x => x.GetGoalLockDateAsync(It.IsAny<long>())).ReturnsAsync(goalUnlockDates);

            var result = await _userController.GetGoalLockDateAsync(organisationCycleId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetGoalLockDateAsync_Error()
        {
            long organisationCycleId = 0;
            List<GoalUnlockDate> goalUnlockDates = new List<GoalUnlockDate>();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetGoalLockDateAsync(It.IsAny<long>())).ReturnsAsync(goalUnlockDates);

            var result = await _userController.GetGoalLockDateAsync(organisationCycleId);
            PayloadCustom<GoalUnlockDate> requData = ((PayloadCustom<GoalUnlockDate>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetGoalLockDateAsync_NotSuccess()
        {
            long organisationCycleId = 1;
            List<GoalUnlockDate> goalUnlockDates = new List<GoalUnlockDate>();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetGoalLockDateAsync(It.IsAny<long>())).ReturnsAsync(goalUnlockDates);

            var result = await _userController.GetGoalLockDateAsync(organisationCycleId);
            PayloadCustom<GoalUnlockDate> requData = ((PayloadCustom<GoalUnlockDate>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetGoalLockDateAsync_ValidToken()
        {
            long organisationCycleId = 1;
            List<GoalUnlockDate> goalUnlockDates = new List<GoalUnlockDate>() { new GoalUnlockDate() { OrganisationCycleId = 1 } };

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetGoalLockDateAsync(It.IsAny<long>())).ReturnsAsync(goalUnlockDates);

            var result = await _userController.GetGoalLockDateAsync(organisationCycleId);
            PayloadCustom<GoalUnlockDate> requData = ((PayloadCustom<GoalUnlockDate>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public void GlobalSearch_InValidToken()
        {
            string finder = "name";
            PageResult<GlobalSearchList> searchUserList = new PageResult<GlobalSearchList>();
            var pageIndex = 0;
            var pageSize = 10;
            int searchType = 0;

            _userService.Setup(x => x.GlobalSearch(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).Returns(searchUserList);

            var result = _userController.GlobalSearch(finder, searchType, pageIndex, pageSize) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public void GlobalSearch_EmployeeNotExists()
        {
            string finder = "name";
            PageResult<GlobalSearchList> list = new PageResult<GlobalSearchList>() { Records = new List<GlobalSearchList>() };
            var pageIndex = 0;
            var pageSize = 10;
            int searchType = 0;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GlobalSearch(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).Returns(list);

            var result = _userController.GlobalSearch(finder, searchType, pageIndex, pageSize);
            PayloadCustomGenric<GlobalSearchList> requData = ((PayloadCustomGenric<GlobalSearchList>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public void GlobalSearch_ValidToken()
        {
            string finder = "name";
            PageResult<GlobalSearchList> list = new PageResult<GlobalSearchList>() { Records = new List<GlobalSearchList>() { new GlobalSearchList() { FirstName = "xxx" } } };
            var pageIndex = 0;
            var pageSize = 10;
            int searchType = 0;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GlobalSearch(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).Returns(list);

            var result = _userController.GlobalSearch(finder, searchType, pageIndex, pageSize);
            PayloadCustomGenric<GlobalSearchList> requData = ((PayloadCustomGenric<GlobalSearchList>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public void GlobalSearch_SearchTypeInvalid()
        {
            string finder = "name";
            PageResult<GlobalSearchList> list = new PageResult<GlobalSearchList>() { Records = new List<GlobalSearchList>() { new GlobalSearchList() { FirstName = "xxx" } } };
            var pageIndex = 0;
            var pageSize = 10;
            int searchType = 2;

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GlobalSearch(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).Returns(list);

            var result = _userController.GlobalSearch(finder, searchType, pageIndex, pageSize);
            PayloadCustomGenric<GlobalSearchList> requData = ((PayloadCustomGenric<GlobalSearchList>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        #region Private region

        private void SetUserClaimsAndRequest()
        {
            _userController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.Role, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108"),
                new Claim(ClaimTypes.Email, "abcd@gmail.com")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _userController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };

            string sampleAuthToken = Guid.NewGuid().ToString();
            _userController.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + sampleAuthToken;
        }
        #endregion
    }
}
