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

    public DateTimeOffset? LastLogin { get; set; }

    public DateTimeOffset LastPasswordChange { get; set; }

    public DateTimeOffset LastEmailChange { get; set; }

    public DateTimeOffset LastUsernameChange { get; set; }

    public DateTimeOffset? LastEmailConfirmationSent { get; set; }

    public int Experience { get; set; }

    public List<ProblemSubmission> ProblemSubmissions { get; set; } = [];
}