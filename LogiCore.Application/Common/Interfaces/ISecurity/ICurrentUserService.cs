using System;

namespace LogiCore.Application.Common.Interfaces.Security;

public interface ICurrentUserService
{
    /// Current authenticated user's Id (from JWT 'sub' or NameIdentifier claim) or null.
    string? UserId { get; }

    /// Current authenticated user's email claim or null.
    string? Email { get; }

    /// Whether the current request is authenticated.
    bool IsAuthenticated { get; }
}
