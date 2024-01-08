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
}