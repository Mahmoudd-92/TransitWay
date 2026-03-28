using System.ComponentModel.DataAnnotations;

namespace TransitWay.Entites
{
    public class BusLocation
    {
        public int Id { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; }
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }
        [Required]
        [Range(-90, 90)]
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt
        {
            get; set;
        }
    }
}
