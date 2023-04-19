using LaYumba.Functional;

namespace DataProcessing.Core;

public static class EnumerableExtensions
{
    public static IEnumerable<TR> Select<T, TP, TR>(this IEnumerable<T> @this, Func<T, TP, TR> fn, TP param1)
        => @this.Select(x => fn(x, param1));
    
    public static IEnumerable<TR> Bind<T, TP, TR>(this IEnumerable<T> @this, Func<T, TP, Option<TR>> fn, TP param1)
        => @this.Bind(x => fn(x, param1));
    
    public static TR Match<T, TR>(
        this IEnumerable<T> list,
        Func<TR> empty,
        Func<IEnumerable<T>, TR> otherwise)
    {
        return !list.Any() ? empty() : otherwise(list);
    }
}