using Microsoft.AspNetCore.Identity;

namespace Readiness_New.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
