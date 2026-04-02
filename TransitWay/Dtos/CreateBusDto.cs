namespace TransitWay.Dtos
{
    public class CreateBusDto
    {
        public string? BusNumber { get; set; }
        public string PlateNumber { get; set; }
        public string LicenseNumber { get; set; }
        public int Capacity { get; set; }
        public int? RouteId { get; set; }
    }
}
