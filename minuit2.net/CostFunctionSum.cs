namespace minuit2.net;

public class CostFunctionSum : ICostFunction
{
    private readonly ICostFunction[] _components;

    protected CostFunctionSum(params ICostFunction[] components)
    {
        Parameters = components.DistinctParameters();
        HasGradient = components.All(c => c.HasGradient);
        Up = 1;  // TODO: The scaling will be/needs to be performed within the components (see below), that's why it should be 1 here (to not scale errors for the cost-sum again). However, I need to think about this in more detail.
        
        _components = components.Select(AsComponentCostFunction).ToArray();
    }

    private ICostFunction AsComponentCostFunction(ICostFunction costFunction) =>
        new ComponentCostFunction(costFunction, Parameters);

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
}

file class ComponentCostFunction(ICostFunction inner, IList<string> parameters) : ICostFunction
{
    private readonly List<int> _parameterIndices = inner.Parameters.Select(parameters.IndexOf).ToList();
    
    private double[] Belonging(IList<double> values)
    {
        var belongingValues = new double[_parameterIndices.Count];
        for (var i = 0; i < _parameterIndices.Count; i++) 
            belongingValues[i] = values[_parameterIndices[i]];
        
        return belongingValues;
    }

    private double[] Projected(IList<double> values)
    {
        var projectedValues = new double[parameters.Count];
        foreach (var (value, index) in values.Zip(_parameterIndices))
            projectedValues[index] = value;
        
        return projectedValues;
    }

    public IList<string> Parameters => inner.Parameters;
    public bool HasGradient => inner.HasGradient;
    public double Up => inner.Up;  // TODO: If I scale the gradients "manually" (see below), I probably should expose 1 here, even if nobody is listening.
    
    public double ValueFor(IList<double> parameterValues) => inner.ValueFor(Belonging(parameterValues));  // TODO: Do I need to scale this by 1/Up? This is done in iminuit. Not sure if it is correct, though. Certainly need test first.

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradients = inner.GradientFor(Belonging(parameterValues));  // TODO: These should certainly be scaled by 1/Up. Need test first.
        return Projected(gradients);
    }
}

file static class CostFunctionCollectionExtensions
{
    public static string[] DistinctParameters(this IEnumerable<ICostFunction> costFunctions) =>
        costFunctions.Aggregate(Enumerable.Empty<string>(), Union).ToArray();

    private static IEnumerable<string> Union(IEnumerable<string> parameters, ICostFunction cost) =>
        parameters.Union(cost.Parameters);
}
