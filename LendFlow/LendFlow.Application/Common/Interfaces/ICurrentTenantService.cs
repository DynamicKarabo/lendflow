using System;

namespace LendFlow.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid TenantId { get; }
    void SetTenant(Guid tenantId);
}
