namespace OKRAdminService.ViewModels.Response
{
    public class IdentityResponse: Identity
    {
        public bool IsTokenActive { get; set; }
        public string UserToken { get; set; }
    }
}
