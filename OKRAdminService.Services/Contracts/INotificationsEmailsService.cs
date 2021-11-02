using OKRAdminService.EF;
using OKRAdminService.ViewModels;
using OKRAdminService.ViewModels.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.Services.Contracts
{
    public interface INotificationsEmailsService
    {
        Task AddUserNotificationsAndEmailsAsync(Employee employee, string jwtToken);
        Task DeleteUserNotificationsAndEmailsAsync(List<Employee> employees, string jwtToken);
        Task BulkUploadNotificationsAndEmailsForCsvAsync(List<BulkUploadDataModel> bulkList, string jwtToken);
        Task BulkUploadNotificationsAndEmailsForExcelAsync(List<long> reporting, List<string> employeeCodes, string jwtToken);
        Task ResetPasswordNotificationsAndEmailAsync(Employee employeeDetails, long employeeId);
        Task AddOrganisationsNotificationsAndEmailsAsync(OrganisationRequest request, Organisation organisation, string jwtToken);
        Task AddChildOrganisationEmailAndNotificationsAsync(ChildRequest request, string jwtToken);
        Task UpdateChildOrganisationMailAndNotificationsAsync(ChildRequest request, Organisation organisation, long parentId, Organisation oldOrganisationLeader, long updatedBy, OrganisationCycleResponse cycleResponse, string jwtToken);
        Task CreateRoleMailAndNotificationsAsync(RoleRequestModel roleRequestModel, string roleCode, string jwtToken);
        Task UpdateOrganisationNotificationsAndEmailsAsync(OrganisationRequest request, Organisation organisation, Organisation oldOrganisationLeader, long updatedBy, OrganisationCycle organisationCycle, OrganizationObjective organizationObjective, string jwtToken);
        Task AddUpdateUserContactNotificationsAndMailsAsync(Employee employee, long loggedInUserId, string jwtToken);
        Task UploadProfileImageNotificationsAndEmailsAsync(Employee employee, long loggedInUser);
        Task EditUserNotificationsAndEmailsAsync(List<long> oldOrganisationId, long newOrganisationId, List<string> firstNames, List<long> reportingIds, long updatedBy, List<string> emailIds, string jwtToken);
        Task DeleteUserFromSystemNotificationsAndEmailsAsync(List<Employee> employees, string jwtToken);
        Task InviteAdUserEmailsAsync(UserRequestModel userRequestModel, string httpsSubDomain, string jwtToken);

    }
}
