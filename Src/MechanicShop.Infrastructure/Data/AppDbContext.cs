using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) :
                                            IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
    public DbSet<VehicleMake> VehicleMakes => Set<VehicleMake>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<RepairTask> RepairTasks => Set<RepairTask>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await DispatchDomainEventsAsync(cancellationToken);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker.Entries().Where(n => n.Entity is Entity entity && entity.DomainEvents.Count > 0)
                                                    .Select(n => (Entity)n.Entity).ToList();

        var DomainEvents = domainEntities.SelectMany(n => n.DomainEvents).ToList();

        foreach (var domain in DomainEvents)
        {
            await mediator.Publish(domain, cancellationToken);
        }

        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvent();
        }

    }
}
