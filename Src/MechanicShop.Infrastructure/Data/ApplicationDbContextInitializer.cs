using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
public class ApplicationDbContextInitializer(UserManager<AppUser> user, RoleManager<IdentityRole<Guid>> roleManager,
                                            AppDbContext context, ILogger<ApplicationDbContextInitializer> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            // await context.Database.EnsureCreatedAsync();
            await context.Database.MigrateAsync();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed To Create The DataBase");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedDataAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed During Seeding The Data");
            throw;
        }
    }

    private async Task TrySeedDataAsync()
    {
        var ManagerRole = new IdentityRole<Guid>(Role.Manager.ToString());
        if (!await roleManager.Roles.AnyAsync(n => n.Name == ManagerRole.Name))
        {
            await roleManager.CreateAsync(ManagerRole);
        }

        var LaborRole = new IdentityRole<Guid>(Role.Labor.ToString());
        if (!await roleManager.Roles.AnyAsync(n => n.Name == LaborRole.Name))
        {
            await roleManager.CreateAsync(LaborRole);
        }

        var SystemManager = new AppUser
        {
            Id = "19a59129-6c20-417a-834d-11a208d32d96".ToGuid().Value,
            Email = "pm@localhost",
            UserName = "pm@localhost",
            EmailConfirmed = true
        };

        if (!await user.Users.AnyAsync(n => n.Email == SystemManager.Email))
        {
            await user.CreateAsync(SystemManager, SystemManager.Email);
            await user.AddToRolesAsync(SystemManager, [ManagerRole.Name!]);
        }

        var Labor1 = new AppUser
        {
            Id = "b6327240-0aea-46fc-863a-777fc4e42560".ToGuid().Value,
            Email = "john.labor@localhost",
            UserName = "john.labor@localhost",
            EmailConfirmed = true
        };

        if (!await user.Users.AnyAsync(n => n.Email == Labor1.Email))
        {
            await user.CreateAsync(Labor1, Labor1.Email);
            await user.AddToRolesAsync(Labor1, [LaborRole.Name!]);
        }

        var Labor2 = new AppUser
        {
            Id = "8104AB20-26C2-4651-B1DE-C0BAF04DBBD9".ToGuid().Value,
            Email = "peter.labor@localhost",
            UserName = "peter.labor@localhost",
            EmailConfirmed = true
        };

        if (!await user.Users.AnyAsync(n => n.Email == Labor2.Email))
        {
            await user.CreateAsync(Labor2, Labor2.Email);
            await user.AddToRolesAsync(Labor2, [LaborRole.Name!]);
        }


        if (!await context.Employees.AnyAsync())
        {
            await context.Employees.AddRangeAsync([
                Employee.Create(SystemManager.Id, "Admin", "Manager").Value,
                Employee.Create(Labor1.Id, "john", "M.").Value,
                Employee.Create(Labor2.Id, "Peter", "R.").Value
            ]);
        }

        if (!await context.VehicleMakes.AnyAsync())
        {
            await context.VehicleMakes.AddRangeAsync([
                VehicleMake.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Hyundai", [VehicleModel.Create(Guid.Parse("11111111-1111-1111-1111-222222222221"), "Accent").Value, VehicleModel.Create(Guid.Parse("11111111-1111-1111-1111-222222222222"), "Elantra").Value, VehicleModel.Create(Guid.Parse("11111111-1111-1111-1111-222222222223"), "Tucson").Value, VehicleModel.Create(Guid.Parse("11111111-1111-1111-1111-222222222224"), "Creta").Value, VehicleModel.Create(Guid.Parse("11111111-1111-1111-1111-222222222225"), "Santa Fe").Value]).Value,
                VehicleMake.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Nissan", [VehicleModel.Create(Guid.Parse("22222222-2222-2222-2222-333333333331"), "Sunny").Value, VehicleModel.Create(Guid.Parse("22222222-2222-2222-2222-333333333332"), "Sentra").Value, VehicleModel.Create(Guid.Parse("22222222-2222-2222-2222-333333333333"), "Qashqai").Value, VehicleModel.Create(Guid.Parse("22222222-2222-2222-2222-333333333334"), "X-Trail").Value, VehicleModel.Create(Guid.Parse("22222222-2222-2222-2222-333333333335"), "Patrol").Value]).Value,
                VehicleMake.Create(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Toyota", [VehicleModel.Create(Guid.Parse("33333333-3333-3333-3333-444444444441"), "Corolla").Value, VehicleModel.Create(Guid.Parse("33333333-3333-3333-3333-444444444442"), "Yaris").Value, VehicleModel.Create(Guid.Parse("33333333-3333-3333-3333-444444444443"), "Fortuner").Value, VehicleModel.Create(Guid.Parse("33333333-3333-3333-3333-444444444444"), "Camry").Value, VehicleModel.Create(Guid.Parse("33333333-3333-3333-3333-444444444445"), "Land Cruiser").Value]).Value,
                VehicleMake.Create(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Kia", [VehicleModel.Create(Guid.Parse("44444444-4444-4444-4444-555555555551"), "Cerato").Value, VehicleModel.Create(Guid.Parse("44444444-4444-4444-4444-555555555552"), "Sportage").Value, VehicleModel.Create(Guid.Parse("44444444-4444-4444-4444-555555555553"), "Carens").Value, VehicleModel.Create(Guid.Parse("44444444-4444-4444-4444-555555555554"), "Sorento").Value, VehicleModel.Create(Guid.Parse("44444444-4444-4444-4444-555555555555"), "Picanto").Value]).Value,
                VehicleMake.Create(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Chery", [VehicleModel.Create(Guid.Parse("55555555-5555-5555-5555-666666666661"), "Arrizo 5").Value, VehicleModel.Create(Guid.Parse("55555555-5555-5555-5555-666666666662"), "Tiggo 3").Value, VehicleModel.Create(Guid.Parse("55555555-5555-5555-5555-666666666663"), "Tiggo 4 Pro").Value, VehicleModel.Create(Guid.Parse("55555555-5555-5555-5555-666666666664"), "Tiggo 7 Pro").Value, VehicleModel.Create(Guid.Parse("55555555-5555-5555-5555-666666666665"), "Tiggo 8 Pro").Value]).Value,
                VehicleMake.Create(Guid.Parse("66666666-6666-6666-6666-666666666666"), "MG", [VehicleModel.Create(Guid.Parse("66666666-6666-6666-6666-777777777771"), "MG 5").Value, VehicleModel.Create(Guid.Parse("66666666-6666-6666-6666-777777777772"), "MG 6").Value, VehicleModel.Create(Guid.Parse("66666666-6666-6666-6666-777777777773"), "ZS").Value, VehicleModel.Create(Guid.Parse("66666666-6666-6666-6666-777777777774"), "HS").Value, VehicleModel.Create(Guid.Parse("66666666-6666-6666-6666-777777777775"), "RX5").Value]).Value,
                VehicleMake.Create(Guid.Parse("77777777-7777-7777-7777-777777777777"), "Chevrolet", [VehicleModel.Create(Guid.Parse("77777777-7777-7777-7777-888888888881"), "Optra").Value, VehicleModel.Create(Guid.Parse("77777777-7777-7777-7777-888888888882"), "Aveo").Value, VehicleModel.Create(Guid.Parse("77777777-7777-7777-7777-888888888883"), "Captiva").Value, VehicleModel.Create(Guid.Parse("77777777-7777-7777-7777-888888888884"), "Malibu").Value, VehicleModel.Create(Guid.Parse("77777777-7777-7777-7777-888888888885"), "Trailblazer").Value]).Value,
                VehicleMake.Create(Guid.Parse("88888888-8888-8888-8888-888888888888"), "Renault", [VehicleModel.Create(Guid.Parse("88888888-8888-8888-8888-999999999991"), "Logan").Value, VehicleModel.Create(Guid.Parse("88888888-8888-8888-8888-999999999992"), "Sandero").Value, VehicleModel.Create(Guid.Parse("88888888-8888-8888-8888-999999999993"), "Megane").Value, VehicleModel.Create(Guid.Parse("88888888-8888-8888-8888-999999999994"), "Duster").Value, VehicleModel.Create(Guid.Parse("88888888-8888-8888-8888-999999999995"), "Koleos").Value]).Value,
                VehicleMake.Create(Guid.Parse("99999999-9999-9999-9999-999999999999"), "Peugeot", [VehicleModel.Create(Guid.Parse("99999999-9999-9999-9999-000000000001"), "301").Value, VehicleModel.Create(Guid.Parse("99999999-9999-9999-9999-000000000002"), "3008").Value, VehicleModel.Create(Guid.Parse("99999999-9999-9999-9999-000000000003"), "5008").Value, VehicleModel.Create(Guid.Parse("99999999-9999-9999-9999-000000000004"), "508").Value, VehicleModel.Create(Guid.Parse("99999999-9999-9999-9999-000000000005"), "208").Value]).Value,
                VehicleMake.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "BMW", [VehicleModel.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbbb1"), "116i").Value, VehicleModel.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbbb2"), "320i").Value, VehicleModel.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbbb3"), "520i").Value, VehicleModel.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbbb4"), "X3").Value, VehicleModel.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbbb5"), "X5").Value, VehicleModel.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-bbbbbbbbbbb6"), "M4").Value]).Value,
                VehicleMake.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Mercedes-Benz", [VehicleModel.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-ccccccccccc1"), "A-Class").Value, VehicleModel.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-ccccccccccc2"), "C-Class").Value, VehicleModel.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-ccccccccccc3"), "E-Class").Value, VehicleModel.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-ccccccccccc4"), "S-Class").Value, VehicleModel.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-ccccccccccc5"), "GLC").Value, VehicleModel.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-ccccccccccc6"), "GLE").Value]).Value,
                VehicleMake.Create(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Audi", [VehicleModel.Create(Guid.Parse("cccccccc-cccc-cccc-cccc-ddddddddddd1"), "A3").Value, VehicleModel.Create(Guid.Parse("cccccccc-cccc-cccc-cccc-ddddddddddd2"), "A4").Value, VehicleModel.Create(Guid.Parse("cccccccc-cccc-cccc-cccc-ddddddddddd3"), "A6").Value, VehicleModel.Create(Guid.Parse("cccccccc-cccc-cccc-cccc-ddddddddddd4"), "Q3").Value, VehicleModel.Create(Guid.Parse("cccccccc-cccc-cccc-cccc-ddddddddddd5"), "Q5").Value]).Value,
                VehicleMake.Create(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Volkswagen", [VehicleModel.Create(Guid.Parse("dddddddd-dddd-dddd-dddd-eeeeeeeeeee1"), "Golf").Value, VehicleModel.Create(Guid.Parse("dddddddd-dddd-dddd-dddd-eeeeeeeeeee2"), "Passat").Value, VehicleModel.Create(Guid.Parse("dddddddd-dddd-dddd-dddd-eeeeeeeeeee3"), "Jetta").Value, VehicleModel.Create(Guid.Parse("dddddddd-dddd-dddd-dddd-eeeeeeeeeee4"), "Tiguan").Value, VehicleModel.Create(Guid.Parse("dddddddd-dddd-dddd-dddd-eeeeeeeeeee5"), "Touareg").Value]).Value,
                VehicleMake.Create(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Skoda", [VehicleModel.Create(Guid.Parse("eeeeeeee-eeee-eeee-eeee-fffffffffff1"), "Octavia").Value, VehicleModel.Create(Guid.Parse("eeeeeeee-eeee-eeee-eeee-fffffffffff2"), "Superb").Value, VehicleModel.Create(Guid.Parse("eeeeeeee-eeee-eeee-eeee-fffffffffff3"), "Kodiaq").Value, VehicleModel.Create(Guid.Parse("eeeeeeee-eeee-eeee-eeee-fffffffffff4"), "Kamiq").Value, VehicleModel.Create(Guid.Parse("eeeeeeee-eeee-eeee-eeee-fffffffffff5"), "Karoq").Value]).Value
            ]);
        }

        if (!await context.RepairTasks.AnyAsync())
        {
            await context.RepairTasks.AddRangeAsync([
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000001"), "Engine Oil Change", 50.00m, RepairDurationInMinutes.Min60, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-100000000001"), 25.00m, "Engine Oil", 1).Value, Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-100000000002"), 10.00m, "Oil Filter", 1).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000002"), "Brake Replacement", 150.00m, RepairDurationInMinutes.Min90, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-200000000001"), 40.00m, "Brake Pads", 2).Value, Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-200000000002"), 15.00m, "Brake Fluid", 1).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000003"), "Tire Rotation", 30.00m, RepairDurationInMinutes.Min45, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-300000000001"), 5.00m, "Tire Valve", 4).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000004"), "Battery Replacement", 70.00m, RepairDurationInMinutes.Min30, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-400000000001"), 120.00m, "Car Battery", 1).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000005"), "Wheel Alignment", 80.00m, RepairDurationInMinutes.Min60, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-500000000001"), 5.00m, "Alignment Shim Kit", 4).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000006"), "Air Conditioning Recharge", 100.00m, RepairDurationInMinutes.Min30, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-600000000001"), 50.00m, "Refrigerant", 1).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000007"), "Spark Plug Replacement", 90.00m, RepairDurationInMinutes.Min60, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-700000000001"), 10.00m, "Spark Plug", 4).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000008"), "Engine Diagnostic", 120.00m, RepairDurationInMinutes.Min120, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-800000000001"), 20.00m, "Smoke Leak Detector Fluid Cartridge", 1).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000009"), "Timing Belt Replacement", 200.00m, RepairDurationInMinutes.Min120, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-900000000001"), 75.00m, "Timing Belt", 1).Value]).Value,
                RepairTask.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-000000000010"), "Transmission Fluid Change", 100.00m, RepairDurationInMinutes.Min45, [Part.Create(Guid.Parse("ffffffff-ffff-ffff-ffff-a00000000001"), 60.00m, "Transmission Fluid", 1).Value]).Value
            ]);
        }

        if (!context.Customers.Any())
        {
            List<Vehicle> vehiclesCustomer1 = [
                        Vehicle.Create(id: Guid.Parse("61401e63-007b-4b1c-8914-9eb6e9bd95c5"), VehicleModelId:Guid.Parse("33333333-3333-3333-3333-444444444444") , Year: 2020, LicensePlate: "ABC123").Value,
                        Vehicle.Create(id: Guid.Parse("13c80914-41ad-4d46-b7bb-60f6c89ad01e"), VehicleModelId:Guid.Parse("33333333-3333-3333-3333-444444444441") , Year: 2020, LicensePlate: "ABC321").Value,
                    ];
            List<Vehicle> vehiclesCustomer2 = [
                        Vehicle.Create(id: Guid.Parse("a04f329d-0f5a-46a0-beae-699c034ae401"), VehicleModelId:Guid.Parse("11111111-1111-1111-1111-222222222221") , Year: 2021, LicensePlate: "DEF789").Value,
                        Vehicle.Create(id: Guid.Parse("cf60e95b-5752-4c26-aa07-31a34164606c"), VehicleModelId:Guid.Parse("eeeeeeee-eeee-eeee-eeee-fffffffffff1") , Year: 2019, LicensePlate: "GHI012").Value,
                    ];

            context.Customers.AddRange(
            [
                Customer.Create(id: Guid.Parse("f522bbe5-e3b1-4e2c-a8a3-c41550dcf39d"), name: "John Doe", phone: "123456789", email: "john.doe@localhost", vehicles: vehiclesCustomer1).Value,
                Customer.Create(id: Guid.Parse("73a04dd3-c81a-4a54-9882-ef1017eb192d"), name: "Sarah Peter", phone: "987654321", email: "sarah.peter@localhost", vehicles: vehiclesCustomer2).Value,
            ]);
        }


        await context.SaveChangesAsync();
    }
}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
            await initialiser.InitializeAsync();
            await initialiser.SeedAsync();
        }
    }
}

