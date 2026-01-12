namespace DerpCode.API.Constants;

public static class CacheKeys
{
    /// <summary>
    /// Key for caching the list of problems.
    /// </summary>
    public const string Problems = "Problems";

    /// <summary>
    /// Key for caching the list of tags.
    /// </summary>
    public const string Tags = "tags";

    /// <summary>
    /// Gets the cache key for personalized problems for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The cache key for the user's personalized problems.</returns>
    public static string GetPersonalizedProblemsKey(int userId) => $"{Problems}:{userId}";
}