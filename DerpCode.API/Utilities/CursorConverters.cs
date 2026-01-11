using System;
using DerpCode.API.Extensions;

namespace DerpCode.API.Utilities;

/// <summary>
/// Provides common reusable composite cursor converters for standard type combinations.
/// </summary>
public static class CursorConverters
{
    #region Int+String+Int Composite Methods

    /// <summary>
    /// Creates a composite key converter for int primary order value, string secondary order value, and int entity key.
    /// </summary>
    public static Func<(int PrimaryOrderValue, string SecondaryOrderValue, int Key), string> CreateCompositeKeyConverterIntStringInt()
    {
        return composite =>
            $"{composite.PrimaryOrderValue.ConvertToBase64UrlEncodedString()}|{composite.SecondaryOrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for int primary order value, string secondary order value, and int entity key.
    /// </summary>
    public static Func<string, (int PrimaryOrderValue, string SecondaryOrderValue, int Key)> CreateCompositeCursorConverterIntStringInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 3)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (
                parts[0].ConvertToInt32FromBase64UrlEncodedString(),
                parts[1].ConvertToStringFromBase64UrlEncodedString(),
                parts[2].ConvertToInt32FromBase64UrlEncodedString());
        };
    }

    #endregion

    #region Int+Int+Int Composite Methods

    /// <summary>
    /// Creates a composite key converter for int primary order value, int secondary order value, and int entity key.
    /// </summary>
    public static Func<(int PrimaryOrderValue, int SecondaryOrderValue, int Key), string> CreateCompositeKeyConverterIntIntInt()
    {
        return composite =>
            $"{composite.PrimaryOrderValue.ConvertToBase64UrlEncodedString()}|{composite.SecondaryOrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for int primary order value, int secondary order value, and int entity key.
    /// </summary>
    public static Func<string, (int PrimaryOrderValue, int SecondaryOrderValue, int Key)> CreateCompositeCursorConverterIntIntInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 3)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (
                parts[0].ConvertToInt32FromBase64UrlEncodedString(),
                parts[1].ConvertToInt32FromBase64UrlEncodedString(),
                parts[2].ConvertToInt32FromBase64UrlEncodedString());
        };
    }

    #endregion

    #region String Order Value Methods

    /// <summary>
    /// Creates a composite key converter for string order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(string OrderValue, int Key), string> CreateCompositeKeyConverterStringInt()
    {
        return composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for string order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (string OrderValue, int Key)> CreateCompositeCursorConverterStringInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (parts[0].ConvertToStringFromBase64UrlEncodedString(), parts[1].ConvertToInt32FromBase64UrlEncodedString());
        };
    }

    /// <summary>
    /// Creates a composite key converter for string order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(string OrderValue, long Key), string> CreateCompositeKeyConverterStringLong()
    {
        return composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for string order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (string OrderValue, long Key)> CreateCompositeCursorConverterStringLong()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (parts[0].ConvertToStringFromBase64UrlEncodedString(), parts[1].ConvertToLongFromBase64UrlEncodedString());
        };
    }

    #endregion

    #region Int Order Value Methods

    /// <summary>
    /// Creates a composite key converter for int order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(int OrderValue, int Key), string> CreateCompositeKeyConverterIntInt()
    {
        return composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for int order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (int OrderValue, int Key)> CreateCompositeCursorConverterIntInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (parts[0].ConvertToInt32FromBase64UrlEncodedString(), parts[1].ConvertToInt32FromBase64UrlEncodedString());
        };
    }

    /// <summary>
    /// Creates a composite key converter for int order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(int OrderValue, long Key), string> CreateCompositeKeyConverterIntLong()
    {
        return composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for int order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (int OrderValue, long Key)> CreateCompositeCursorConverterIntLong()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (parts[0].ConvertToInt32FromBase64UrlEncodedString(), parts[1].ConvertToLongFromBase64UrlEncodedString());
        };
    }

    #endregion

    #region Long Order Value Methods

    /// <summary>
    /// Creates a composite key converter for long order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(long OrderValue, int Key), string> CreateCompositeKeyConverterLongInt()
    {
        return composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for long order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (long OrderValue, int Key)> CreateCompositeCursorConverterLongInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (parts[0].ConvertToLongFromBase64UrlEncodedString(), parts[1].ConvertToInt32FromBase64UrlEncodedString());
        };
    }

    /// <summary>
    /// Creates a composite key converter for long order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(long OrderValue, long Key), string> CreateCompositeKeyConverterLongLong()
    {
        return composite => $"{composite.OrderValue.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for long order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (long OrderValue, long Key)> CreateCompositeCursorConverterLongLong()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            return (parts[0].ConvertToLongFromBase64UrlEncodedString(), parts[1].ConvertToLongFromBase64UrlEncodedString());
        };
    }

    #endregion

    #region DateTime Order Value Methods

    /// <summary>
    /// Creates a composite key converter for DateTime order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(DateTime OrderValue, int Key), string> CreateCompositeKeyConverterDateTimeInt()
    {
        return composite => $"{composite.OrderValue.ToBinary().ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for DateTime order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (DateTime OrderValue, int Key)> CreateCompositeCursorConverterDateTimeInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            var binaryValue = parts[0].ConvertToLongFromBase64UrlEncodedString();
            return (DateTime.FromBinary(binaryValue), parts[1].ConvertToInt32FromBase64UrlEncodedString());
        };
    }

    /// <summary>
    /// Creates a composite key converter for DateTime order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(DateTime OrderValue, long Key), string> CreateCompositeKeyConverterDateTimeLong()
    {
        return composite => $"{composite.OrderValue.ToBinary().ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for DateTime order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (DateTime OrderValue, long Key)> CreateCompositeCursorConverterDateTimeLong()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid composite cursor format: {cursor}");
            }

            var binaryValue = parts[0].ConvertToLongFromBase64UrlEncodedString();
            return (DateTime.FromBinary(binaryValue), parts[1].ConvertToLongFromBase64UrlEncodedString());
        };
    }

    #endregion

    #region DateTimeOffset Order Value Methods

    /// <summary>
    /// Creates a composite key converter for DateTimeOffset order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(DateTimeOffset OrderValue, int Key), string> CreateCompositeKeyConverterDateTimeOffsetInt()
    {
        return composite => $"{composite.OrderValue.DateTime.ToBinary().ConvertToBase64UrlEncodedString()}|{composite.OrderValue.Offset.Ticks.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for DateTimeOffset order value and int entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (DateTimeOffset OrderValue, int Key)> CreateCompositeCursorConverterDateTimeOffsetInt()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 3)
            {
                throw new ArgumentException($"Invalid DateTimeOffset composite cursor format: {cursor}");
            }

            var dateTimeBinary = parts[0].ConvertToLongFromBase64UrlEncodedString();
            var offsetTicks = parts[1].ConvertToLongFromBase64UrlEncodedString();
            var key = parts[2].ConvertToInt32FromBase64UrlEncodedString();
            var dateTime = DateTime.FromBinary(dateTimeBinary);
            var offset = new TimeSpan(offsetTicks);
            return (new DateTimeOffset(dateTime, offset), key);
        };
    }

    /// <summary>
    /// Creates a composite key converter for DateTimeOffset order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    public static Func<(DateTimeOffset OrderValue, long Key), string> CreateCompositeKeyConverterDateTimeOffsetLong()
    {
        return composite => $"{composite.OrderValue.DateTime.ToBinary().ConvertToBase64UrlEncodedString()}|{composite.OrderValue.Offset.Ticks.ConvertToBase64UrlEncodedString()}|{composite.Key.ConvertToBase64UrlEncodedString()}";
    }

    /// <summary>
    /// Creates a composite cursor converter for DateTimeOffset order value and long entity key.
    /// </summary>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    public static Func<string, (DateTimeOffset OrderValue, long Key)> CreateCompositeCursorConverterDateTimeOffsetLong()
    {
        return cursor =>
        {
            var parts = cursor.Split('|');
            if (parts.Length != 3)
            {
                throw new ArgumentException($"Invalid DateTimeOffset composite cursor format: {cursor}");
            }

            var dateTimeBinary = parts[0].ConvertToLongFromBase64UrlEncodedString();
            var offsetTicks = parts[1].ConvertToLongFromBase64UrlEncodedString();
            var key = parts[2].ConvertToLongFromBase64UrlEncodedString();
            var dateTime = DateTime.FromBinary(dateTimeBinary);
            var offset = new TimeSpan(offsetTicks);
            return (new DateTimeOffset(dateTime, offset), key);
        };
    }

    #endregion

    #region Generic Factory Methods

    /// <summary>
    /// Creates a generic composite key converter for any supported order value type and entity key type.
    /// </summary>
    /// <typeparam name="TOrderKey">The type of the ordering field (must be one of: string, int, long, DateTime, DateTimeOffset).</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key (must be int or long).</typeparam>
    /// <returns>A function that converts a composite key to a Base64 URL encoded string.</returns>
    /// <exception cref="NotSupportedException">Thrown when the type combination is not supported.</exception>
    public static Func<(TOrderKey OrderValue, TEntityKey Key), string> CreateCompositeKeyConverter<TOrderKey, TEntityKey>()
        where TOrderKey : IComparable<TOrderKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        var orderType = typeof(TOrderKey);
        var entityType = typeof(TEntityKey);

        return (orderType, entityType) switch
        {
            (Type t1, Type t2) when t1 == typeof(string) && t2 == typeof(int) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterStringInt(),
            (Type t1, Type t2) when t1 == typeof(string) && t2 == typeof(long) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterStringLong(),
            (Type t1, Type t2) when t1 == typeof(int) && t2 == typeof(int) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterIntInt(),
            (Type t1, Type t2) when t1 == typeof(int) && t2 == typeof(long) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterIntLong(),
            (Type t1, Type t2) when t1 == typeof(long) && t2 == typeof(int) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterLongInt(),
            (Type t1, Type t2) when t1 == typeof(long) && t2 == typeof(long) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterLongLong(),
            (Type t1, Type t2) when t1 == typeof(DateTime) && t2 == typeof(int) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterDateTimeInt(),
            (Type t1, Type t2) when t1 == typeof(DateTime) && t2 == typeof(long) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterDateTimeLong(),
            (Type t1, Type t2) when t1 == typeof(DateTimeOffset) && t2 == typeof(int) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterDateTimeOffsetInt(),
            (Type t1, Type t2) when t1 == typeof(DateTimeOffset) && t2 == typeof(long) =>
                (Func<(TOrderKey, TEntityKey), string>)(object)CreateCompositeKeyConverterDateTimeOffsetLong(),
            _ => throw new NotSupportedException($"Unsupported type combination: TOrderKey={orderType.Name}, TEntityKey={entityType.Name}")
        };
    }

    /// <summary>
    /// Creates a generic composite cursor converter for any supported order value type and entity key type.
    /// </summary>
    /// <typeparam name="TOrderKey">The type of the ordering field (must be one of: string, int, long, DateTime, DateTimeOffset).</typeparam>
    /// <typeparam name="TEntityKey">The type of the entity key (must be int or long).</typeparam>
    /// <returns>A function that converts a Base64 URL encoded string back to a composite key.</returns>
    /// <exception cref="NotSupportedException">Thrown when the type combination is not supported.</exception>
    public static Func<string, (TOrderKey OrderValue, TEntityKey Key)> CreateCompositeCursorConverter<TOrderKey, TEntityKey>()
        where TOrderKey : IComparable<TOrderKey>
        where TEntityKey : IEquatable<TEntityKey>, IComparable<TEntityKey>
    {
        var orderType = typeof(TOrderKey);
        var entityType = typeof(TEntityKey);

        return (orderType, entityType) switch
        {
            (Type t1, Type t2) when t1 == typeof(string) && t2 == typeof(int) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterStringInt(),
            (Type t1, Type t2) when t1 == typeof(string) && t2 == typeof(long) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterStringLong(),
            (Type t1, Type t2) when t1 == typeof(int) && t2 == typeof(int) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterIntInt(),
            (Type t1, Type t2) when t1 == typeof(int) && t2 == typeof(long) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterIntLong(),
            (Type t1, Type t2) when t1 == typeof(long) && t2 == typeof(int) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterLongInt(),
            (Type t1, Type t2) when t1 == typeof(long) && t2 == typeof(long) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterLongLong(),
            (Type t1, Type t2) when t1 == typeof(DateTime) && t2 == typeof(int) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterDateTimeInt(),
            (Type t1, Type t2) when t1 == typeof(DateTime) && t2 == typeof(long) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterDateTimeLong(),
            (Type t1, Type t2) when t1 == typeof(DateTimeOffset) && t2 == typeof(int) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterDateTimeOffsetInt(),
            (Type t1, Type t2) when t1 == typeof(DateTimeOffset) && t2 == typeof(long) =>
                (Func<string, (TOrderKey, TEntityKey)>)(object)CreateCompositeCursorConverterDateTimeOffsetLong(),
            _ => throw new NotSupportedException($"Unsupported type combination: TOrderKey={orderType.Name}, TEntityKey={entityType.Name}")
        };
    }

    #endregion
}
