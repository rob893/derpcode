using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DerpCode.API.Data.Comparers;

/// <summary>
/// Value comparer for JSON-serialized collections in EF Core
/// </summary>
/// <typeparam name="T">The type of the collection</typeparam>
public class JsonCollectionComparer<T> : ValueComparer<T> where T : class
{
    public JsonCollectionComparer(JsonSerializerOptions options)
        : base(
            (c1, c2) => JsonSerializer.Serialize(c1, options) == JsonSerializer.Serialize(c2, options),
            c => c == null ? 0 : JsonSerializer.Serialize(c, options).GetHashCode(StringComparison.Ordinal),
            c => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(c, options), options)!)
    { }
}