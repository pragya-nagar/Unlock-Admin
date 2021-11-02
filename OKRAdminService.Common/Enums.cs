namespace OKRAdminService.Common
{
    /// MessageType
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// The information
        /// </summary>
        Info,
        /// <summary>
        /// The success
        /// </summary>
        Success,
        /// <summary>
        /// The alert
        /// </summary>
        Alert,
        /// <summary>
        /// The warning
        /// </summary>
        Warning,
        /// <summary>
        /// The error
        /// </summary>
        Error,
    }

    public enum CycleDurations
    {
        Quarterly,
        HalfYearly,
        Annually,
        ThreeYears
    }
    public enum CycleStart
    {
        Active,
        InActive,
        NA
    }

    public enum DurationSymbols
    {
        Q1,
        Q2,
        Q3,
        Q4,
        H1,
        H2,
        Y,
        Y3
    }

    public enum Status
    {
        Active = 1,
        InActive = 0
    }

    public enum Quarterly
    {
        Q1 = 1,
        Q2 = 2,
        Q3 = 3,
        Q4 = 4
    }

    public enum HalfYearly
    {
        H1 = 1,
        H2 = 2
    }

    public enum Annualy
    {
        Y1 = 1
    }

    public enum ThreeYears
    {
        Y3 = 1
    }

    public enum NotificationType
    {
        PasswordReset = 1,
        NewUserCreation = 2,
        RemovalOfUser = 3,
        OrganisationLeaderChange = 4,
        ChildOrganisationLeaderChange = 5,
        BulkUploadCsv = 6,
        NewRoleCreation = 7,
        RoleUpdation = 8,
        OrganisationSettingsChanges = 9,
        ProfileChanges = 10,
        UserOrganisationChange = 11,
        UserDeletionFromSystem = 12
    }

    public enum MessageTypeForNotifications
    {
        NotificationsMessages = 1
    }

    public enum TemplateCodes
    {
        NCU = 10,
        OLNU = 11,
        FP = 12,
        PRC = 13,
        OLO = 14,
        NLO = 15,
        UR = 16,
        BU = 17,
        OSC = 18,
        PI = 19,
        OC = 20,
        OCM = 21,
        OCML = 22,
        CTL = 23,
        TRV = 92,
        IADU =93
    }

    public enum CreateEditCodes
    {
        CR,
        ER
    }

    public enum ObjectiveTypes
    {
        Organisational,
        Private
    }

}
