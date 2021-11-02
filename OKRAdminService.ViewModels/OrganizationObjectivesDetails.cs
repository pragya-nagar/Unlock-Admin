namespace OKRAdminService.ViewModels
{
    public class OrganizationObjectivesDetails
    {
        public long Id { get; set; }
        public long OrgnizationId { get; set; }
        public string ObjectiveName { get; set; }
        public bool IsActive { get; set; }

    }
}
