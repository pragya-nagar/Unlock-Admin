
namespace OKRAdminService.ViewModels.Requests
{
    public class UpdateTeamLeaderOkrRequest
    {
        public long OldLeader { get; set; }
        public long NewLeader { get; set; }
        public long TeamId { get; set; }
        public long CycleId { get; set; }
    }
}
