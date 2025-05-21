using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using DerpCode.API.Constants;
using Microsoft.AspNetCore.JsonPatch;

namespace DerpCode.API.Extensions;

public static class UtilityExtensions
{
    public static bool IsValid<T>(this JsonPatchDocument<T> patchDoc, [NotNullWhen(false)] out List<string>? errors)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(patchDoc);

        errors = null;

        try
        {
            patchDoc.ApplyTo(new T());
            return true;
        }
        catch (Exception error)
        {
            errors = [error.Message];

            return false;
        }
    }

    public static bool TryGetUserId(this ClaimsPrincipal principal, [NotNullWhen(true)] out int? userId)
    {
        ArgumentNullException.ThrowIfNull(principal);

        userId = null;

        var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (nameIdClaim == null)
        {
            return false;
        }

        if (int.TryParse(nameIdClaim.Value, out var value))
        {
            userId = value;
            return true;
        }

        return false;
    }

    public static bool TryGetUserEmail(this ClaimsPrincipal principal, [NotNullWhen(true)] out string? email)
    {
        ArgumentNullException.ThrowIfNull(principal);

        email = null;

        var emailClaim = principal.FindFirst(ClaimTypes.Email);

        if (emailClaim == null)
        {
            return false;
        }

        email = emailClaim.Value;
        return true;
    }

    public static bool TryGetEmailVerified(this ClaimsPrincipal principal, [NotNullWhen(true)] out bool? emailVerified)
    {
        ArgumentNullException.ThrowIfNull(principal);

        emailVerified = null;

        var emailVerifiedClaim = principal.FindFirst(AppClaimTypes.EmailVerified);

        if (emailVerifiedClaim == null)
        {
            return false;
        }

        if (bool.TryParse(emailVerifiedClaim.Value, out var value))
        {
            emailVerified = value;
            return true;
        }

        return false;
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.IsInRole(UserRoleName.Admin);
    }
}