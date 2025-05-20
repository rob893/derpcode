using System;

namespace DerpCode.API.Models;

public interface IOwnedByUser<TKey> where TKey : IEquatable<TKey>, IComparable<TKey>
{
    TKey UserId { get; }
}