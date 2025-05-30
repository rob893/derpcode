using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using DerpCode.API.Constants;
using Microsoft.AspNetCore.Http;

namespace DerpCode.API.Extensions;

public static class HttpExtensions
{
    public static bool TryGetCorrelationId(this IHeaderDictionary headers, [NotNullWhen(true)] out string? correlationId)
    {
        ArgumentNullException.ThrowIfNull(headers);

        correlationId = null;

        if (headers.TryGetValue(AppHeaderNames.CorrelationId, out var value))
        {
            correlationId = value;
            correlationId ??= string.Empty;

            return true;
        }

        return false;
    }

    public static bool TryGetCorrelationId(this HttpHeaders headers, [NotNullWhen(true)] out string? correlationId)
    {
        ArgumentNullException.ThrowIfNull(headers);

        correlationId = null;

        if (headers.TryGetValues(AppHeaderNames.CorrelationId, out var values) && values.Any())
        {
            correlationId = values.First();
            return true;
        }

        return false;
    }

    public static string GetOrGenerateCorrelationId(this HttpHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (headers.TryGetValues(AppHeaderNames.CorrelationId, out var values) && values.Any())
        {
            return values.First();
        }

        return Guid.NewGuid().ToString();
    }

    public static string GetOrGenerateCorrelationId(this IHeaderDictionary headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (headers.TryGetValue(AppHeaderNames.CorrelationId, out var value))
        {
            return value.First() ?? Guid.NewGuid().ToString();
        }

        return Guid.NewGuid().ToString();
    }
}