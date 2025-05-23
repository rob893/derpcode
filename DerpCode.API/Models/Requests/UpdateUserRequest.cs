using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Requests;

public sealed record UpdateUserRequest
{
    [MaxLength(255)]
    public string? FirstName { get; init; }

    [MaxLength(255)]
    public string? LastName { get; init; }
}