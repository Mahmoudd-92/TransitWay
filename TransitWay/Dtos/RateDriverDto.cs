namespace TransitWay.Dtos
{
    public class RateDriverDto
    {
        public int UserId { get; set; }
        public int DriverId { get; set; }
        public int TicketId { get; set; }
        public int Rate { get; set; }
        public string? Comment { get; set; }
    }
}
