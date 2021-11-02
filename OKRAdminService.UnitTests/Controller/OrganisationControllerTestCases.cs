using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using Microsoft.Extensions.Configuration;
using Xunit;



namespace OKRAdminService.UnitTests.Controller
{
    public class OrganisationControllerTestCases
    {
        private readonly Mock<IOrganisationService> _organisationService;
        private readonly Mock<IUserService> _userService;
        private readonly Mock<IIdentityService> _identityService;
        private readonly Mock<IRoleService> _roleService;
        private readonly Mock<IPermissionService> _permissionService;
        private readonly OrganisationController _organisationController;
        public OrganisationControllerTestCases()
        {
            _organisationService = new Mock<IOrganisationService>();
            _userService = new Mock<IUserService>();
            _identityService = new Mock<IIdentityService>();
            _roleService = new Mock<IRoleService>();
            _permissionService = new Mock<IPermissionService>();
            _organisationController = new OrganisationController(_identityService.Object, _organisationService.Object, _userService.Object, _roleService.Object, _permissionService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task CreateOrganisationsAsync_InvalidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();
            Organisation organisation = new Organisation();
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationName = "CompunnelDigital" };

            ///act
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.CreateOrganisationsAsync(organisationRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task CreateOrganisationsAsync_ValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();
            Organisation organisation = new Organisation();
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationName = "CompunnelDigital" };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.CreateOrganisationsAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task CreateOrganisationsAsync_Error()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;
            Organisation organisation = new Organisation();
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.CreateOrganisationsAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task CreateOrganisationsAsync_Success()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();
            Organisation organisation = null;
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2, OrganisationName = "CompunnelDigital", CycleDuration = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(x => x.DoesCycleFallsInFutureDate(It.IsAny<OrganisationRequest>())).ReturnsAsync(true);

            ///assert
            var result = await _organisationController.CreateOrganisationsAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task CreateOrganisationsAsync_NotSuccess()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();
            Organisation organisation = null;
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2, OrganisationName = "CompunnelDigital", CycleDuration = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.CreateOrganisationsAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetOrganisationByIdAsync_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            OrganisationDetail organisationDetail = new OrganisationDetail() { CycleDuration = "HalfYearly" };
            Organisation organisation = new Organisation();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByIdAsync(It.IsAny<long>())).ReturnsAsync(organisationDetail);

