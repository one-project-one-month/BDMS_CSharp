using System.Text.Json;
using System.Text.Json.Serialization;

namespace Testing;

public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
