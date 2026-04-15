namespace LogiCore.Application.DTOs;

public record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool IsActive { get; init; }
    public IEnumerable<string>? Roles { get; init; }
    public DateTime CreatedAt { get; init; }
}
