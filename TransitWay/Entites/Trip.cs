namespace TransitWay.Entites
{
    public class Trip
    {
        public int Id { get; set; }

        public int BusId { get; set; }
        public Bus Bus { get; set; }

        public int RouteId { get; set; }
        public Route Route { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
