using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DerpCode.API.Models.Entities;

public sealed class User : IdentityUser<int>, IIdentifiable<int>
{
    public DateTimeOffset Created { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];

    public List<UserRole> UserRoles { get; set; } = [];

    public List<LinkedAccount> LinkedAccounts { get; set; } = [];
}