using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using LendFlow.Application.Common.Interfaces;

namespace LendFlow.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=LendFlow;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

        // Stub services for design-time only
        var services = new ServiceCollection();
        services.AddScoped<ICurrentTenantService, DesignTimeTenantService>();
        services.AddScoped<ICurrentUserService, DesignTimeUserService>();
        var provider = services.BuildServiceProvider();

        return new AppDbContext(
            optionsBuilder.Options,
            provider.GetRequiredService<ICurrentTenantService>(),
            provider.GetRequiredService<ICurrentUserService>());
    }
}

// Stub implementations — design time only, never registered in production
file class DesignTimeTenantService : ICurrentTenantService
{
    public Guid TenantId => Guid.Empty;
    public void SetTenant(Guid tenantId) { }
}

file class DesignTimeUserService : ICurrentUserService
{
    public string? UserId => "system";
}
