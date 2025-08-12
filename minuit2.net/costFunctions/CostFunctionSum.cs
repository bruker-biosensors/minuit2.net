namespace minuit2.net.costFunctions;

internal class CostFunctionSum : ICompositeCostFunction
{
    protected readonly ICostFunction[] Components;

    public CostFunctionSum(params ICostFunction[] components)
    {
        Parameters = components.DistinctParameters();
        HasGradient = components.All(c => c.HasGradient);
        ErrorDefinition = 1;  // Neutral element; Scaling is performed within the components since factors may differ.
        
        Components = components.Select(AsComponentCostFunction).ToArray();
    }

    private ICostFunction AsComponentCostFunction(ICostFunction costFunction)
    {
        return costFunction is ComponentCostFunction
            ? costFunction
            : CostFunction.Component(costFunction, Parameters);
    }

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
    public double ValueFor(IList<double> parameterValues) => Components.Select(c => c.ValueFor(parameterValues)).Sum();

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradients = new double[Parameters.Count];
        foreach (var componentGradients in Components.Select(c => c.GradientFor(parameterValues))) 
            Add(componentGradients, gradients);

        return gradients;
    }

    private void Add(IList<double> componentGradients, double[] gradients)
    {
        for (var i = 0; i < Parameters.Count; i++) 
            gradients[i] += componentGradients[i];
    }

    public double CompositeValueFor(IList<double> parameterValues) =>
        Components.Select(c => c.ValueFor(parameterValues) * c.ErrorDefinition).Sum();
}

file static class CostFunctionCollectionExtensions
{
    public static string[] DistinctParameters(this IEnumerable<ICostFunction> costFunctions) =>
        costFunctions.Aggregate(Enumerable.Empty<string>(), Union).ToArray();

    private static IEnumerable<string> Union(IEnumerable<string> parameters, ICostFunction cost) =>
        parameters.Union(cost.Parameters);
}