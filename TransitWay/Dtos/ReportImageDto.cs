namespace TransitWay.Dtos
{
    public class ReportImageDto
    {
        public int BusId { get; set; }
        public int UserId { get; set; }
        public IFormFile Image { get; set; }
        public string? TextComplaint { get; set; }
    }
}
