using System;


namespace OKRAdminService.ViewModels.Requests
{
    public class OnBoardingRequest
    {
        public long EmployeeId { get; set; }
        public int SkipCount { get; set; } = 0;
        public int ReadyCount { get; set; } = 0;
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
    }
}
