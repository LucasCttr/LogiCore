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
        // Validations
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(address)) throw new DomainException("La dirección es obligatoria.");
        if (string.IsNullOrWhiteSpace(phone)) throw new DomainException("El teléfono es obligatorio para la entrega.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("La ciudad es obligatoria para el ruteo.");
        if (string.IsNullOrWhiteSpace(province)) throw new DomainException("La provincia es obligatoria.");
        if (string.IsNullOrWhiteSpace(postalCode)) throw new DomainException("El código postal es crítico para las estadísticas.");
        if (string.IsNullOrWhiteSpace(dni)) throw new DomainException("El DNI es obligatorio para el seguro del paquete.");

        // Cleaning (trim)
        return new Recipient(
            name.Trim(),
            address.Trim(),
            phone.Trim(),
            city.Trim(),
            province.Trim(),
            postalCode.Trim(),
            dni.Trim(),
            string.IsNullOrWhiteSpace(floorApartment) ? null : floorApartment.Trim()
        );
    }
}