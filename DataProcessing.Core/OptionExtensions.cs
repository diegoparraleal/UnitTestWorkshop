using LaYumba.Functional;
using static LaYumba.Functional.F;

namespace DataProcessing.Core;

public static class OptionExtensions
{
    public static Option<T> AsOption<T>(this T @this) => @this == null ? None : Some(@this);
    public static bool HasValue<T>(this Option<T> @this) => @this.Match(() => false,_ => true);
    public static T GetOrDefault<T>(this Option<T> @this) => @this.Match(() => default!,x => x);
    public static T GetOrNull<T>(this Option<T> @this) where T: class => @this.Match(() => null!,x => x);
    public static IEnumerable<T> ToEnumerable<T>(this IEnumerable<Option<T>> @this) => 
        @this.SelectMany(x => x.AsEnumerable());
}