using System.Collections.Generic;

namespace DerpCode.API.Models.Requests;

public sealed record EditRoleRequest
{
    public List<string> RoleNames { get; init; } = [];
}