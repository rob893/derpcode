using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using DerpCode.API.Constants;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace DerpCode.API.Extensions;

public static class UtilityExtensions
{
    public static bool TryApply<T>(this JsonPatchDocument<T> patchDoc, T target, out string error)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(patchDoc);

        error = string.Empty;

        try
        {
            patchDoc.ApplyTo(target);
            return true;
        }
        catch (JsonPatchException ex)
        {
            error = ex.Message;

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

    public static bool TryGetUserName(this ClaimsPrincipal principal, [NotNullWhen(true)] out string? userName)
    {
        ArgumentNullException.ThrowIfNull(principal);

        userName = null;

        var userNameClaim = principal.FindFirst(ClaimTypes.Name);

        if (userNameClaim == null)
        {
            return false;
        }

        userName = userNameClaim.Value;
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

    public static HashSet<string> GetUserRoles(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return [.. principal.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles")
            .Select(c => c.Value)];
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.IsInRole(UserRoleName.Admin);
    }

    public static bool IsPremiumUser(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.IsInRole(UserRoleName.PremiumUser);
    }

    public static JsonPatchDocument<TDestination> MapPatchDocument<TSource, TDestination>(
        this JsonPatchDocument<TSource> sourceDoc,
        Func<string, string>? pathMapper = null)
        where TSource : class, new()
        where TDestination : class, new()
    {
        ArgumentNullException.ThrowIfNull(sourceDoc);

        var destDoc = new JsonPatchDocument<TDestination>();

        foreach (var sourceOp in sourceDoc.Operations)
        {
            var mappedOp = new Operation<TDestination>
            {
                op = sourceOp.op,
                path = pathMapper != null ? pathMapper(sourceOp.path) : sourceOp.path,
                from = pathMapper != null ? pathMapper(sourceOp.from) : sourceOp.from,
                value = sourceOp.value
            };

            destDoc.Operations.Add(mappedOp);
        }

        return destDoc;
    }

    public static T JsonClone<T>(this T source)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(source);

        return JsonSerializer.Deserialize<T>(
            JsonSerializer.Serialize(source)) ?? new T();
    }
}