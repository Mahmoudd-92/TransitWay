namespace TransitWay.Dtos
{
    public class BusResponseDto
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; }

        public int? DriverId { get; set; }
        public string DriverName { get; set; }
    }
}
