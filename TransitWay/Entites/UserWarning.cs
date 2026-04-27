namespace TransitWay.Entites
{
    public class UserWarning
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public ActionType Type { get; set; } // <-- Add this property
        public User User { get; set; }

    }
}
