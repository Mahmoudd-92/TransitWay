namespace GraduationProject.Dtos
{
    public class CreateDriverDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public int BusId { get; set; }
        public string Password { get; set; }
    }
}
