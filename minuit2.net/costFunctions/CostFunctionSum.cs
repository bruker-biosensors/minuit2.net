namespace minuit2.net.costFunctions;

public class CostFunctionSum : ICostFunction, IComposite
{
    private readonly ComponentCostFunction[] _components;

    public CostFunctionSum(params ICostFunction[] components)
    {
        Parameters = components.DistinctParameters();
        HasGradient = components.All(c => c.HasGradient);
        ErrorDefinition = 1;  // Neutral element; Scaling is performed within the components since factors may differ.
        
        _components = components.Select(AsComponentCostFunction).ToArray();
    }

    private ComponentCostFunction AsComponentCostFunction(ICostFunction costFunction) =>
        costFunction as ComponentCostFunction ?? new ComponentCostFunction(costFunction, Parameters);

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
    public double ValueFor(IList<double> parameterValues) => _components.Select(c => c.ValueFor(parameterValues)).Sum();

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradients = new double[Parameters.Count];
        foreach (var componentGradients in _components.Select(c => c.GradientFor(parameterValues))) 
            Add(componentGradients, gradients);

        return gradients;
    }

    private void Add(IList<double> componentGradients, double[] gradients)
    {
        for (var i = 0; i < Parameters.Count; i++) 
            gradients[i] += componentGradients[i];
    }

    public double CompositeValueFor(IList<double> parameterValues) =>
        _components.Select(c => c.ValueFor(parameterValues) * c.ErrorDefinition).Sum();
}

file static class CostFunctionCollectionExtensions
{
    public static string[] DistinctParameters(this IEnumerable<ICostFunction> costFunctions) =>
        costFunctions.Aggregate(Enumerable.Empty<string>(), Union).ToArray();

    private static IEnumerable<string> Union(IEnumerable<string> parameters, ICostFunction cost) =>
        parameters.Union(cost.Parameters);
}