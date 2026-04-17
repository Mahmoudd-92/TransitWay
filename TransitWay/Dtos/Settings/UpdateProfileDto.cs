namespace TransitWay.Dtos.Settings
{
    public class UpdateProfileDto
    {
        public int UserId {  get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
