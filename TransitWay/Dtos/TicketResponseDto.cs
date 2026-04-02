namespace TransitWay.Dtos
{
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string RouteName { get; set; }
        public int PassengerID { get; set; }
        public string BusPlate { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
