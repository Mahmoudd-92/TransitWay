using TransitWay.Entites;

public class Driver
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int? BusId { get; set; }  

    public string Phone { get; set; }

    public string Email { get; set; }
    public string Photo { get; set; }= null!;

    public string PasswordHash { get; set; }

    public string LicenseNumber { get; set; }

    public string Status { get; set; } = "Inactive";

    public Bus Bus { get; set; }

    public int? CreatedByAdminId { get; set; }

    public Admin CreatedByAdmin { get; set; }
}