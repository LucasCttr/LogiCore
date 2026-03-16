namespace LogiCore.Application.Commands;

public class CreatePackageCommand
{
    public required string TrackingNumber { get; init; } 
    public required string RecipientName { get; init; }
    public decimal Weight { get; init; }
}