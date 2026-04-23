using System.ComponentModel.DataAnnotations;

namespace TransitWay.Dtos
{
    public class CreateManualTicketByDriverDto
    {
        [Required]
        public int DriverId { get; set; }

        [Required]
        public int BusId { get; set; }

        [Required]
        [Range(1, 24)]
        public int NumberOfTickets { get; set; }

        [Range(0.01, 1000000)]
        public decimal? Price { get; set; }

        [Range(1, 24)]
        public int ValidHours { get; set; } = 2;
    }
}