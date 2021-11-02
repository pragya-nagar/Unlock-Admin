namespace OKRAdminService.ViewModels.Response
{
    public class EmailTeamLeaderResponse
    {
        public long GoalObjectiveId { get; set; }
        public long GoalId { get; set; }       
        public long EmployeeId { get; set; }
        public string ObjectiveName { get; set; }
    }
}
