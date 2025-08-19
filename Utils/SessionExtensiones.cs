using Microsoft.AspNetCore.Http;
using System.Text.Json;


public static class SessionExtensiones
{
    public static void SetJson<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetJson<T>(this ISession session, string key)
    {
        var s = session.GetString(key);
        return s is null ? default : JsonSerializer.Deserialize<T>(s);
    }
}


