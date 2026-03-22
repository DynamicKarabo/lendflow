using LendFlow.Application.Common.Interfaces;

namespace LendFlow.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var tenantClaim = context.User.Claims.FirstOrDefault(c => c.Type == "tenant_id");
        if (tenantClaim == null || !Guid.TryParse(tenantClaim.Value, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Tenant ID claim is missing or invalid." });
            return;
        }

        tenantService.SetTenant(tenantId);
        await _next(context);
    }
}
