using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedCost.Application.Common.Interfaces;
using SharedCost.Infrastructure.Persistence;

namespace SharedCost.Infrastructure;

public static class DependencyInjection
{
    private const string DefaultLocalConnection =
        @"Server=UNICONR\SQLEXPRESS;Database=PhanBoChiPhi;Trusted_Connection=True;TrustServerCertificate=True;";

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultLocalConnection;
        }

        services.AddDbContext<PhanBoChiPhiDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPhanBoChiPhiDbContext>(provider =>
            provider.GetRequiredService<PhanBoChiPhiDbContext>());

        return services;
    }
}
