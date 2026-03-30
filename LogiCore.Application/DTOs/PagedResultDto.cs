namespace LogiCore.Application.DTOs;

public record PagedResultDto<T>(IEnumerable<T> Items, int Total);
