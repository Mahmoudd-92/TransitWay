using System.ComponentModel.DataAnnotations;

namespace TransitWay.Entites
{
    public class RoutePoint
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public Route Route { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Order { get; set; }
    }
}
