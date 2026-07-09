using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ApplicationDI
{
    public static IServiceCollection AddApplication (this IServiceCollection  services)
    {
        var assembly = typeof(ApplicationDI).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(option =>
        {
            option.RegisterServicesFromAssembly(assembly);
            option.AddOpenBehavior(typeof(ValidationBehavior<,>));
            option.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            option.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            option.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}

