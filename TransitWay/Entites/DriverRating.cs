namespace TransitWay.Entites
{
    public class DriverRating
    {
        public int Id { get; set; }

        public int DriverId { get; set; }
        public Driver Driver { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public int UserId { get; set; }

        public int Rate { get; set; } // من 1 لـ 5

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
