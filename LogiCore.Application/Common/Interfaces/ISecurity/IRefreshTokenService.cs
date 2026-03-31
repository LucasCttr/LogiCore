using LogiCore.Domain.Entities;

namespace LogiCore.Application.Common.Interfaces.Security;

public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(string userId, TimeSpan? ttl = null);
    Task<ApplicationUser?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}
