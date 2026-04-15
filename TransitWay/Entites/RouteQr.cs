namespace TransitWay.Entites
{
    public class RouteQr
    {
        public int Id { get; set; }

        public int RouteId { get; set; }
        public int BusId { get; set; }
        public Bus Bus { get; set; }
        public Route Route { get; set; }

        public string Token { get; set; }
    }
}
