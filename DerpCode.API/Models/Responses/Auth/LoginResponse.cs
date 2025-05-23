
using DerpCode.API.Models.Dtos;

namespace DerpCode.API.Models.Responses.Auth;

public sealed record LoginResponse
{
    public string Token { get; init; } = default!;

    public string RefreshToken { get; init; } = default!;

    public UserDto User { get; init; } = default!;
}