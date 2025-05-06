namespace minuit2.UnitTests;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> InRandomOrder<T>(this IEnumerable<T> enumerable)
    {
        var random = new Random();
        return enumerable.OrderBy(_ => random.Next());
    }
}