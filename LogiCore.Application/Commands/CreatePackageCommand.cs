namespace LogiCore.Application.Commands;

public class CreatePackageCommand
{
    public string TrackingNumber { get; init; }
    public string RecipientName { get; init; }
    public decimal Weight { get; init; }
}