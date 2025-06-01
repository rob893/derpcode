namespace DerpCode.API.Models.GitHub;

public sealed record GitHubUser
{
    public long Id { get; init; }

    public string Login { get; init; } = string.Empty;

    public string? Email { get; init; }
}