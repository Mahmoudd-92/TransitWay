using TransitWay.Entites;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string PasswordHash { get; set; }
    public decimal Balance { get; set; }
    public string Photo { get; set; } = null!;
    public int tickets { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }
    public DateTime? BannedAt { get; set; }
    public ICollection<Ticket> Tickets { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public ICollection<Complaint> Complaints { get; set; }
}