using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SharedCost.Domain.Entities;

namespace SharedCost.Application.Common.Interfaces;

public interface IPhanBoChiPhiDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<NguoiDung> NguoiDungs { get; }
    DbSet<CostPool> CostPools { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<Target> Targets { get; }
    DbSet<DriverValue> DriverValues { get; }
    DbSet<AllocationRule> AllocationRules { get; }
    DbSet<RuleTarget> RuleTargets { get; }
    DbSet<RuleVersion> RuleVersions { get; }
    DbSet<RuleVersionTarget> RuleVersionTargets { get; }
    DbSet<Simulation> Simulations { get; }
    DbSet<SimulationResult> SimulationResults { get; }
    DbSet<AllocationRun> AllocationRuns { get; }
    DbSet<AllocationResult> AllocationResults { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Lock> Locks { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
