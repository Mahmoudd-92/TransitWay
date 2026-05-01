namespace TransitWay.Entites
{
    public static class SosAlertStatus
    {
        public const string PendingAdminReview = "PendingAdminReview";
        public const string AwaitingDriverCheckResponse = "AwaitingDriverCheckResponse";
        public const string DriverConfirmedSafe = "DriverConfirmedSafe";
        public const string EscalatedToAuthorities = "EscalatedToAuthorities";
    }
}