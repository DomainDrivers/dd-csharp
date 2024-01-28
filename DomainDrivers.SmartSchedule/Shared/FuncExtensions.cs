namespace DomainDrivers.SmartSchedule.Shared;

public static class FuncExtensions
{
    public static Func<TSource, TResult> AndThen<TSource, TIntermediate, TResult>(
        this Func<TSource, TIntermediate> func1, Func<TIntermediate, TResult> func2)
    {
        return source => func2(func1(source));
    }
}