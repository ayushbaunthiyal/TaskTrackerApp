using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Interfaces.Services;
using TaskTracker.Application.Services;

namespace TaskTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuditService, AuditService>();

        // Register validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
