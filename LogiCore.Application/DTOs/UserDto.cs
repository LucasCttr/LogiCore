namespace LogiCore.Application.DTOs;

public record UserDto(
    string Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    bool EmailConfirmed,
    bool IsActive,
    IEnumerable<string>? Roles,
    DateTime CreatedAt
);
