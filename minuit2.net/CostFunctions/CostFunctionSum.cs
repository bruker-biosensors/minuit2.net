namespace minuit2.net.CostFunctions;

internal class CostFunctionSum : ICompositeCostFunction
{
    private readonly ICostFunction[] _components;
    private readonly string[] _compositeParameters;

    public CostFunctionSum(params ICostFunction[] components)
    {
        _compositeParameters = components.DistinctParameters();
        Parameters = _compositeParameters;
        HasGradient = components.All(c => c.HasGradient);
        ErrorDefinition = 1;  // Neutral element; Scaling is performed within the components since factors may differ.
        
        _components = components.Select(AsComponentCostFunction).ToArray();
    }

    private ICostFunction AsComponentCostFunction(ICostFunction costFunction)
    {
        return costFunction is ComponentCostFunction
            ? costFunction
            : CostFunction.Component(costFunction, _compositeParameters);
    }

    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }

    public double ValueFor(IReadOnlyList<double> parameterValues) =>
        _components.Select(c => c.ValueFor(parameterValues)).Sum();

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradients = new double[Parameters.Count];
        foreach (var componentGradients in _components.Select(c => c.GradientFor(parameterValues))) 
            Add(componentGradients, gradients);

        return gradients;
    }

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) =>
        new CostFunctionSum(_components.Select(c => c.WithErrorDefinitionRecalculatedBasedOnValid(result)).ToArray());

    private void Add(IReadOnlyList<double> componentGradients, double[] gradients)
    {
        for (var i = 0; i < Parameters.Count; i++) 
            gradients[i] += componentGradients[i];
    }

    public double CompositeValueFor(IReadOnlyList<double> parameterValues) =>
        _components.Select(c => c.ValueFor(parameterValues) * c.ErrorDefinition).Sum();
}