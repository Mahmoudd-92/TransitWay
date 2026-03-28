using Microsoft.EntityFrameworkCore;
using GraduationProject.Entites;
using Route = GraduationProject.Entites.Route;

namespace GraduationProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<BusLocation> BusLocations { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<RoutePoint> RoutePoints { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<RouteQr> RouteQrs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Route)
                .WithMany()
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Bus)
                .WithMany(b => b.Tickets)
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Restrict);

           

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Bus)
                .WithMany(b => b.Complaints)
                .HasForeignKey(c => c.BusId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<Station>()
                .HasOne(s => s.Route)
                .WithMany(r => r.Stations)
                .HasForeignKey(s => s.RouteId);

        

            modelBuilder.Entity<Bus>()
                .HasOne(b => b.Route)
                .WithMany(r => r.Buses)
                .HasForeignKey(b => b.RouteId);

           

            modelBuilder.Entity<Route>()
                .HasOne(r => r.Zone)
                .WithMany(z => z.Routes)
                .HasForeignKey(r => r.ZoneId);

   

            modelBuilder.Entity<BusLocation>()
                .HasOne(bl => bl.Bus)
                .WithMany(b => b.BusLocations)
                .HasForeignKey(bl => bl.BusId);

          

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            

            modelBuilder.Entity<RoutePoint>()
                .HasOne(rp => rp.Route)
                .WithMany(r => r.RoutePoints)
                .HasForeignKey(rp => rp.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

        

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.Bus)
                .WithMany(b => b.alerts)
                .HasForeignKey(a => a.BusId)
                .OnDelete(DeleteBehavior.Restrict);

           

            modelBuilder.Entity<Bus>()
                .HasOne(b => b.CreatedByAdmin)
                .WithMany(a => a.CreatedBus)
                .HasForeignKey(b => b.CreatedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<Route>()
                .HasOne(r => r.CreatedByAdmin)
                .WithMany(a => a.CreatedRoutes)
                .HasForeignKey(r => r.CreatedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);


            modelBuilder.Entity<Station>()
                .HasOne(s => s.CreatedStations)
                .WithMany(a => a.CreatedStations)
                .HasForeignKey(s => s.CreatedByAdmin)
                .OnDelete(DeleteBehavior.SetNull);

          

            modelBuilder.Entity<Zone>()
                .HasOne(z => z.CreatedByAdmin)
                .WithMany(a => a.CreatedZones)
                .HasForeignKey(z => z.CreatedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            

            modelBuilder.Entity<Alert>()
                .HasOne(a => a.CreatedAlerts)
                .WithMany()
                .HasForeignKey(a => a.CreatedByAdmin)
                .OnDelete(DeleteBehavior.SetNull);

          

            modelBuilder.Entity<Driver>()
                .HasOne(d => d.Bus)
                .WithOne(b => b.Driver)
                .HasForeignKey<Driver>(d => d.BusId);
   
            modelBuilder.Entity<Ticket>()
                .Property(t => t.Status)
                .HasConversion<string>();
    }
    }
}