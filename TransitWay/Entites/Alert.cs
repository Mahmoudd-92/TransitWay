namespace TransitWay.Entites
{
    public class Alert
    {
        public int Id { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; }

        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SituationPhotoPath { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool NeedReplacementBus { get; set; }
        public bool IsSos { get; set; }
        public DateTime? SafetyCheckStartedAt { get; set; }
        public bool? DriverIsOkay { get; set; }
        public string Status { get; set; } = SosAlertStatus.PendingAdminReview;
        public DateTime CreatedAt { get; set; }
        public int? CreatedByAdmin { get; set; }
        public Admin CreatedAlerts { get; set; }
    }
}
