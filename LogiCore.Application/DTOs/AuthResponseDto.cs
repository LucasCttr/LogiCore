using LogiCore.Application.Features.Auth;

namespace LogiCore.Application.DTOs;

public record AuthResponseDto(string Token, UserDto User);
