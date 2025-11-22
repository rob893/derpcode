using System.Collections.Generic;

namespace DerpCode.API.Models.Requests;

public sealed record EditRoleRequest
{
    public IReadOnlyList<string> RoleNames { get; init; } = [];
}