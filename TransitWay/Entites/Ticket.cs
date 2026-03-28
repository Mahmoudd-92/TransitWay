namespace TransitWay.Entites
{
    public class Ticket
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; }

        public int RouteId { get; set; }
        public Route Route { get; set; }

        public string QRToken { get; set; }

        public decimal Price { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Valid;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpireAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }
    }
}