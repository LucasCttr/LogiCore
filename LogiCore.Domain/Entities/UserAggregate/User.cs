using System.Net.Mail;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relation: One user can have many packages
    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    // Relation: One-to-one with DriverDetails (only populated if user has Driver role)
    public virtual DriverDetails? DriverDetails { get; set; }
}