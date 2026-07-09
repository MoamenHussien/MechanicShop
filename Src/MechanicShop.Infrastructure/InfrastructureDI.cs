using System.Text;
using MechanicShop.Infrastructure.Identity;
using MechanicShop.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Hybrid;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IOptions<JwtSettings> jwt,
                                                                                           IConfiguration config)
    {
        // System Core
        services.AddSingleton(TimeProvider.System);
        services.AddOptions<AppSettings>().BindConfiguration(AppSettings.Name).ValidateOnStart();
        services.AddOptions<JwtSettings>().BindConfiguration(JwtSettings.Name).ValidateOnStart();
        services.AddOptions<MailSettings>().BindConfiguration(MailSettings.Name).ValidateOnStart();
        // BackGround Services
        services.AddHostedService<OverdueBookingCleanupService>();
        // Application Services
        services.AddScoped<IWorkOrderNotifier, SignalRWorkOrderNotifier>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWorkOrderPolicy, WorkOrderPolicy>();
        services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
        services.AddHttpContextAccessor();
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        
        // (EF Core & Data Access)
        var connectionString = config.GetConnectionString("DefaultConnection");
        ArgumentNullException.ThrowIfNull(connectionString);
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        // Security & Identity
        services.AddScoped<IAuthorizationHandler, LaborAssignedRequirementHandler>();
        services.AddAuthorizationBuilder().AddPolicy("SelfScopedWorkOrderAccess", policy => 
                                                                  policy.AddRequirements(new LaborAssignedRequirement()));
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Value.Issuer,
                ValidAudience = jwt.Value.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Value.SecretKey!)),
            };
        });

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequiredUniqueChars = 1;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;

        }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

        // Caching
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10), // L2, L3
            LocalCacheExpiration = TimeSpan.FromSeconds(30), // L1
        });

        return services;
    }
}