            ///assert
            var result = await _organisationController.GetOrganisationByIdAsync(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetOrganisationByIdAsync_Success()
        {
            ///arrange
            long organisationId = 2;
            OrganisationDetail organisationDetail = new OrganisationDetail() { CycleDuration = "HalfYearly" };
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByIdAsync(It.IsAny<long>())).ReturnsAsync(organisationDetail);

            ///assert
            var result = await _organisationController.GetOrganisationByIdAsync(organisationId);
            PayloadCustom<OrganisationDetail> requData = ((PayloadCustom<OrganisationDetail>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetOrganisationByIdAsync_Error()
        {
            ///arrange
            long organisationId = 0;
            OrganisationDetail organisationDetail = new OrganisationDetail() { CycleDuration = "HalfYearly" };
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByIdAsync(It.IsAny<long>())).ReturnsAsync(organisationDetail);

            ///assert
            var result = await _organisationController.GetOrganisationByIdAsync(organisationId);
            PayloadCustom<OrganisationDetail> requData = ((PayloadCustom<OrganisationDetail>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetOrganisationByIdAsync_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            OrganisationDetail organisationDetail = new OrganisationDetail() { CycleDuration = "HalfYearly" };
            Organisation organisation = null;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByIdAsync(It.IsAny<long>())).ReturnsAsync(organisationDetail);

            ///assert
            var result = await _organisationController.GetOrganisationByIdAsync(organisationId);
            PayloadCustom<OrganisationDetail> requData = ((PayloadCustom<OrganisationDetail>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetOrganisationByIdAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            OrganisationDetail organisationDetail = null;
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByIdAsync(It.IsAny<long>())).ReturnsAsync(organisationDetail);

            ///assert
            var result = await _organisationController.GetOrganisationByIdAsync(organisationId);
            PayloadCustom<OrganisationDetail> requData = ((PayloadCustom<OrganisationDetail>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetOrganisationObjectives_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            OrganisationObjectives organisationObjectives = new OrganisationObjectives();

            ///act
            _organisationService.Setup(x => x.GetObjectivesByOrgIdAsync(It.IsAny<long>())).ReturnsAsync(organisationObjectives);

            ///assert
            var result = await _organisationController.GetOrganisationObjectives(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetOrganisationObjectives_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            OrganisationObjectives organisationObjectives = new OrganisationObjectives();

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetObjectivesByOrgIdAsync(It.IsAny<long>())).ReturnsAsync(organisationObjectives);

            ///assert
            var result = await _organisationController.GetOrganisationObjectives(organisationId);
            PayloadCustom<OrganisationObjectives> requData = ((PayloadCustom<OrganisationObjectives>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetOrganisationObjectives_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            OrganisationObjectives organisationObjectives = null;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetObjectivesByOrgIdAsync(It.IsAny<long>())).ReturnsAsync(organisationObjectives);

            ///assert
            var result = await _organisationController.GetOrganisationObjectives(organisationId);
            PayloadCustom<OrganisationObjectives> requData = ((PayloadCustom<OrganisationObjectives>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task SearchOrganisationAsync_InValidToken()
        {
            ///arrange
            string organisationName = "CompunnelDigital";
            List<OrganisationSearch> organisationSearches = new List<OrganisationSearch>();

            ///act
            _organisationService.Setup(x => x.SearchOrganisationAsync(It.IsAny<string>())).ReturnsAsync(organisationSearches);

            ///assert
            var result = await _organisationController.SearchOrganisationAsync(organisationName) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task SearchOrganisationAsync_ValidToken()
        {
            ///arrange
            string organisationName = "CompunnelDigital";
            List<OrganisationSearch> organisationSearches = new List<OrganisationSearch>();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.SearchOrganisationAsync(It.IsAny<string>())).ReturnsAsync(organisationSearches);

            ///assert
            var result = await _organisationController.SearchOrganisationAsync(organisationName);
            PayloadCustom<OrganisationSearch> requData = ((PayloadCustom<OrganisationSearch>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task SearchOrganisationAsync_NotSuccess()
        {
            ///arrange
            string organisationName = "CompunnelDigital";
            List<OrganisationSearch> organisationSearches = null;

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.SearchOrganisationAsync(It.IsAny<string>())).ReturnsAsync(organisationSearches);

            ///assert
            var result = await _organisationController.SearchOrganisationAsync(organisationName);
            PayloadCustom<OrganisationSearch> requData = ((PayloadCustom<OrganisationSearch>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetCurrentOrganisationCycleAsync_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            OrganisationCycleResponse organisationCycleResponse = new OrganisationCycleResponse() { CycleDuration = "HalfYearly" };

            ///act
            _organisationService.Setup(x => x.GetCurrentCycleAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleResponse);

            ///assert
            var result = await _organisationController.GetCurrentOrganisationCycleAsync(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetCurrentOrganisationCycleAsync_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            OrganisationCycleResponse organisationCycleResponse = new OrganisationCycleResponse() { CycleDuration = "HalfYearly" };

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetCurrentCycleAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleResponse);

            ///assert
            var result = await _organisationController.GetCurrentOrganisationCycleAsync(organisationId);
            PayloadCustom<OrganisationCycleResponse> requData = ((PayloadCustom<OrganisationCycleResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetCurrentOrganisationCycleAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            OrganisationCycleResponse organisationCycleResponse = null;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetCurrentCycleAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleResponse);

            ///assert
            var result = await _organisationController.GetCurrentOrganisationCycleAsync(organisationId);
            PayloadCustom<OrganisationCycleResponse> requData = ((PayloadCustom<OrganisationCycleResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task UpdateOrganisationAsync_InValidToken()
        {
            ///arrange
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationName = "CompunnelDigital" };
            Organisation organisation = new Organisation();
            Employee employee = new Employee();
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            ///act
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UpdateOrganisationAsync(organisationRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UpdateOrganisationAsync_ValidToken()
        {
            ///arrange
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2, OrganisationName = "CompunnelDigital", OrganisationId = 1 };
            Organisation organisation = null;
            Employee employee = new Employee();
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(x => x.DoesCycleFallsInFutureDate(It.IsAny<OrganisationRequest>())).ReturnsAsync(true);

            ///assert
            var result = await _organisationController.UpdateOrganisationAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task UpdateOrganisationAsync_Error()
        {
            ///arrange
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2 };
            Organisation organisation = new Organisation();
            Employee employee = null;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UpdateOrganisationAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UpdateOrganisationAsync_OrganisationExists()
        {
            ///arrange
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2, OrganisationName = "CompunnelDigital", OrganisationId = 1 };
            Organisation organisation = new Organisation() { OrganisationName = "CompunnelDigital" };
            Employee employee = null;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UpdateOrganisationAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UpdateOrganisationAsync_NotSuccess()
        {
            ///arrange
            OrganisationRequest organisationRequest = new OrganisationRequest() { OrganisationLeader = 2, OrganisationName = "CompunnelDigital", OrganisationId = 1 };
            Organisation organisation = null;
            Employee employee = new Employee();
            IOperationStatus operationStatus = new OperationStatus();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateOrganisationAsync(It.IsAny<OrganisationRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UpdateOrganisationAsync(organisationRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task GetOrganisationCycleDetailsAsync_InValidToken()
        {
            ///arrange
            long organisationId = 1;
            Organisation organisation = new Organisation();
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { OrganisationName = "CompunnelDigital" };

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationCycleDetailsAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleDetails);

            ///assert
            var result = await _organisationController.GetOrganisationCycleDetailsAsync(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetOrganisationCycleDetailsAsync_Error()
        {
            ///arrange
            long organisationId = 0;
            Organisation organisation = new Organisation();
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { OrganisationName = "CompunnelDigital" };

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationCycleDetailsAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleDetails);

            ///assert
            var result = await _organisationController.GetOrganisationCycleDetailsAsync(organisationId);
            PayloadCustom<OrganisationCycleDetails> requData = ((PayloadCustom<OrganisationCycleDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetOrganisationCycleDetailsAsync_OrganisationNotExists()
        {
            ///arrange
            long organisationId = 1;
            Organisation organisation = null;
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { OrganisationName = "CompunnelDigital" };

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationCycleDetailsAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleDetails);

            ///assert
            var result = await _organisationController.GetOrganisationCycleDetailsAsync(organisationId);
            PayloadCustom<OrganisationCycleDetails> requData = ((PayloadCustom<OrganisationCycleDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetOrganisationCycleDetailsAsync_ValidToken()
        {
            ///arrange
            long organisationId = 1;
            Organisation organisation = new Organisation();
            OrganisationCycleDetails organisationCycleDetails = new OrganisationCycleDetails() { OrganisationName = "CompunnelDigital" };

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationCycleDetailsAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleDetails);

            ///assert
            var result = await _organisationController.GetOrganisationCycleDetailsAsync(organisationId);
            PayloadCustom<OrganisationCycleDetails> requData = ((PayloadCustom<OrganisationCycleDetails>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetOrganisationCycleDetailsAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 1;
            Organisation organisation = new Organisation();
            OrganisationCycleDetails organisationCycleDetails = null;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationCycleDetailsAsync(It.IsAny<long>())).ReturnsAsync(organisationCycleDetails);

            ///assert
            var result = await _organisationController.GetOrganisationCycleDetailsAsync(organisationId);
            PayloadCustom<OrganisationCycleDetails> requData = ((PayloadCustom<OrganisationCycleDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetImportOkrCycleAsync_InValidToken()
        {
            ///arrange
            long organisationId = 1;
            long CurrentCycleId = 1;
            int cycleYear = 2019;
            List<ImportOkrCycle> importOkrCycle = new List<ImportOkrCycle>();

            ///act
            _organisationService.Setup(x => x.GetImportOkrCycleAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(importOkrCycle);

            ///assert
            var result = await _organisationController.GetImportOkrCycleAsync(organisationId, CurrentCycleId, cycleYear) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetImportOkrCycleAsync_ValidToken()
        {
            ///arrange
            long organisationId = 1;
            long CurrentCycleId = 1;
            int cycleYear = 2019;
            List<ImportOkrCycle> importOkrCycle = new List<ImportOkrCycle>();

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetImportOkrCycleAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(importOkrCycle);

            ///assert
            var result = await _organisationController.GetImportOkrCycleAsync(organisationId, CurrentCycleId, cycleYear);
            PayloadCustom<ImportOkrCycle> requData = ((PayloadCustom<ImportOkrCycle>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetImportOkrCycleAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 1;
            long CurrentCycleId = 1;
            int cycleYear = 2019;
            List<ImportOkrCycle> importOkrCycle = null;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetImportOkrCycleAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(importOkrCycle);

            ///assert
            var result = await _organisationController.GetImportOkrCycleAsync(organisationId, CurrentCycleId, cycleYear);
            PayloadCustom<ImportOkrCycle> requData = ((PayloadCustom<ImportOkrCycle>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetAllOrganisationsAsync_InValidToken()
        {
            ///arrange
            List<ActiveOrganisations> activeOrganisations = new List<ActiveOrganisations>();

            ///act
            _organisationService.Setup(x => x.GetAllOrganisationsAsync()).ReturnsAsync(activeOrganisations);

            ///assert
            var result = await _organisationController.GetAllOrganisationsAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllOrganisationsAsync_NotSuccess()
        {
            ///arrange
            List<ActiveOrganisations> activeOrganisations = new List<ActiveOrganisations>();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetAllOrganisationsAsync()).ReturnsAsync(activeOrganisations);

            ///assert
            var result = await _organisationController.GetAllOrganisationsAsync();
            PayloadCustom<ActiveOrganisations> requData = ((PayloadCustom<ActiveOrganisations>)((ObjectResult)result).Value);
            var finalData = requData.EntityList;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task GetAllOrganisationsAsync_ValidToken()
        {
            ///arrange
            List<ActiveOrganisations> activeOrganisations = new List<ActiveOrganisations>() { new ActiveOrganisations() { OrganisationName = "Compunnel", OrganisationId = 1 } };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetAllOrganisationsAsync()).ReturnsAsync(activeOrganisations);

            ///assert
            var result = await _organisationController.GetAllOrganisationsAsync();
            PayloadCustom<ActiveOrganisations> requData = ((PayloadCustom<ActiveOrganisations>)((ObjectResult)result).Value);
            var finalData = requData.EntityList;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task UndoChangesForOrganisationAsync_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();

            ///act
            _organisationService.Setup(x => x.UndoChangesForOrganisationAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UndoChangesForOrganisationAsync(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UndoChangesForOrganisationAsync_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.UndoChangesForOrganisationAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UndoChangesForOrganisationAsync(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task UndoChangesForOrganisationAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.UndoChangesForOrganisationAsync(It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.UndoChangesForOrganisationAsync(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationAsync_InValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Employee employee = new Employee();
            ChildRequest childRequest = new ChildRequest();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddChildOrganisationAsync(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationAsync(childRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddChildOrganisationAsync_Error()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Employee employee = null;
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddChildOrganisationAsync(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationAsync_OrganisationExists()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Organisation organisations = null;
            Employee employee = new Employee();
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1, ChildOrganisationName = "InfoPro", ParentOrganisationId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisations);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddChildOrganisationAsync(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationAsync_ValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Organisation organisation = new Organisation();
            Employee employee = new Employee();
            Organisation organisations = null;
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1, ChildOrganisationName = "InfoPro", ParentOrganisationId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisations);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddChildOrganisationAsync(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationAsync_NotSuccess()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Employee employee = new Employee();
            Organisation organisations = null;
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1, ChildOrganisationName = "InfoPro", ParentOrganisationId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>())).ReturnsAsync(organisations);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddChildOrganisationAsync(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task EditChildOrganisationAsync_InValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Employee employee = new Employee();
            ChildRequest childRequest = new ChildRequest();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateChildOrganisation(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.EditChildOrganisationAsync(childRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task EditChildOrganisationAsync_Error()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Employee employee = null;
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateChildOrganisation(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.EditChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task EditChildOrganisationAsync_OrganisationExists()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Organisation organisations = null;
            Employee employee = new Employee();
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1, ChildOrganisationName = "InfoPro", ParentOrganisationId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisations);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateChildOrganisation(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.EditChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task EditChildOrganisationAsync_ValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Organisation organisation = new Organisation();
            Organisation organisations = null;
            Employee employee = new Employee();
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1, ChildOrganisationName = "InfoPro", ParentOrganisationId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisations);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateChildOrganisation(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.EditChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task EditChildOrganisationAsync_NotSuccess()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();
            Organisation organisations = null;
            Employee employee = new Employee();
            ChildRequest childRequest = new ChildRequest() { LeaderId = 1, ChildOrganisationName = "InfoPro", ParentOrganisationId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.GetOrganisationByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(organisations);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.UpdateChildOrganisation(It.IsAny<ChildRequest>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.EditChildOrganisationAsync(childRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task DetachChildOrganisationFromParentOrganisationAsync_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DetachChildOrganisationFromParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DetachChildOrganisationFromParentOrganisationAsync(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DetachChildOrganisationFromParentOrganisationAsync_Error()
        {
            ///arrange
            long organisationId = 0;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DetachChildOrganisationFromParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DetachChildOrganisationFromParentOrganisationAsync(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DetachChildOrganisationFromParentOrganisationAsync_OrganisationNotExists()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = null;

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DetachChildOrganisationFromParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DetachChildOrganisationFromParentOrganisationAsync(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DetachChildOrganisationFromParentOrganisationAsync_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DetachChildOrganisationFromParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DetachChildOrganisationFromParentOrganisationAsync(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task DetachChildOrganisationFromParentOrganisationAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DetachChildOrganisationFromParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DetachChildOrganisationFromParentOrganisationAsync(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task AddParentToParentOrganisationAsync_InValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();
            Organisation organisation = new Organisation();
            ParentRequest parentRequest = new ParentRequest();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddParentToParentOrganisationAsync(It.IsAny<ParentRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddParentToParentOrganisationAsync(parentRequest) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddParentToParentOrganisationAsync_Error()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = null;
            Organisation organisation = new Organisation();
            ParentRequest parentRequest = new ParentRequest() { LeaderId = 1 };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddParentToParentOrganisationAsync(It.IsAny<ParentRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddParentToParentOrganisationAsync(parentRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddParentToParentOrganisationAsync_OrganisationNotExists()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();
            Organisation organisation = null;
            ParentRequest parentRequest = new ParentRequest() { LeaderId = 1, OrganisationId = 1, ParentName = "Compunnel" };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddParentToParentOrganisationAsync(It.IsAny<ParentRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddParentToParentOrganisationAsync(parentRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddParentToParentOrganisationAsync_ValidToken()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Employee employee = new Employee();
            Organisation organisation = new Organisation();
            ParentRequest parentRequest = new ParentRequest() { LeaderId = 1, OrganisationId = 1, ParentName = "Compunnel" };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddParentToParentOrganisationAsync(It.IsAny<ParentRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddParentToParentOrganisationAsync(parentRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AddParentToParentOrganisationAsync_NotSuccess()
        {
            ///arrange
            IOperationStatus operationStatus = new OperationStatus();
            Employee employee = new Employee();
            Organisation organisation = new Organisation();
            ParentRequest parentRequest = new ParentRequest() { LeaderId = 1, OrganisationId = 1, ParentName = "Compunnel" };

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _userService.Setup(x => x.GetUserByEmployeeIdAsync(It.IsAny<long>())).ReturnsAsync(employee);
            _organisationService.Setup(x => x.AddParentToParentOrganisationAsync(It.IsAny<ParentRequest>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddParentToParentOrganisationAsync(parentRequest);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationToParentOrganisationAsync_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            long childId = 3;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildOrganisationToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationToParentOrganisationAsync(organisationId, childId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddChildOrganisationToParentOrganisationAsync_Error()
        {
            ///arrange
            long organisationId = 0;
            long childId = 0;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildOrganisationToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationToParentOrganisationAsync(organisationId, childId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationToParentOrganisationAsync_OrganisationNotExists()
        {
            ///arrange
            long organisationId = 2;
            long childId = 3;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = null;

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildOrganisationToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationToParentOrganisationAsync(organisationId, childId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationToParentOrganisationAsync_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            long childId = 3;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildOrganisationToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationToParentOrganisationAsync(organisationId, childId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildOrganisationToParentOrganisationAsync_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            long childId = 3;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildOrganisationToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildOrganisationToParentOrganisationAsync(organisationId, childId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task AddChildAsParentToParentOrganisationAsync_InValidToken()
        {
            ///arrange
            long oldParentId = 2;
            long newParentId = 3;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildAsParentToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildAsParentToParentOrganisationAsync(oldParentId, newParentId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AddChildAsParentToParentOrganisationAsync_Error()
        {
            ///arrange
            long oldParentId = 0;
            long newParentId = 0;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildAsParentToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildAsParentToParentOrganisationAsync(oldParentId, newParentId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildAsParentToParentOrganisationAsync_OrganisationNotExists()
        {
            ///arrange
            long oldParentId = 2;
            long newParentId = 3;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = null;

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildAsParentToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildAsParentToParentOrganisationAsync(oldParentId, newParentId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildAsParentToParentOrganisationAsync_ValidToken()
        {
            ///arrange
            long oldParentId = 2;
            long newParentId = 3;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildAsParentToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildAsParentToParentOrganisationAsync(oldParentId, newParentId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task AddChildAsParentToParentOrganisationAsync_NotSuccess()
        {
            ///arrange
            long oldParentId = 2;
            long newParentId = 3;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.AddChildAsParentToParentOrganisationAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.AddChildAsParentToParentOrganisationAsync(oldParentId, newParentId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOrganisation_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DeleteOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DeleteOrganisation(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteOrganisation_Error()
        {
            ///arrange
            long organisationId = 0;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DeleteOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DeleteOrganisation(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOrganisation_OrganisationNotExists()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = null;

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DeleteOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);

            ///assert
            var result = await _organisationController.DeleteOrganisation(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOrganisation_HaveChildOrganisation()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DeleteOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(x => x.HaveChildOrganisationsAsync(It.IsAny<long>())).ReturnsAsync(true);

            ///assert
            var result = await _organisationController.DeleteOrganisation(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOrganisation_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus() { Success = true };
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DeleteOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(x => x.HaveChildOrganisationsAsync(It.IsAny<long>())).ReturnsAsync(false);

            ///assert
            var result = await _organisationController.DeleteOrganisation(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task DeleteOrganisation_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            IOperationStatus operationStatus = new OperationStatus();
            Organisation organisation = new Organisation();

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetOrganisationAsync(It.IsAny<long>())).ReturnsAsync(organisation);
            _organisationService.Setup(x => x.DeleteOrganisationAsync(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(operationStatus);
            _organisationService.Setup(x => x.HaveChildOrganisationsAsync(It.IsAny<long>())).ReturnsAsync(false);

            ///assert
            var result = await _organisationController.DeleteOrganisation(organisationId);
            PayloadCustom<IOperationStatus> requData = ((PayloadCustom<IOperationStatus>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadLogoOnCloudFront_InValidToken()
        {
            ///arrange
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.xlsx");

            ///act
            _organisationService.Setup(x => x.UploadLogoOnAzure(It.IsAny<IFormFile>())).ReturnsAsync("");

            ///assert
            var result = await _organisationController.UploadLogoOnAzure(file) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task UploadLogoOnCloudFront_Error()
        {
            //arrange
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "dummy.xlsx");

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.UploadLogoOnAzure(It.IsAny<IFormFile>())).ReturnsAsync("");

            ///assert
            var result = await _organisationController.UploadLogoOnAzure(file);
            PayloadCustom<string> requData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadLogoOnCloudFront_InValidFile()
        {
            //arrange
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.exe");

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.UploadLogoOnAzure(It.IsAny<IFormFile>())).ReturnsAsync("");

            ///assert
            var result = await _organisationController.UploadLogoOnAzure(file);
            PayloadCustom<string> requData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task UploadLogoOnCloudFront_NotSuccess()
        {
            //arrange
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.xlsx");

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.UploadLogoOnAzure(It.IsAny<IFormFile>())).ReturnsAsync("");

            ///assert
            var result = await _organisationController.UploadLogoOnAzure(file);
            PayloadCustom<string> requData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task UploadLogoOnCloudFront_ValidToken()
        {
            //arrange
            IFormFile file = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 1, 1, "Data", "dummy.jpg");

            ///act
            _userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.UploadLogoOnAzure(It.IsAny<IFormFile>())).ReturnsAsync("");

            ///assert
            var result = await _organisationController.UploadLogoOnAzure(file);
            PayloadCustom<string> requData = ((PayloadCustom<string>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetUsersTeamDetailsAsync_InValidToken()
        {
            ///arrange
            List<TeamDetails> teamDetails = new List<TeamDetails>();
            int goalType = 1;
            long empId = 1;
            bool isCoach = false;

            ///act
            _organisationService.Setup(x => x.GetUsersTeamDetailsAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetUsersTeamDetailsAsync(goalType, empId, isCoach) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetUsersTeamDetailsAsync_NotSuccess()
        {
            ///arrange
            List<TeamDetails> teamDetails = new List<TeamDetails>();
            int goalType = 1;
            long empId = 1;
            bool isCoach = false;
            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetUsersTeamDetailsAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetUsersTeamDetailsAsync(goalType, empId, isCoach);
            PayloadCustom<TeamDetails> requData = ((PayloadCustom<TeamDetails>)((ObjectResult)result).Value);
            var finalData = requData.EntityList;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task GetUsersTeamDetailsAsync_ValidToken()
        {
            ///arrange
            List<TeamDetails> teamDetails = new List<TeamDetails>() { new TeamDetails() { OrganisationName = "Compunnel", OrganisationId = 1 } };
            int goalType = 1;
            long empId = 1;
            bool isCoach = false;
            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetUsersTeamDetailsAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetUsersTeamDetailsAsync(goalType, empId, isCoach);
            PayloadCustom<TeamDetails> requData = ((PayloadCustom<TeamDetails>)((ObjectResult)result).Value);
            var finalData = requData.EntityList;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task GetUsersTeamDetailsAsync_Error()
        {
            ///arrange
            List<TeamDetails> teamDetails = new List<TeamDetails>() { new TeamDetails() { OrganisationName = "Compunnel", OrganisationId = 1 } };
            int goalType = 0;
            long empId = 1;
            bool isCoach = false;
            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetUsersTeamDetailsAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<bool>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetUsersTeamDetailsAsync(goalType, empId, isCoach);
            PayloadCustom<TeamDetails> requData = ((PayloadCustom<TeamDetails>)((ObjectResult)result).Value);
            var finalData = requData.IsSuccess;
            Assert.False(finalData);
        }

        [Fact]
        public async Task GetTeamDetailsByIdAsync_InValidToken()
        {
            ///arrange
            SubTeamDetails teamDetails = new SubTeamDetails();
            long teamId = 1;

            ///act
            _organisationService.Setup(x => x.GetTeamDetailsByIdAsync(It.IsAny<long>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetTeamDetailsByIdAsync(teamId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetTeamDetailsByIdAsync_NotSuccess()
        {
            ///arrange
            SubTeamDetails teamDetails = new SubTeamDetails();
            long teamId = 1;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetTeamDetailsByIdAsync(It.IsAny<long>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetTeamDetailsByIdAsync(teamId);
            PayloadCustom<SubTeamDetails> requData = ((PayloadCustom<SubTeamDetails>)((ObjectResult)result).Value);
            var finalData = requData.Entity;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task GetTeamDetailsByIdAsync_ValidToken()
        {
            ///arrange
            SubTeamDetails teamDetails = new SubTeamDetails() { OrganisationName = "Compunnel", OrganisationId = 1 };
            long teamId = 1;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetTeamDetailsByIdAsync(It.IsAny<long>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetTeamDetailsByIdAsync(teamId);
            PayloadCustom<SubTeamDetails> requData = ((PayloadCustom<SubTeamDetails>)((ObjectResult)result).Value);
            var finalData = requData.Entity;
            Assert.NotNull(finalData);
        }

        [Fact]
        public async Task GetTeamDetailsByIdAsync_Error()
        {
            ///arrange
            SubTeamDetails teamDetails = new SubTeamDetails() { OrganisationName = "Compunnel", OrganisationId = 1 };
            long teamId = 0;

            ///act
            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetTeamDetailsByIdAsync(It.IsAny<long>())).ReturnsAsync(teamDetails);

            ///assert
            var result = await _organisationController.GetTeamDetailsByIdAsync(teamId);
            PayloadCustom<SubTeamDetails> requData = ((PayloadCustom<SubTeamDetails>)((ObjectResult)result).Value);
            var finalData = requData.IsSuccess;
            Assert.False(finalData);
        }


        [Fact]
        public async Task GetDirectReportsById_InValidToken()
        {
            long employeeId = 795;
            var directReports = new List<DirectReportsDetails>();

            _organisationService.Setup(x => x.GetDirectReportsByIdAsync(It.IsAny<long>())).ReturnsAsync(directReports);

            var result = await _organisationController.GetDirectReportsByIdAsync(employeeId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetDirectReportsById_ValidToken()
        {
            long employeeId = 795;
            var directReports = new List<DirectReportsDetails>();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetDirectReportsByIdAsync(It.IsAny<long>())).ReturnsAsync(directReports);

            var result = await _organisationController.GetDirectReportsByIdAsync(employeeId);
            PayloadCustom<DirectReportsDetails> requData = ((PayloadCustom<DirectReportsDetails>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetDirectReportsById_NotSuccess()
        {
            long employeeId = 0;
            var directReports = new List<DirectReportsDetails>();

            _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            _organisationService.Setup(x => x.GetDirectReportsByIdAsync(It.IsAny<long>())).ReturnsAsync(directReports);

            var result = await _organisationController.GetDirectReportsByIdAsync(employeeId);
            PayloadCustom<DirectReportsDetails> requData = ((PayloadCustom<DirectReportsDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }


        #region Private region
        private void SetUserClaimsAndRequest()
        {
            _organisationController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.Role, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108"),
                new Claim(ClaimTypes.Email, "abcd@gmail.com")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _organisationController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };

            string sampleAuthToken = Guid.NewGuid().ToString();
            _organisationController.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + sampleAuthToken;
        }
        #endregion

    }
}
