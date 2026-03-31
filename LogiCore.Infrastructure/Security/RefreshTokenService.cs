using LogiCore.Application.Common.Interfaces.Security;
using LogiCore.Infrastructure.Persistence;
using LogiCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LogiCore.Infrastructure.Security;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly LogiCoreDbContext _db;
    private readonly TimeSpan _defaultTtl = TimeSpan.FromDays(30);

    public RefreshTokenService(LogiCoreDbContext db)
    {
        _db = db;
    }

    public async Task<string> CreateRefreshTokenAsync(string userId, TimeSpan? ttl = null)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);

        var rt = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.Add(ttl ?? _defaultTtl),
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync();

        return token;
    }

    public async Task<ApplicationUser?> ValidateRefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var rt = await _db.RefreshTokens.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token && r.RevokedAt == null);

        if (rt == null)
            return null;

        if (rt.ExpiresAt < DateTime.UtcNow)
            return null;

        return rt.User;
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token && r.RevokedAt == null);
        if (rt == null)
            return;

        rt.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
