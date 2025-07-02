namespace minuit2.net;

internal static class EnumerableExtensions
{
    public static T ElementAtOrDefault<T>(this IEnumerable<T> enumerable, int index, T defaultValue) =>
        enumerable.Skip(index).FirstOrDefault(defaultValue);
}