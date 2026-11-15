using Microsoft.Extensions.DependencyInjection;
using SharedCost.Application.Services;

namespace SharedCost.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IRuleManagementService, RuleManagementService>();
        services.AddScoped<IAllocationWorkflowService, AllocationWorkflowService>();

        return services;
    }
}
