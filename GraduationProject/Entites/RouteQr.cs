namespace GraduationProject.Entites
{
    public class RouteQr
    {
        public int Id { get; set; }

        public int RouteId { get; set; }
        public Route Route { get; set; }

        public string Token { get; set; }
    }
}
