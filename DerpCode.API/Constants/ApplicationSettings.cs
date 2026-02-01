namespace DerpCode.API.Constants;

public static class ApplicationSettings
{
    public const string HealthCheckEndpoint = "/health";

    public const string LivenessHealthCheckEndpoint = "/health/liveness";

    public const string GitHubAppHeader = "derpcode-api";

    public const int SystemUserId = 1;
}