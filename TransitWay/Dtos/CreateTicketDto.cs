namespace TransitWay.Dtos
{
    public class CreateTicketDto
    {
        public int UserId { get; set; }
        public int BusId { get; set; }
        public int RouteId { get; set; }

        public decimal Price { get; set; }

        public int ValidHours { get; set; } = 2;
    }
}
