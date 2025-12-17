namespace minuit2.net.CostFunctions;

internal class CostFunctionSum : ICompositeCostFunction
{
    private readonly ComponentCostFunction[] _components;

    public CostFunctionSum(params ICostFunction[] components)
    {
        var parameters = components.DistinctParameters();
        Parameters = parameters;
        HasGradient = components.All(c => c.HasGradient);
        HasHessian = components.All(c => c.HasHessian);
        HasHessianDiagonal = components.All(c => c.HasHessianDiagonal);
        ErrorDefinition = 1;  // Neutral element; Scaling is performed for each component individually since factors may differ.
        
        _components = components.Select(c => new ComponentCostFunction(c, parameters)).ToArray();
    }

    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public bool HasHessian { get; }
    public bool HasHessianDiagonal { get; }
    public double ErrorDefinition { get; }

    public double ValueFor(IReadOnlyList<double> parameterValues) => 
        _components.Sum(c => c.ValueFor(parameterValues) / c.ErrorDefinition);

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradient = new double[Parameters.Count];
        foreach (var component in _components)
        {
            var componentGradient = component.GradientFor(parameterValues);
            for (var i = 0; i < Parameters.Count; i++)
                gradient[i] += componentGradient[i] / component.ErrorDefinition;
        }
        
        return gradient;
    }

    public IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues)
    {
        var hessian = new double[Parameters.Count * Parameters.Count];
        foreach (var component in _components)
        {
            var componentHessian = component.HessianFor(parameterValues);
            for (var i = 0; i < Parameters.Count * Parameters.Count; i++)
                hessian[i] += componentHessian[i] / component.ErrorDefinition;
        }
        
        return hessian;
    }

    public IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues)
    {
        var hessianDiagonal = new double[Parameters.Count];
        foreach (var component in _components)
        {
            var componentHessianDiagonal = component.HessianDiagonalFor(parameterValues);
            for (var i = 0; i < Parameters.Count; i++)
                hessianDiagonal[i] += componentHessianDiagonal[i] / component.ErrorDefinition;
        }
        
        return hessianDiagonal;
    }

    public double CompositeValueFor(IReadOnlyList<double> parameterValues) =>
        _components.Sum(c => c.ValueFor(parameterValues));

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) =>
        new CostFunctionSum(_components.Select(c => c.WithErrorDefinitionRecalculatedBasedOnValid(result)).ToArray());
}