using Microsoft.AspNetCore.Identity;

namespace DerpCode.API.Models.Entities;

public sealed class UserRole : IdentityUserRole<int>
{
    public User User { get; set; } = default!;

    public Role Role { get; set; } = default!;
}