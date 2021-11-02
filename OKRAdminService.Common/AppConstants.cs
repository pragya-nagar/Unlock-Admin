namespace OKRAdminService.Common
{
    public static class AppConstants
    {
        public const string PrivateObjective = "Private";
        public const string DefaultUserRole = "Default";
        public const string AdminRole = "Admin";
        public const long DefaultParentId = 0;
        public const long AdminRoleId = 2;
        public const int AppIdForAdmin = 4;
        public const int AppIdForOkrService = 3;
        public const string NewUserCreationMessage = "New users successfully created";
        public const string NewLeaderReplacedOldLeaderMessage = "The leader of Team <organizationName> has been changed from <old leader> to <new leader>";
        public const string UserRemovalMessage = " was removed from your company!";
        public const string BulkUploadMessage = "You have new team members!";
        public const string NewRoleCreationMessage = "A new user role <roleName> was created recently. Check it out in the admin section.";
        public const string RoleUpdationMessage = "The user role <roleName> was edited. If you have not approved it, you may want to review it.";
        public const string OrganisationSettingsChangesMessage = "<organisationName> organization settings altered. If this is not approved, you may want to review it.<settings>";
        public const string ProfileChangesMessage = "Changes were made to your profile. Was it you?";
        public const string UserOrganisationChangeMessage = " moved to organization <organisationName>";
        public const string UserRemovalFromSystem = "was deleted from the system.";
        public const string StrongPassRegex = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
        public const string EmailRegex = "^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@"
                                         + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$";
        public const string FirstNameRegex = @"^+[a-zA-Z0-9""'\s-]*$";
        public const string PhoneNumberRegex = "^[0-9-+()]{10,20}$";
        public const string CountryStdCodeRegex = "^[0-9+()]{1,10}$";
        public const string SkypeRegex = "^[A-Za-z][A-Za-z0-9\\d\\,\\-._]{4,100}$";
        public const string TwitterUrls = "https://twitter.com,http://twitter.com,https://www.twitter.com,http://www.twitter.com";
        public const string LinkedInUrls = "https://linkedin.com,http://linkedin.com,https://www.linkedin.com,http://www.linkedin.com";
        public const string Base64Regex = @"^[a-zA-Z0-9\+/]*={0,3}$";
        public const int ResetPasswordExpireHoursForNewlyAddedUser = 360;
        public const int ExpireHoursForLoggedInUser = 24;
        public const int ExpireHoursForResetPassword = 12;
        public const string SecretKey = "622a1812-5188-4f94-9155-e6515623";
        public const string PassportUserType = "Employee";
        public const string Learning = "Learning";
        public const string Digital = "Digital";
        public const string Staffing = "Staffing";
        public const string InfoproLearning = "Infopro Learning";
        public const string CompunnelDigital = "Compunnel Digital";
        public const string CompunnelStaffing = "Compunnel Staffing";
        public const string CompunnelSoftwareGroup = "Compunnel Software Group";
        public const string TopBar = "topBar.png";
        public const string LogoImages = "logo.png";
        public const string PasswordImage = "password-image.png";
        public const string ScreenImage = "screen-image.png";
        public const string TickImages = "tick.png";
        public const string ResetButtonImage = "reset-btn.png";
        public const string GetStartedButtonImage = "get-started-btn.png";
        public const string LeaderChangeImage = "leader-changed1.png";
        public const string UnlockButtonImage = "go-to-unlock-okr.png";
        public const string LoginButtonImage = "login.png";
        public const string UnlockSupportEmailId = "adminsupport@unlockokr.com";
        public const string UnlockLearn = "Unlock:learn";
        public const string InfoProLearning = "InfoProLearning";
        public const string Unlocklearn = "Unlock Learn";
        public const string Facebook = "facebook.png";
        public const string Linkedin = "linkedin.png";
        public const string Twitter = "twitter.png";
        public const string Instagram = "instagram.png";
        public const int AzureTokenType = 1;
        public const string HandShakeImage = "user-manager.png";
        public const string NotificationChangeleader = "ChangeLeader";
        public const int SkipCountConstant = 3;
        public const string ChangePasswordGraphUrl = "https://graph.microsoft.com/v1.0/me/changePassword";
        public const string Handshake = "hand-shake.png";
        public const string Credentials = "credentials.png";

        public const string GetAllUsers = "GetAllUsers";
        public const string TeamsDetails = "TeamsDetails";
        public const string TeamsById = "TeamsById";
        public const string OrganizationCycleDetails = "OrganizationCycleDetails";
        public const string OkrFilters = "OkrFilters";
        public const string GetAllMaster = "GetAllMaster";
        public const string AssignmentTypes = "AssignmentTypes";
        public const string OkrMasterData = "OkrMasterData";
        public const string Metrics = "Metrics";
    }
}
