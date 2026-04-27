namespace TransitWay.Entites
{
    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
    }
}
