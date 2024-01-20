namespace DomainDrivers.SmartSchedule.Shared;

public static class CollectionExtensions
{
    // Based on: https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/util/Set.html#hashCode()
    public static int CalculateHashCode<T>(this ISet<T> collection) where T : class
    {
        var sum = 0;
        foreach (var element in collection)
        {
            sum = unchecked(sum + element.GetHashCode());
        }

        return sum;
    }

    // Based on: https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/util/List.html#hashCode()
    public static int CalculateHashCode<T>(this IList<T> collection) where T : class
    {
        return collection
            .Aggregate(0, (hash, pair) => HashCode.Combine(hash, pair.GetHashCode()));
    }

    // Based on: https://docs.oracle.com/en/java/javase/21/docs/api/java.base/java/util/Map.html#hashCode()
    public static int CalculateHashCode<TKey, TValue>(this IDictionary<TKey, TValue> collection)
    {
        var sum = 0;
        foreach (var element in collection)
        {
            sum = unchecked(sum + (element.Value == null ? 0 : element.Value.GetHashCode()));
        }

        return sum;
    }
    
    public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> first,
        IDictionary<TKey, TValue> second, IEqualityComparer<TValue>? valueComparer = null)
    {
        if (ReferenceEquals(first, second)) return true;
        if (first.Count != second.Count) return false;

        valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

        foreach (var kvp in first)
        {
            if (!second.TryGetValue(kvp.Key, out var secondValue)) return false;
            if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
        }

        return true;
    }

    public static string ToCollectionString<T>(this IEnumerable<T> enumerable)
    {
        return $"[{string.Join(", ", enumerable)}]";
    }
}