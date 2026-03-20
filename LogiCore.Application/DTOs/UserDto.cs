namespace LogiCore.Application.Features.Auth;

// This DTO is used to transfer package data between the application and the API layer.
public record UserDto(String Id, string FirstName, string LastName, string Email, DateTime CreatedAt);