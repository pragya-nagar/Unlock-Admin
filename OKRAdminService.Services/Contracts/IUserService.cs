using Microsoft.AspNetCore.Http;
using OKRAdminService.EF;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using OKRAdminService.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface IUserService
    {
        Task<LoginUserDetails> Identity(long userId, string token);
        Task<UserLoginResponse> GetIdentity(string userEmail);
        Task<UserLoginResponse> LoginAsync(LoginRequest loginRequest);
        Task<UserLoginResponse> UserByToken(string userEmail, string subDomainName);
        void SaveLog(string pageName, string functionName, string errorDetail);
        void Logout(string tokenId, long employeeId);
        bool IsUsersActiveToken(string tokenId, long employeeId, int tokenType = 1);
        bool IsActiveToken(string tokenId, long employeeId, int tokenType = 1);
        Task<PageResult<SearchUserList>> SearchEmployee(string key, int page, int pageSize, long employeeId);
        Task<Employee> GetUserByEmployeeIdAsync(long userId);
        Task<UserDetails> GetUserByEmployeeCodeAsync(string empCode);
        Task<Employee> GetUserByEmployeeCodeAsync(string empCode, long employeeId);
        Task<UserDetails> GetUserByEmpIdAsync(long empId);
        Task<Employee> GetUserByMailIdAsync(string mailId);
        Task<Employee> GetUserByEmailIdAsync(string emailId, long employeeId);
        Task<PageResults<AllUsersResponse>> GetAllUsersAsync(int pageIndex = 1, int pageSize = 10);
        Task<PageResults<AllUsersResponse>> MultiSearchUserListAsync(string jwtToken, List<string> searchTexts, int pageIndex = 1, int pageSize = 10);
        Task<UserRequestModel> AddUserAsync(UserRequestModel userRequestModel, long loggedInUserId, string subDomain);
        Task<IOperationStatus> EditUserAsync(UserRequestModel userRequestModel, long loggedInUserId);
        Task<IOperationStatus> DeleteUserAsync(List<long> employeeIdList, long loggedInUserId, string jwtToken);
        Task<IOperationStatus> UploadBulkUserAsync(IFormFile formFile, long loggedInUserId, string jwtToken, string subDomain);
        Task<IOperationStatus> ChangeRoleAsync(ChangeRoleRequestModel changeRoleRequestModel, long loggedInUserId);
        Task<IOperationStatus> ChangeUserReportingAsync(EditUserReportingRequest editUserReportingRequest, long loggedInUserId);
        Task<IOperationStatus> ChangeUserOrganisationAsync(ChangeUserOrganisationRequest changeUserOrganisationRequest, long loggedInUserId, string jwtToken);
        Task<string> DownloadCsvAsync();
        Task<List<string>> GetDesignationAsync(string designation);
        Task<Employee> GetReportingToOrganisationHeadAsync(List<long> employeeIdList);
        Task<IOperationStatus> ResetPasswordAsync(long employeeId, ResetPasswordRequest resetPasswordRequest);
        Task<bool> SendResetPasswordMailAsync(SendResetPasswordMailRequest sendResetPasswordMailRequest);
        Task<IOperationStatus> AddUpdateUserContactAsync(UserContactDetail userContactDetail, long loggedInUserId, string jwtToken);
        Task<EmployeeProfileResponse> GetEmployeeProfileByEmployeeIdAsync(long employeeId, string jwtToken);
        Task<IOperationStatus> UploadProfileImageAsync(IFormFile file, long loggedInUser);
        Task<IOperationStatus> DeleteProfileImageAsync(long logedInUser);
        Task<IOperationStatus> ChangePasswordAsync(long employeeId, ChangePasswordRequest changePasswordRequest);
        Task<bool> ChangeAdPasswordAsync(ChangePasswordRequest changePasswordRequest);
        Task<RefreshTokenResponse> GetRefreshToken(string jwtToken, long logedInUser);
        Task<bool> ReSendResetPasswordMailAsync(long employeeId);
        string GenerateJwtToken(Employee userDetails, long loggedInRoleId, DateTime expireTime);
        Task<UserToken> GetEmployeeAccessTokenAsync(long empId);
        Task<List<GoalUnlockDate>> GetGoalLockDateAsync(long organisationCycleId);
        Task<IOperationStatus> AddUpdateUserAccessTokenAsync(long employeeId, string token, DateTime expireTime, bool isNewToken);
        Task<string> GetExistingValidUserTokenAsync(long employeeId);
        UserToken GetUserTokenByTokenId(string token);
        PageResult<GlobalSearchList> GlobalSearch(string key, int searchType, int page, int pageSize, long employeeId);
        PageResult<SearchUserList> SearchTeamEmployee(string key, long teamId, int page, int pageSize, long employeeId);
        Task<IOperationStatus> AddAdUserAsync(UserRequestModel userRequest, long loggedInUserId, string domain);
        Task<AdUserResponse> IsUserExistInAdAsync(string username);
        Task<bool> IsValidReporting(long empId, long reportingId);
    }

}
