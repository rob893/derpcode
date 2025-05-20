using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DerpCode.API.Models.Entities;

public class User : IdentityUser<int>, IIdentifiable<int>
{
    [MaxLength(255)]
    public string? FirstName { get; set; }

    [MaxLength(255)]
    public string? LastName { get; set; }

    public DateTimeOffset Created { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];

    public List<UserRole> UserRoles { get; set; } = [];

    public List<LinkedAccount> LinkedAccounts { get; set; } = [];
}