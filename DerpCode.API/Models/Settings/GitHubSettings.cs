namespace DerpCode.API.Models.Settings;

public sealed record GitHubSettings
{
    public string PersonalAccessToken { get; init; } = default!;

    public string DerpCodeRepository { get; init; } = default!;

    public string DerpCodeRepositoryOwner { get; init; } = default!;
}