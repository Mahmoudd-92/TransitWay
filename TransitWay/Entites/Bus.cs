using System.Net.Sockets;

namespace TransitWay.Entites
{
    public class Bus
    {
        public int Id { get; set; }
        public string BusNumber { get; set; }
        public string PlateNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; } = "Active";
        public int Capacity { get; set; }
        public int RouteId { get; set; }
        public Route Route { get; set; }
        public Driver Driver { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<BusLocation> BusLocations { get; set; }
        public ICollection<Alert> alerts { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
        public int? CreatedByAdminId { get; set; }
        public Admin CreatedByAdmin { get; set; }
    }
}
