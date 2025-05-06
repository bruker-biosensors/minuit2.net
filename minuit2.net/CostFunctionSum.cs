namespace minuit2.net;

public class CostFunctionSum : ICompositeCostFunction
{
    private readonly ComponentCostFunction[] _components;

    public CostFunctionSum(params ICostFunction[] components)
    {
        Parameters = components.DistinctParameters();
        HasGradient = components.All(c => c.HasGradient);
        ErrorDefinition = 1;  // Neutral element; Scaling is performed within the components because their factors might differ.
        RequiresErrorDefinitionAutoScaling = components.Any(c => c.RequiresErrorDefinitionAutoScaling);
        
        _components = components.Select(AsComponentCostFunction).ToArray();
    }

    private ComponentCostFunction AsComponentCostFunction(ICostFunction costFunction) => new(costFunction, Parameters);

    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    public bool RequiresErrorDefinitionAutoScaling { get; }
    
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
    
    public void AutoScaleErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables)
    {
        foreach (var component in _components.Where(c => c.RequiresErrorDefinitionAutoScaling)) 
            component.AutoScaleErrorDefinitionBasedOn(parameterValues, variables);
    }

    public double CompositeValueFor(IList<double> parameterValues) =>
        _components.Select(c => c.ValueFor(parameterValues) * c.ErrorDefinition).Sum();
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

    private string[] Belonging(IList<string> variables) => variables.Where(var => Parameters.Contains(var)).ToArray();

    public IList<string> Parameters => inner.Parameters;
    public bool HasGradient => inner.HasGradient;
    public double ErrorDefinition => inner.ErrorDefinition;
    public bool RequiresErrorDefinitionAutoScaling => inner.RequiresErrorDefinitionAutoScaling;

    public double ValueFor(IList<double> parameterValues) =>
        inner.ValueFor(Belonging(parameterValues)) / ErrorDefinition;

    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradients = inner.GradientFor(Belonging(parameterValues));
        
        var expandedGradients = new double[parameters.Count];
        foreach (var (gradient, index) in gradients.Zip(_parameterIndices))
            expandedGradients[index] = gradient / ErrorDefinition;
        
        return expandedGradients;
    }
    
    public void AutoScaleErrorDefinitionBasedOn(IList<double> parameterValues, IList<string> variables) =>
        inner.AutoScaleErrorDefinitionBasedOn(Belonging(parameterValues), Belonging(variables));
}

file static class CostFunctionCollectionExtensions
{
    public static string[] DistinctParameters(this IEnumerable<ICostFunction> costFunctions) =>
        costFunctions.Aggregate(Enumerable.Empty<string>(), Union).ToArray();

    private static IEnumerable<string> Union(IEnumerable<string> parameters, ICostFunction cost) =>
        parameters.Union(cost.Parameters);
}