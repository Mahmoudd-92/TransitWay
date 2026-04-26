namespace TransitWay.Dtos
{
    public class DriverRegisterDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string LicenseNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public IFormFile Photo { get; set; }

    }
}