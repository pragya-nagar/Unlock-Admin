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
    public interface IOrganisationService
    {
        Task<IOperationStatus> AddOrganisationAsync(OrganisationRequest request, long loggedInUserId, string jwtToken);

        Task<IOperationStatus> UpdateOrganisationAsync(OrganisationRequest request, long loggedInUserId,
            string jwtToken);

        Task<OrganisationObjectives> GetObjectivesByOrgIdAsync(long organisationId);
        Task<OrganisationDetail> GetOrganisationByIdAsync(long organisationId);
        Task<OrganisationCycleResponse> GetCurrentCycleAsync(long organisationId);
        Task<List<OrganisationSearch>> SearchOrganisationAsync(string organisationName);
        Task<OrganisationCycleDetails> GetOrganisationCycleDetailsAsync(long organisationId);
        Task<List<ImportOkrCycle>> GetImportOkrCycleAsync(long organisationId, long currentCycleId, int cycleYear);
        Task<List<ActiveOrganisations>> GetAllOrganisationsAsync();
        Task<IOperationStatus> UndoChangesForOrganisationAsync(long organisationId);

        Task<IOperationStatus> DetachChildOrganisationFromParentOrganisationAsync(long organisationId,
            long loggedInUserId);

        Task<IOperationStatus> AddParentToParentOrganisationAsync(ParentRequest parent, long loggedInuserId);
        Task<IOperationStatus> AddChildOrganisationAsync(ChildRequest request, long loggedInUserId, string jwtToken);
        Task<IOperationStatus> UpdateChildOrganisation(ChildRequest request, long loggedInUserId, string jwtToken);

        Task<IOperationStatus> AddChildOrganisationToParentOrganisationAsync(long organisationId, long ChildId,
            long loggedInUserId);

        Task<IOperationStatus> AddChildAsParentToParentOrganisationAsync(long oldParentId, long newParentId,
            long loggedInUserId);

        Task<IOperationStatus> DeleteOrganisationAsync(long organisationId, long loggedInUserId);
        Task<Organisation> GetOrganisationAsync(long orgId);
        Task<Organisation> GetOrganisationByNameAsync(string organisationName);
        Task<Organisation> GetOrganisationByNameAsync(string organisationName, long orgId);

        Task<IOperationStatus> GenerateOrganisationCycleAsync(long cycleDurationId, long organisationId,
            DateTime StartDate, long loggedInUserId);

        Task<string> UploadLogoOnAzure(IFormFile file);
        Task<bool> HaveChildOrganisationsAsync(long organisationId);
        Task<long> GetParentOrganisationIdAsync(long organisationId);
        Task<bool> DoesCycleFallsInFutureDate(OrganisationRequest organisationRequest);
        Task<List<TeamDetails>> GetUsersTeamDetailsAsync(long loggedInUser, int goalType, long empId, bool isCoach);
        Task<SubTeamDetails> GetTeamDetailsByIdAsync(long teamId);
        Task<List<SubTeamDetails>> GetTeamDetailsAsync();
        Task<List<DirectReportsDetails>> GetDirectReportsByIdAsync(long employeeId);
        Task<List<ColorCodesResponse>> GetOrganizationColorCodesAsync();
        Task<IOperationStatus> UpdateOrganisationColorAsync(long loggedInUserId,string jwtToken);
        Task<EmployeeOrganizationDetails> GetOrganizationDetailsByEmployeeId(long employeeId);
        Task<LicenseDetail> GetLicenceDetail(string jwtToken);
    }
}
