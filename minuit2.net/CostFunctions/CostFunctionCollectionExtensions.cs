namespace minuit2.net.CostFunctions;

internal static class CostFunctionCollectionExtensions
{
    public static List<string> DistinctParameters(this IEnumerable<ICostFunction> costFunctions) =>
        costFunctions.Aggregate(Enumerable.Empty<string>(), Union).ToList();

    private static IEnumerable<string> Union(IEnumerable<string> parameters, ICostFunction cost) =>
        parameters.Union(cost.Parameters);
}