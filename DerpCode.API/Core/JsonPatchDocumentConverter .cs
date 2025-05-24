using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.JsonPatch;

namespace DerpCode.API.Core;

public sealed class JsonPatchDocumentConverter<T> : JsonConverter<JsonPatchDocument<T>> where T : class
{
    public override JsonPatchDocument<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var patchDocument = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonPatchDocument<T>>(JsonDocument.ParseValue(ref reader).RootElement.GetRawText());

        return patchDocument ?? throw new JsonException("Failed to deserialize JsonPatchDocument.");
    }

    public override void Write(Utf8JsonWriter writer, JsonPatchDocument<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
        writer.WriteRawValue(json);
    }
}