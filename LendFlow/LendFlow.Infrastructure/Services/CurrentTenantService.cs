using System;
using LendFlow.Application.Common.Interfaces;

namespace LendFlow.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    public Guid TenantId { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
