namespace DerpCode.API.Models.Responses.Auth;

public sealed record RefreshTokenResponse
{
    public string Token { get; init; } = default!;
}