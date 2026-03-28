namespace TransitWay.Dtos
{
    public class CreateStationDto
    {
        public string Name { get; set; }
        public string Zone { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RouteId { get; set; }
    }
}
