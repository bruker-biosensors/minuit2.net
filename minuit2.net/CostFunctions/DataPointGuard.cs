namespace minuit2.net.CostFunctions;
using NamedValues = (IReadOnlyList<double> Values, string Name);

internal static class DataPointGuard
{
    public static void ThrowIfCountMismatchBetween(NamedValues reference, params NamedValues[] others)
    {
        var exceptions = new List<Exception>();
        foreach (var other in others)
        {
            if (reference.Values.Count != other.Values.Count)
                exceptions.Add(new ArgumentException(
                    $"{reference.Name} and {other.Name} must have the same number of values, " +
                    $"but found {reference.Values.Count} and {other.Values.Count}, respectively."));
        }
        
        if (exceptions.Count == 1)
            throw exceptions.Single();
        if (exceptions.Count > 1) 
            throw new AggregateException(exceptions);
    }
}