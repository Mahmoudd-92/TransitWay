namespace TransitWay.Dtos
{
    public class UpdateUserProfileDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public IFormFile? Photo { get; set; }
    }
}