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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace OKRAdminService.UnitTests.Controller
{
    public class MasterControllerTestCases
    {
        private readonly Mock<IMasterService> _masterService;
        private readonly Mock<IIdentityService> _identityService;
        private readonly MasterController _masterController;
        protected readonly Mock<IDistributedCache> _distributedCache;
        protected readonly Mock<IConfiguration> _Configuration;
        public MasterControllerTestCases()
        {
            _Configuration = new Mock <IConfiguration >();
            _masterService = new Mock<IMasterService>();
            _identityService = new Mock<IIdentityService>();
            _distributedCache = new Mock<IDistributedCache>();
            _masterController = new MasterController(_identityService.Object, _masterService.Object, _distributedCache.Object, _Configuration.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task GetAllMasterDetails_InValidToken()
        {
            ///arrange
            MasterResponse masterResponse = new MasterResponse();
            ///act
            _masterService.Setup(x => x.GetAllMasterDetailsAsync()).ReturnsAsync(masterResponse);
            ///assert
            var result = await _masterController.GetAllMasterDetails() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllMasterDetails_ValidToken()
        {
            ///arrange
            MasterResponse masterResponse = new MasterResponse();
            ///act
            _masterService.Setup(x => x.GetAllMasterDetailsAsync()).ReturnsAsync(masterResponse);
           // _userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAllMasterDetails();
            PayloadCustom<MasterResponse> requData = ((PayloadCustom<MasterResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllMasterDetails_NotSuccess()
        {
            ///arrange
            MasterResponse masterResponse = null;
            ///act
            _masterService.Setup(x => x.GetAllMasterDetailsAsync()).ReturnsAsync(masterResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAllMasterDetails();
            PayloadCustom<MasterResponse> requData = ((PayloadCustom<MasterResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetOkrFiltersMaster_InValidToken()
        {
            ///arrange
            long organisationId = 2;
            OkrStatusMasterDetails oKRStatusMasterDetails = new OkrStatusMasterDetails();
            ///act
            _masterService.Setup(x => x.GetOkrFiltersMasterAsync(It.IsAny<long>())).ReturnsAsync(oKRStatusMasterDetails);
            ///assert
            var result = await _masterController.GetOkrFiltersMaster(organisationId) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetOkrFiltersMaster_ValidToken()
        {
            ///arrange
            long organisationId = 2;
            OkrStatusMasterDetails oKRStatusMasterDetails = new OkrStatusMasterDetails();
            ///act
            _masterService.Setup(x => x.GetOkrFiltersMasterAsync(It.IsAny<long>())).ReturnsAsync(oKRStatusMasterDetails);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetOkrFiltersMaster(organisationId);
            PayloadCustom<OkrStatusMasterDetails> requData = ((PayloadCustom<OkrStatusMasterDetails>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetOkrFiltersMaster_NotSuccess()
        {
            ///arrange
            long organisationId = 2;
            OkrStatusMasterDetails oKRStatusMasterDetails = null;
            ///act
            _masterService.Setup(x => x.GetOkrFiltersMasterAsync(It.IsAny<long>())).ReturnsAsync(oKRStatusMasterDetails);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetOkrFiltersMaster(organisationId);
            PayloadCustom<OkrStatusMasterDetails> requData = ((PayloadCustom<OkrStatusMasterDetails>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetAssignmentTypeMasterAsync_InValidToken()
        {
            ///arrange
            List<AssignmentTypeResponse> assignmentTypeResponse = new List<AssignmentTypeResponse>();
            ///act
            _masterService.Setup(x => x.GetAssignmentTypeMasterAsync()).ReturnsAsync(assignmentTypeResponse);
            ///assert
            var result = await _masterController.GetAssignmentTypeMasterAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAssignmentTypeMasterAsync_ValidToken()
        {
            ///arrange
            List<AssignmentTypeResponse> assignmentTypeResponse = new List<AssignmentTypeResponse>();
            ///act
            _masterService.Setup(x => x.GetAssignmentTypeMasterAsync()).ReturnsAsync(assignmentTypeResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAssignmentTypeMasterAsync();
            PayloadCustom<List<AssignmentTypeResponse>> requData = ((PayloadCustom<List<AssignmentTypeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAssignmentTypeMasterAsync_NotSuccess()
        {
            ///arrange
            List<AssignmentTypeResponse> assignmentTypeResponse = new List<AssignmentTypeResponse>();
            ///act
            _masterService.Setup(x => x.GetAssignmentTypeMasterAsync()).ReturnsAsync(assignmentTypeResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAssignmentTypeMasterAsync();
            PayloadCustom<List<AssignmentTypeResponse>> requData = ((PayloadCustom<List<AssignmentTypeResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetAllMetricMasterAsync_InValidToken()
        {
            ///arrange
            List<MetricMasterResponse> metricMasterResponse = new List<MetricMasterResponse>();
            ///act
            _masterService.Setup(x => x.GetAllMetricMasterAsync()).ReturnsAsync(metricMasterResponse);
            ///assert
            var result = await _masterController.GetAllMetricMasterAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllMetricMasterAsync_ValidToken()
        {
            ///arrange
            List<MetricMasterResponse> metricMasterResponse = new List<MetricMasterResponse>();
            ///act
            _masterService.Setup(x => x.GetAllMetricMasterAsync()).ReturnsAsync(metricMasterResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAllMetricMasterAsync();
            PayloadCustom<List<MetricMasterResponse>> requData = ((PayloadCustom<List<MetricMasterResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllMetricMasterAsync_NotSuccess()
        {
            ///arrange
            List<MetricMasterResponse> metricMasterResponse = new List<MetricMasterResponse>();
            ///act
            _masterService.Setup(x => x.GetAllMetricMasterAsync()).ReturnsAsync(metricMasterResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAllMetricMasterAsync();
            PayloadCustom<List<MetricMasterResponse>> requData = ((PayloadCustom<List<MetricMasterResponse>>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }


        [Fact]
        public async Task GetAllOkrMasterAsync_InValidToken()
        {
            ///arrange
            GetAllOkrMaster metricMasterResponse = new GetAllOkrMaster();
            ///act
            _masterService.Setup(x => x.GetAllOkrMaster()).ReturnsAsync(metricMasterResponse);
            ///assert
            var result = await _masterController.GetAllOkrMasterAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllOkrMasterAsync_ValidToken()
        {
            ///arrange
            GetAllOkrMaster metricMasterResponse = new GetAllOkrMaster();
            ///act
            _masterService.Setup(x => x.GetAllOkrMaster()).ReturnsAsync(metricMasterResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAllOkrMasterAsync();
            PayloadCustom<GetAllOkrMaster> requData = ((PayloadCustom<GetAllOkrMaster>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllOkrMasterAsync_NotSuccess()
        {
            ///arrange
            GetAllOkrMaster metricMasterResponse = new GetAllOkrMaster();
            ///act
            _masterService.Setup(x => x.GetAllOkrMaster()).ReturnsAsync(metricMasterResponse);
            //_userService.Setup(p => p.IsUsersActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _masterController.GetAllOkrMasterAsync();
            PayloadCustom<GetAllOkrMaster> requData = ((PayloadCustom<GetAllOkrMaster>)((ObjectResult)result).Value);
            
            Assert.True(requData.IsSuccess);
        }



        #region Private region
        private void SetUserClaimsAndRequest()
        {
            _masterController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.Role, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108"),
                new Claim(ClaimTypes.Email, "abcd@gmail.com")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _masterController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };

            string sampleAuthToken = Guid.NewGuid().ToString();
            _masterController.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + sampleAuthToken;
        }
        #endregion

    }
}
