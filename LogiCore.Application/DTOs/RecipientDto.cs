namespace LogiCore.Application.DTOs;

public record RecipientDto(
	string Name,
	string? Address,
	string? Phone,
	string? FloorApartment,
	string? City,
	string? Province,
	string? PostalCode,
	string? Dni
);
