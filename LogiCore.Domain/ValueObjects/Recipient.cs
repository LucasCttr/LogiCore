using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.ValueObjects;

public sealed class Recipient
{
    public string Name { get; }
    public string Address { get; }
    public string Phone { get; }
    public string City { get; }
    public string Province { get; }
    public string PostalCode { get; }
    public string Dni { get; }
    public string? FloorApartment { get; }

    private Recipient(
        string name, 
        string address, 
        string phone, 
        string city, 
        string province, 
        string postalCode, 
        string dni, 
        string? floorApartment)
    {
        Name = name;
        Address = address;
        Phone = phone;
        City = city;
        Province = province;
        PostalCode = postalCode;
        Dni = dni;
        FloorApartment = floorApartment;
    }

    public static Recipient Create(
        string name, 
        string address, 
        string phone, 
        string? floorApartment, // Optional
        string city, 
        string province, 
        string postalCode, 
        string dni)
    {
        // Relaxed validations: allow creation with partial data (frontend may provide only address strings).
        var finalName = string.IsNullOrWhiteSpace(name) ? "Unknown" : name.Trim();
        var finalAddress = string.IsNullOrWhiteSpace(address) ? string.Empty : address.Trim();
        var finalPhone = string.IsNullOrWhiteSpace(phone) ? string.Empty : phone.Trim();
        var finalCity = string.IsNullOrWhiteSpace(city) ? string.Empty : city.Trim();
        var finalProvince = string.IsNullOrWhiteSpace(province) ? string.Empty : province.Trim();
        var finalPostalCode = string.IsNullOrWhiteSpace(postalCode) ? string.Empty : postalCode.Trim();
        var finalDni = string.IsNullOrWhiteSpace(dni) ? string.Empty : dni.Trim();
        var finalFloor = string.IsNullOrWhiteSpace(floorApartment) ? null : floorApartment.Trim();

        return new Recipient(
            finalName,
            finalAddress,
            finalPhone,
            finalCity,
            finalProvince,
            finalPostalCode,
            finalDni,
            finalFloor
        );
    }
}