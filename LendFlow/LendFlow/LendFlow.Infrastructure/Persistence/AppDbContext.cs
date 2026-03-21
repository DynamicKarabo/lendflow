using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Common;
using LendFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LendFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _userService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService tenantService,
        ICurrentUserService userService) : base(options)
    {
        _tenantService = tenantService;
        _userService = userService;
    }

    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Multi-tenant global query filters
        modelBuilder.Entity<Applicant>()
            .HasQueryFilter(a => a.TenantId == _tenantService.TenantId);

        modelBuilder.Entity<LoanApplication>()
            .HasQueryFilter(l => l.TenantId == _tenantService.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = _userService.UserId ?? "system";
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = _userService.UserId ?? "system";
            }
        }
        return await base.SaveChangesAsync(ct);
    }
}
