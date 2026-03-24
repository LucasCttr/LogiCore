namespace LogiCore.Application.DTOs;

public record PublicHistoryEntryDto(string Status, DateTime OccurredAt);

public record PackagePublicHistoryDto(string TrackingNumber, IEnumerable<PublicHistoryEntryDto> History);
