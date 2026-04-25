using System.ComponentModel.DataAnnotations;

namespace TransitWay.Dtos
{
    public class CreateManualTicketByDriverDto
    {
        public int DriverId { get; set; }

        public int BusId { get; set; }
        public int NumberOfTickets { get; set; }

      public int RouteId { get; set; }
    }
}