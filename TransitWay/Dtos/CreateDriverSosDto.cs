using Microsoft.AspNetCore.Http;

namespace TransitWay.Dtos
{
    public class CreateDriverSosDto
    {
        public int DriverId { get; set; }
        public string SituationType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool NeedReplacementBus { get; set; }
        public IFormFile? Photo { get; set; }
    }

    public class DriverSafetyCheckResponseDto
    {
        public bool IsOkay { get; set; }
        public string? AdditionalDetails { get; set; }
    }
}