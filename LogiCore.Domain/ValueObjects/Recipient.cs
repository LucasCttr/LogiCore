using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.ValueObjects;

public sealed class Recipient
{
    public string Name { get; }
    public string? Address { get; }
    public string? Phone { get; }

    private Recipient(string name, string? address, string? phone)
    {
        Name = name;
        Address = address;
        Phone = phone;
    }

    public static Recipient Create(string name, string address, string phone)
{
    // Validation logic for recipient data
    if (string.IsNullOrWhiteSpace(name)) 
        throw new DomainException("Recipient name is required.");
        
    if (string.IsNullOrWhiteSpace(address)) 
        throw new DomainException("A delivery address is mandatory for the shipment.");
        
    if (string.IsNullOrWhiteSpace(phone)) 
        throw new DomainException("A contact phone number is required for delivery coordination.");

    // Clean up input data (e.g., trim whitespace)
    return new Recipient(name.Trim(), address.Trim(), phone.Trim());
}       
}
