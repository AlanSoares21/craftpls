namespace AlternativeMkt;

public static class ServerUtils
{
    public static Dictionary<string, string> GetQueryValues<T>(T query) {
        if (query is null)
            return new();
        return query.GetType()
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .ToDictionary(p => p.Name, p => {
                var propValue = p.GetValue(query, null);
                if (propValue is null)
                    return "";
                string? value = propValue.ToString();
                if (string.IsNullOrEmpty(value))
                    return "";
                return value;
            });
        
    }
}