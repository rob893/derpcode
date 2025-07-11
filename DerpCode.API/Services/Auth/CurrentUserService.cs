using System;
using System.Linq;
using System.Security.Claims;
using DerpCode.API.Extensions;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace DerpCode.API.Services.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    private User? userOverride;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        this.httpContextAccessor = httpContextAccessor;
    }

    public int UserId => userOverride?.Id != null ? userOverride.Id :
        this.User.TryGetUserId(out var id) ? id.Value : throw new InvalidOperationException("User ID claim missing or invalid.");

    public string UserName => userOverride?.UserName != null ? userOverride.UserName :
        this.User.TryGetUserName(out var userName) ? userName : throw new InvalidOperationException("User name claim missing or invalid.");

    public bool EmailVerified => userOverride?.EmailConfirmed != null ? userOverride.EmailConfirmed :
        this.User.TryGetEmailVerified(out var emailVerified) && emailVerified.Value;

    private ClaimsPrincipal User => this.httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No current user context.");

    public bool IsInRole(string role) => userOverride != null ? userOverride.UserRoles.Any(r => r.Role?.Name?.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false)
        : this.User.IsInRole(role);

    public bool IsUserAuthorizedForResource(IOwnedByUser<int> resource, bool isAdminAuthorized = true)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == resource.UserId);
    }

    public void SetOverrideUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        this.userOverride = user;
    }

    public void ClearOverrideUser()
    {
        this.userOverride = null;
    }
}