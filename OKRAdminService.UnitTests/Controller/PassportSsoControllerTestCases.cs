using Microsoft.AspNetCore.Mvc;
using Moq;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using OKRAdminService.WebCore.Controllers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OKRAdminService.UnitTests.Controller
{
    public class PassportSsoControllerTestCases
    {
        private readonly Mock<IPassportSsoService> _passportSsoService;
        private readonly Mock<IIdentityService> _identityService;
        private readonly PassportSsoController _passportSsoController;

        public PassportSsoControllerTestCases()
        {
            _passportSsoService = new Mock<IPassportSsoService>();
            _identityService = new Mock<IIdentityService>();
            _passportSsoController = new PassportSsoController(_identityService.Object, _passportSsoService.Object);
        }

        [Fact]
        public async Task SsoLoginAsync_Success()
        {
            SsoLoginRequest ssoLoginRequest = new SsoLoginRequest() { AppId = "test", SessionId = "test" };
            UserLoginResponse loginResponse = new UserLoginResponse() { TokenId = "token" };

            _passportSsoService.Setup(x => x.SsoLoginAsync(It.IsAny<SsoLoginRequest>())).ReturnsAsync(loginResponse);

            var result = await _passportSsoController.SsoLoginAsync(ssoLoginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task SsoLoginAsync_NotSuccess()
        {
            SsoLoginRequest ssoLoginRequest = new SsoLoginRequest() { AppId = "test", SessionId = "test" };
            UserLoginResponse loginResponse = new UserLoginResponse();

            _passportSsoService.Setup(x => x.SsoLoginAsync(It.IsAny<SsoLoginRequest>())).ReturnsAsync(loginResponse);

            var result = await _passportSsoController.SsoLoginAsync(ssoLoginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);            
            Assert.False(requData.IsSuccess);
        }

        [Fact]
        public async Task SsoLoginAsync_Error()
        {
            SsoLoginRequest ssoLoginRequest = new SsoLoginRequest();
            UserLoginResponse loginResponse = new UserLoginResponse();

            _passportSsoService.Setup(x => x.SsoLoginAsync(It.IsAny<SsoLoginRequest>())).ReturnsAsync(loginResponse);

            var result = await _passportSsoController.SsoLoginAsync(ssoLoginRequest);
            PayloadCustom<UserLoginResponse> requData = ((PayloadCustom<UserLoginResponse>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task ActiveUserAsync_Success()
        {
            List<PassportEmployeeResponse> passportEmployeeResponses = new List<PassportEmployeeResponse>() { new PassportEmployeeResponse() { EmployeeId = 1 } }; 
 
            _passportSsoService.Setup(x => x.ActiveUserAsync()).ReturnsAsync(passportEmployeeResponses);

            var result = await _passportSsoController.ActiveUserAsync();
            PayloadCustom<List<PassportEmployeeResponse>> requData = ((PayloadCustom<List<PassportEmployeeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task ActiveUserAsync_NoContent()
        {
            List<PassportEmployeeResponse> passportEmployeeResponses = new List<PassportEmployeeResponse>();

            _passportSsoService.Setup(x => x.ActiveUserAsync()).ReturnsAsync(passportEmployeeResponses);

            var result = await _passportSsoController.ActiveUserAsync();
            PayloadCustom<List<PassportEmployeeResponse>> requData = ((PayloadCustom<List<PassportEmployeeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task InActiveUserAsync_Success()
        {
            List<PassportEmployeeResponse> passportEmployeeResponses = new List<PassportEmployeeResponse>() { new PassportEmployeeResponse() { EmployeeId = 1 } };

            _passportSsoService.Setup(x => x.InActiveUserAsync()).ReturnsAsync(passportEmployeeResponses);

            var result = await _passportSsoController.InActiveUserAsync();
            PayloadCustom<List<PassportEmployeeResponse>> requData = ((PayloadCustom<List<PassportEmployeeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task InActiveUserAsync_NoContent()
        {
            List<PassportEmployeeResponse> passportEmployeeResponses = new List<PassportEmployeeResponse>();

            _passportSsoService.Setup(x => x.InActiveUserAsync()).ReturnsAsync(passportEmployeeResponses);

            var result = await _passportSsoController.InActiveUserAsync();
            PayloadCustom<List<PassportEmployeeResponse>> requData = ((PayloadCustom<List<PassportEmployeeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllPassportUsersAsync_Success()
        {
            List<PassportEmployeeResponse> passportEmployeeResponses = new List<PassportEmployeeResponse>() { new PassportEmployeeResponse() { EmployeeId = 1} };

            _passportSsoService.Setup(x => x.GetAllPassportUsersAsync()).ReturnsAsync(passportEmployeeResponses);

            var result = await _passportSsoController.GetAllPassportUsersAsync();
            PayloadCustom<List<PassportEmployeeResponse>> requData = ((PayloadCustom<List<PassportEmployeeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task GetAllPassportUsersAsync_NoContent()
        {
            List<PassportEmployeeResponse> passportEmployeeResponses = new List<PassportEmployeeResponse>();

            _passportSsoService.Setup(x => x.GetAllPassportUsersAsync()).ReturnsAsync(passportEmployeeResponses);

            var result = await _passportSsoController.GetAllPassportUsersAsync();
            PayloadCustom<List<PassportEmployeeResponse>> requData = ((PayloadCustom<List<PassportEmployeeResponse>>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }
    }
}
