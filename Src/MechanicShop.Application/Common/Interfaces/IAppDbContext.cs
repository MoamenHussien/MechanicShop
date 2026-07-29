using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    public DbSet<Customer> Customers { get; }
    public DbSet<Vehicle> Vehicles { get; }
    public DbSet<VehicleModel> VehicleModels { get; }
    public DbSet<VehicleMake> VehicleMakes { get; }
    public DbSet<Employee> Employees { get; }
    public DbSet<WorkOrder> WorkOrders { get; }
    public DbSet<Invoice> Invoices { get; }
    public DbSet<RepairTask> RepairTasks { get; }
    public DbSet<Part> Parts { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
