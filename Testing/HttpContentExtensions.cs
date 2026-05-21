using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Testing;

public static class HttpContentExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static Task<T?> ReadFromJsonAsync<T>(this HttpContent content)
    {
        return content.ReadFromJsonAsync<T>(DefaultOptions);
    }
}
