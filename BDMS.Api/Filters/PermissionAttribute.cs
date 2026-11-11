using System.Security.Claims;
using BDMS.Database.AppDbContextModels;
using BDMS.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class PermissionAttribute : TypeFilterAttribute
{
    public PermissionAttribute(params string[] permissions) : base(typeof(PermissionFilter))
    {
        Arguments = new object[] { permissions };
    }
}

public class PermissionFilter : IAsyncActionFilter
{
    private readonly string[] _requiredPermissions;
    private readonly AppDbContext _context;

    public PermissionFilter(string[] requiredPermissions, AppDbContext context)
    {
        _requiredPermissions = requiredPermissions;
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 1. Admin bypass - admin has all permissions
        if (user.IsInRole("admin"))
        {
            await next();
            return;
        }

        // 3. Fallback: Live DB lookup (catches mid-session permission updates or tokens without claims)
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdClaim))
        {
            try
            {
                var decryptedId = EncryptionHelper.Decrypt(userIdClaim);
                if (int.TryParse(decryptedId, out var userId))
                {
                    var hasDbPermission = await _context.Users
                        .Where(u => u.Id == userId)
                        .SelectMany(u => u.Role.RolePermissions)
                        .AnyAsync(rp => _requiredPermissions.Contains(rp.Permission.Name));

                    if (hasDbPermission)
                    {
                        await next();
                        return;
                    }
                }
            }
            catch
            {
                // Decryption failure falls through to forbidden
            }
        }

        context.Result = new ForbidResult();
    }
}

