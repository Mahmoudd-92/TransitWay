namespace TransitWay.Entites
{
    public class Admin
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }

        public ICollection <Bus>? CreatedBus { get; set; }
            public ICollection <Driver>? CreatedDrivers { get; set; }
            public ICollection <Route>? CreatedRoutes { get; set; }
        public ICollection<Zone>? CreatedZones { get; set; }
        public ICollection<Station>? CreatedStations { get; set; }
        public ICollection<Alert>? CreatedAlerts { get; set; }
    }
}
