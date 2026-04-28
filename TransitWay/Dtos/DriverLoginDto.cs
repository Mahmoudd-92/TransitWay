namespace TransitWay.Dtos
{
    public class DriverLoginDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? Bus { get; set; }
        public int? BusId { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; }
        public string? Photo { get; set; }
    }
}
