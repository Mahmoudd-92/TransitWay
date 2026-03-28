using System.ComponentModel.DataAnnotations;

namespace TransitWay.Entites
{
    public class Station
    {
        public int Id { get; set; }

        public string Name { get; set; }
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }
        [Required]
        [Range(-90, 90)]
        public double Longitude { get; set; }

        public string Code { get; set; }
        public string Zone { get; set; }
        public string Status { get; set; } = "Active";
        public int RouteId { get; set; }
        public Route Route { get; set; }
        public int? CreatedByAdmin { get; set; }
        public Admin CreatedStations { get; set; }
    }
}
