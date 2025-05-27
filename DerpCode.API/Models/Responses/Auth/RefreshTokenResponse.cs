namespace DerpCode.API.Models.Responses.Auth;

public sealed record RefreshTokenResponse
{
    public required string Token { get; init; }
}