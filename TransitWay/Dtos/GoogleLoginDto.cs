using System.ComponentModel.DataAnnotations;

namespace TransitWay.Dtos
{
    public class GoogleLoginDto
    {
        [Required]
        public string? IdToken { get; set; }
    }
}
