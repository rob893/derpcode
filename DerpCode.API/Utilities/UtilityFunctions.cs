using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Utilities;

public static class UtilityFunctions
{
    public static string GetSourceName(
        [CallerFilePath]
            string sourceFilePath = "",
        [CallerMemberName]
            string memberName = "")
    {
        var sourceName = string.Empty;
        if (!string.IsNullOrWhiteSpace(sourceFilePath))
        {
            sourceName = sourceFilePath.Contains('\\', StringComparison.Ordinal)
                ? sourceFilePath.Split('\\').Last().Split('.').First()
                : sourceFilePath.Split('/').Last().Split('.').First();
        }

        return $"{sourceName}.{memberName}";
    }

    public static string GetControllerName<T>()
    {
        var typeName = typeof(T).Name;
        var splitOn = "Controller";

        if (typeName == null || !typeName.EndsWith(splitOn, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Controllers must end with 'Controller'. {typeName} does not.", nameof(T));
        }

        return typeName.Split(splitOn).First();
    }

    public static LogLevel LogLevelFromString(string logLevel)
    {
        ArgumentNullException.ThrowIfNull(logLevel);

        return logLevel.ToUpperInvariant() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" => LogLevel.Debug,
            "INFORMATION" => LogLevel.Information,
            "WARNING" => LogLevel.Warning,
            "ERROR" => LogLevel.Error,
            "CRITICAL" => LogLevel.Critical,
            _ => throw new ArgumentException($"{nameof(logLevel)} must be Trace, Debug, Information, Warning, Error, or Critical.", nameof(logLevel)),
        };
    }

    public static bool AreListsEqual<T>(List<T> list1, List<T> list2)
    {
        ArgumentNullException.ThrowIfNull(list1);
        ArgumentNullException.ThrowIfNull(list2);

        if (list1.Count != list2.Count)
        {
            return false;
        }

        // For List<object>, use JSON serialization to compare values as this handles
        // type differences between deserialized JSON and database objects
        if (typeof(T) == typeof(object))
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json1 = JsonSerializer.Serialize(list1, options);
            var json2 = JsonSerializer.Serialize(list2, options);
            return json1 == json2;
        }

        // For other types, use regular equality comparison
        for (int i = 0; i < list1.Count; i++)
        {
            if (!Equals(list1[i], list2[i]))
            {
                return false;
            }
        }

        return true;
    }
}