namespace minuit2.net;

public class CostFunctionSum : ICostFunction
{
    private readonly ComponentCostFunction[] _components;
    private readonly Func<MinimizationResult, double> _parameterCovarianceScaleFactor;

    public CostFunctionSum(params ICostFunction[] components)
    {
        Parameters = components.DistinctParameters();
        HasGradient = components.All(c => c.HasGradient);
        Up = 1;  // Neutral element; Scaling is performed within the components because their factors might differ.
        
        _components = components.Select(AsComponentCostFunction).ToArray();
        _parameterCovarianceScaleFactor = components.ParameterCovarianceScaleFactor();
    }

    private ComponentCostFunction AsComponentCostFunction(ICostFunction costFunction) => new(costFunction, Parameters);

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double Up { get; }

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
    
    public MinimizationResult Adjusted(MinimizationResult minimizationResult)
    {
        var unscaledCostValue = _components.Sum(c => c.UnscaledValueFor(minimizationResult.ParameterValues.ToArray()));
        var covarianceScaleFactor = _parameterCovarianceScaleFactor(minimizationResult);
        return minimizationResult
            .WithCostValue(unscaledCostValue)
            .WithParameterCovariancesScaledBy(covarianceScaleFactor);
    }
}

internal class ComponentCostFunction(ICostFunction inner, IList<string> parameters) : ICostFunction
{
    private readonly List<int> _parameterIndices = inner.Parameters.Select(parameters.IndexOf).ToList();
    
    private double[] Belonging(IList<double> parameterValues)
    {
        var belonging = new double[_parameterIndices.Count];
        for (var i = 0; i < _parameterIndices.Count; i++) 
            belonging[i] = parameterValues[_parameterIndices[i]];
        
        return belonging;
    }

    public IList<string> Parameters => inner.Parameters;
    public bool HasGradient => inner.HasGradient;
    public double Up => inner.Up;
    
    public double ValueFor(IList<double> parameterValues)
    {
        // Scaling by 1/Up is needed to ensure numerical gradients are correct.
        // Re-scaling of final values (after minimization) is done in the parent/composite class.
        return UnscaledValueFor(parameterValues) / Up;
    }

    public double UnscaledValueFor(IList<double> parameterValues) => inner.ValueFor(Belonging(parameterValues));

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradients = inner.GradientFor(Belonging(parameterValues));
        
        var expandedGradients = new double[parameters.Count];
        foreach (var (gradient, index) in gradients.Zip(_parameterIndices))
            // Scaling by 1/Up is needed to ensure analytical gradients are correct.
            // In fact, since analytical gradients are trumped by numerical gradients (when different) for the default
            // strategy(1), this only has an effect on the result for the fast strategy(0).
            expandedGradients[index] = gradient / Up;
        
        return expandedGradients;
    }
    
    public MinimizationResult Adjusted(MinimizationResult minimizationResult) => minimizationResult;
}

file static class CostFunctionCollectionExtensions
{
    public static string[] DistinctParameters(this IEnumerable<ICostFunction> costFunctions) =>
        costFunctions.Aggregate(Enumerable.Empty<string>(), Union).ToArray();

    private static IEnumerable<string> Union(IEnumerable<string> parameters, ICostFunction cost) =>
        parameters.Union(cost.Parameters);

    public static Func<MinimizationResult, double> ParameterCovarianceScaleFactor(
        this IReadOnlyCollection<ICostFunction> costFunctions)
    {
        if (costFunctions.All(c => c is LeastSquares))
        {
            var leastSquares = costFunctions.Cast<LeastSquares>().ToArray();
            if (leastSquares.Any(c => c.ShouldScaleCovariances))
                // see LeastSquares class for details
                return r => r.CostValue / (leastSquares.Sum(c => c.NumberOfData) - r.NumberOfVariables);
        }

        return _ => 1;
    }
}
