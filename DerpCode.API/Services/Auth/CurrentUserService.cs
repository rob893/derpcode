using System;
using System.Security.Claims;
using DerpCode.API.Extensions;
using DerpCode.API.Models;
using Microsoft.AspNetCore.Http;

namespace DerpCode.API.Services.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        this.httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => this.httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("No current user context.");

    public int UserId => this.User.TryGetUserId(out var id) ? id.Value : throw new InvalidOperationException("User ID claim missing or invalid.");

    public bool EmailVerified => this.User.TryGetEmailVerified(out var emailVerified) && emailVerified.Value;

    public bool IsInRole(string role) => this.User.IsInRole(role);

    public bool IsUserAuthorizedForResource(IOwnedByUser<int> resource, bool isAdminAuthorized = true)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return (isAdminAuthorized && this.User.IsAdmin()) || (this.User.TryGetUserId(out var userId) && userId == resource.UserId);
    }
}