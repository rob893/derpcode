namespace DerpCode.API.Models.GitHub;

public sealed record GitHubEmail
{
    public string Email { get; init; } = string.Empty;

    public bool Primary { get; init; }

    public bool Verified { get; init; }

    public string Visibility { get; init; } = string.Empty;
}