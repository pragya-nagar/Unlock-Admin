namespace OKRAdminService.ViewModels
{
    public class EmployeeInformation
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImagePath { get; set; }
        public bool IsActive { get; set; }
        public string Designation { get; set; }
    }
}
