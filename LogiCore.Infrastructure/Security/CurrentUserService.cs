using System.Security.Claims;
using LogiCore.Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace LogiCore.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId
    {
        get
        {
            if (User == null) return null;
            // prefer NameIdentifier, fallback to 'sub'
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return id;
        }
    }

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
