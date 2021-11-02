using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OKRAdminService.EF;
using OKRAdminService.Services.Contracts;
using OKRAdminService.ViewModels;
using OKRAdminService.WebCore.Controllers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OKRAdminService.UnitTests.Controller
{
    public class PermissionControllerTestCases
    {
        private readonly Mock<IPermissionService> _permissionService;
        private readonly Mock<IIdentityService> _identityService;
        private readonly PermissionController _permissionController;
        public PermissionControllerTestCases()
        {
            _identityService = new Mock<IIdentityService>();
            _permissionService = new Mock<IPermissionService>();
            _permissionController = new PermissionController(_identityService.Object, _permissionService.Object);
            SetUserClaimsAndRequest();
        }

        [Fact]
        public async Task EditPermissionToRoleAsync_InValidToken()
        {
            ///arrange
            long roleId = 2;
            long permissionId = 3;
            bool isChecked = true;
            ///act
            _permissionService.Setup(x => x.EditPermissionToRoleAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(true);
            ///assert
            var result = await _permissionController.EditPermissionToRoleAsync(roleId, permissionId, isChecked) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task EditPermissionToRoleAsync_ValidToken()
        {
            ///arrange
            long roleId = 2;
            long permissionId = 3;
            bool isChecked = true;
            ///act
            _permissionService.Setup(x => x.EditPermissionToRoleAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(true);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.EditPermissionToRoleAsync(roleId, permissionId, isChecked);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            Assert.True(requData.IsSuccess);
        }

        [Fact]
        public async Task EditPermissionToRoleAsync_Error()
        {
            ///arrange
            long roleId = 0;
            long permissionId = 0;
            bool isChecked = true;
            ///act
            _permissionService.Setup(x => x.EditPermissionToRoleAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<long>())).ReturnsAsync(true);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.EditPermissionToRoleAsync(roleId, permissionId, isChecked);
            PayloadCustom<bool> requData = ((PayloadCustom<bool>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.BadRequest, finalData);
        }

        [Fact]
        public async Task GetAllRolePermissionAsync_InValidToken()
        {
            ///arrange
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>();
            ///act
            _permissionService.Setup(x => x.GetAllRolePermissionAsync()).ReturnsAsync(list);
            ///assert
            var result = await _permissionController.GetAllRolePermissionAsync() as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetAllRolePermissionAsync_NotSuccess()
        {
            ///arrange
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>();
            ///act
            _permissionService.Setup(x => x.GetAllRolePermissionAsync()).ReturnsAsync(list);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.GetAllRolePermissionAsync();
            PayloadCustom<PermissionRoleResponseModel> requData = ((PayloadCustom<PermissionRoleResponseModel>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task GetAllRolePermissionAsync_ValidToken()
        {
            ///arrange
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>() { new PermissionRoleResponseModel() { RoleId = 1, RoleName = "Admin" } };
            ///act
            _permissionService.Setup(x => x.GetAllRolePermissionAsync()).ReturnsAsync(list);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.GetAllRolePermissionAsync();
            PayloadCustom<PermissionRoleResponseModel> requData = ((PayloadCustom<PermissionRoleResponseModel>)((ObjectResult)result).Value);
            var finalList = requData.EntityList;
            Assert.NotNull(finalList);
        }

        [Fact]
        public async Task SearchRoleAsync_InValidToken()
        {
            ///arrange
            string roleName = "Admin";
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>();
            ///act
            _permissionService.Setup(x => x.SearchRoleAsync(It.IsAny<string>())).ReturnsAsync(list);
            ///assert
            var result = await _permissionController.SearchRoleAsync(roleName) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task SearchRoleAsync_NotSuccess()
        {
            ///arrange
            string roleName = "Admin";
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>();
            ///act
            _permissionService.Setup(x => x.SearchRoleAsync(It.IsAny<string>())).ReturnsAsync(list);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.SearchRoleAsync(roleName);
            PayloadCustom<PermissionRoleResponseModel> requData = ((PayloadCustom<PermissionRoleResponseModel>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task SearchRoleAsync_ValidToken()
        {
            ///arrange
            string roleName = "Admin";
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>() { new PermissionRoleResponseModel() { RoleId = 1, RoleName = "Admin" } };
            ///act
            _permissionService.Setup(x => x.SearchRoleAsync(It.IsAny<string>())).ReturnsAsync(list);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.SearchRoleAsync(roleName);
            PayloadCustom<PermissionRoleResponseModel> requData = ((PayloadCustom<PermissionRoleResponseModel>)((ObjectResult)result).Value);
            var finalList = requData.EntityList;
            Assert.NotNull(finalList);
        }

        [Fact]
        public async Task SortRoleAsync_InValidToken()
        {
            ///arrange
            bool sortOrder = true;
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>();
            ///act
            _permissionService.Setup(x => x.SortRoleAsync(It.IsAny<bool>())).ReturnsAsync(list);
            ///assert
            var result = await _permissionController.SortRoleAsync(sortOrder) as StatusCodeResult;
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task SortRoleAsync_NotSuccess()
        {
            ///arrange
            bool sortOrder = true;
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>();
            ///act
            _permissionService.Setup(x => x.SortRoleAsync(It.IsAny<bool>())).ReturnsAsync(list);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.SortRoleAsync(sortOrder);
            PayloadCustom<PermissionRoleResponseModel> requData = ((PayloadCustom<PermissionRoleResponseModel>)((ObjectResult)result).Value);
            var finalData = requData.Status;
            Assert.Equal((int)HttpStatusCode.NoContent, finalData);
        }

        [Fact]
        public async Task SortRoleAsync_ValidToken()
        {
            ///arrange
            bool sortOrder = true;
            List<PermissionRoleResponseModel> list = new List<PermissionRoleResponseModel>() { new PermissionRoleResponseModel() { RoleId = 1, RoleName = "Admin" } };
            ///act
            _permissionService.Setup(x => x.SortRoleAsync(It.IsAny<bool>())).ReturnsAsync(list);
            //_userService.Setup(p => p.IsActiveToken(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>())).Returns(true);
            ///assert
            var result = await _permissionController.SortRoleAsync(sortOrder);
            PayloadCustom<PermissionRoleResponseModel> requData = ((PayloadCustom<PermissionRoleResponseModel>)((ObjectResult)result).Value);
            var finalList = requData.EntityList;
            Assert.NotNull(finalList);
        }


        #region Private region
        private void SetUserClaimsAndRequest()
        {
            _permissionController.ControllerContext = new ControllerContext();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "108"),
                new Claim(ClaimTypes.Role, "108"),
                new Claim(ClaimTypes.NameIdentifier, "108"),
                new Claim(ClaimTypes.Email, "abcd@gmail.com")
            };

            var identity = new ClaimsIdentity(claims, "108");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _permissionController.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal
            };

            string sampleAuthToken = Guid.NewGuid().ToString();
            _permissionController.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer " + sampleAuthToken;
        }
        #endregion

    }
}

