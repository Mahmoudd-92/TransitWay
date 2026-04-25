namespace TransitWay.Entites
{
    public class Complaint
    {
        public int Id { get; set; }

        public int BusId { get; set; }
        public int UserId { get; set; }

        public string OriginalImagePath { get; set; }
        public string ResultImagePath { get; set; }

        public bool ProblemDetected { get; set; }
      
        public string Category { get; set; } = "Unknown";
        public string? TextComplaint { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public Bus Bus { get; set; }
    }
}
