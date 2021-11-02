using System;

namespace OKRAdminService.ViewModels
{
    public class UnLockLog
    {
        public long UnLockLogId { get; set; }
        public int Year { get; set; }
        public int Cycle { get; set; }
        public long EmployeeId { get; set; }
        public DateTime LockedOn { get; set; }
        public DateTime LockedTill { get; set; } 
        public long CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } 
        public int Status { get; set; } 
        public bool IsActive { get; set; } 
    }
}
