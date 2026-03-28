namespace TransitWay.Entites
{
    public class Payment
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
