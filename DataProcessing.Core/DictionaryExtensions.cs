using LaYumba.Functional;

namespace DataProcessing.Core;

public static class DictionaryExtensions
{
    public static bool TryGetValue<TK, TV>(this IDictionary<TK, TV> @this, TK key, out TV value)
    {
        var result = @this.Lookup(key);
        value = result.GetOrDefault();
        return result.HasValue();
    }
}