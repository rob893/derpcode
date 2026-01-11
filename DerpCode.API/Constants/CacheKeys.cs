namespace DerpCode.API.Constants;

public static class CacheKeys
{
    /// <summary>
    /// Key for caching the list of problems.
    /// </summary>
    public const string Problems = "Problems";

    /// <summary>
    /// Prefix for caching a user's favorite problem IDs. Append the user ID.
    /// </summary>
    public const string UserFavoriteProblemIdsPrefix = "UserFavoriteProblemIds:";

    /// <summary>
    /// Key for caching the list of tags.
    /// </summary>
    public const string Tags = "tags";
}