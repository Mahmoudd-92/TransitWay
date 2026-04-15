namespace TransitWay.Dtos
{
    public class UserTripResponseDto
    {
        public int Id { get; set; }
        public string BusNumber { get; set; }

        public string DistanceToStationKm { get; set; }

        public string TripDistanceKm { get; set; }

        public string EstimatedArrivalTime { get; set; }
    }
}
