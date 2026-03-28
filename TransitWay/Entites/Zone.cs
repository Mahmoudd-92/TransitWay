namespace TransitWay.Entites
{
    public class Zone
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public ICollection<Route> Routes { get; set; }
        public int? CreatedByAdminId { get; set; }
        public Admin CreatedByAdmin { get; set; }
    }
}
