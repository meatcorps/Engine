namespace Meatcorps.Engine.Session.Utilities;

public class SessionUtil
{
    public static string PackToToken(params IReadOnlyDictionary<string, string>[] maps)
    {
        var merged = new Dictionary<string, string>();
        foreach (var m in maps)
        foreach (var kv in m)
            merged[kv.Key] = kv.Value;

        var json = System.Text.Json.JsonSerializer.Serialize(merged);
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    }
    
    internal static int Fnv1aHash(string s)
    {
        unchecked
        {
            const int fnvPrime = 16777619;
            int hash = (int)2166136261;
            foreach (var c in s)
            {
                hash ^= c;
                hash *= fnvPrime;
            }
            return hash;
        }
    }
}