using DerpCode.API.Models.Dtos;

namespace DerpCode.API.Models.Responses.Auth;

public sealed record LoginResponse
{
    public required string Token { get; init; }

    public required UserDto User { get; init; }
}