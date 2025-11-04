namespace minuit2.net.CostFunctions;

internal class CostFunctionSum : ICompositeCostFunction
{
    private readonly ComponentCostFunction[] _components;

    public CostFunctionSum(params ICostFunction[] components)
    {
        var parameters = components.DistinctParameters();
        Parameters = parameters;
        HasGradient = components.All(c => c.HasGradient);
        ErrorDefinition = 1;  // Neutral element; Scaling is performed for each component individually since factors may differ.
        
        _components = components.Select(c => new ComponentCostFunction(c, parameters)).ToArray();
    }

    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }

    public double ValueFor(IReadOnlyList<double> parameterValues) => 
        _components.Sum(c => c.ValueFor(parameterValues) / c.ErrorDefinition);

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradients = new double[Parameters.Count];
        foreach (var component in _components)
        {
            var componentGradients = component.GradientFor(parameterValues);
            for (var i = 0; i < Parameters.Count; i++)
                gradients[i] += componentGradients[i] / component.ErrorDefinition;
        }
        
        return gradients;
    }

    public double CompositeValueFor(IReadOnlyList<double> parameterValues) =>
        _components.Sum(c => c.ValueFor(parameterValues));

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) =>
        new CostFunctionSum(_components.Select(c => c.WithErrorDefinitionRecalculatedBasedOnValid(result)).ToArray());
}