using Microsoft.EntityFrameworkCore;
using RCP_Project.Models;

public class LocalDbContext : DbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

    public DbSet<Individual> Individuals { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Software> Software { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Employee> Employees { get; set; } // Added Employee DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Individual>()
            .HasIndex(i => i.PESEL)
            .IsUnique();

        modelBuilder.Entity<Company>()
            .HasIndex(c => c.KRS)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}