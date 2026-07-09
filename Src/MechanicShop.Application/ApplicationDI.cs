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
            option.AddBehavior(typeof(ValidationBehavior<,>));
            option.AddBehavior(typeof(PerformanceBehavior<,>));
            option.AddBehavior(typeof(UnhandledExceptionBehavior<,>));
            option.AddBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}

