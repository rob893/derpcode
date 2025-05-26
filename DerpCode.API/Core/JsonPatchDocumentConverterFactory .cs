using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.JsonPatch;

namespace DerpCode.API.Core;

public sealed class JsonPatchDocumentConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(JsonPatchDocument<>);
    }


    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);
        Type modelType = typeToConvert.GetGenericArguments()[0];
        Type converterType = typeof(JsonPatchDocumentConverter<>).MakeGenericType(modelType);
        return (JsonConverter?)Activator.CreateInstance(converterType) ?? throw new InvalidOperationException($"Unable to create converter for type {converterType}.");
    }
}