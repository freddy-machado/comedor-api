using Microsoft.AspNetCore.Identity;

namespace Comedor.Core.Entities;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true; // New field
}