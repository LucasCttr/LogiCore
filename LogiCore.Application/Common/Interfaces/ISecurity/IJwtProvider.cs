using LogiCore.Application.DTOs;

namespace LogiCore.Application.Common.Interfaces.Security;

public interface IJwtProvider
{
    string CreateToken(string userId, string email, IEnumerable<KeyValuePair<string,string>>? additionalClaims = null);
}
