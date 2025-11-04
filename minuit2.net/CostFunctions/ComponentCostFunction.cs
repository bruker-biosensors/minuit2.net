namespace minuit2.net.CostFunctions;

internal class ComponentCostFunction(ICostFunction inner, IList<string> compositeParameters) : ICostFunction
{
    private readonly int[] _parameterIndices = inner.Parameters.Select(compositeParameters.IndexOf).ToArray();

    private double[] Belonging(IReadOnlyList<double> parameterValues)
    {
        var belonging = new double[_parameterIndices.Length];
        for (var i = 0; i < _parameterIndices.Length; i++) 
            belonging[i] = parameterValues[_parameterIndices[i]];
        
        return belonging;
    }

    public IReadOnlyList<string> Parameters => inner.Parameters;
    public bool HasGradient => inner.HasGradient;
    public double ErrorDefinition => inner.ErrorDefinition;

    public double ValueFor(IReadOnlyList<double> parameterValues) => inner.ValueFor(Belonging(parameterValues));

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradients = inner.GradientFor(Belonging(parameterValues));
        var expandedGradients = new double[compositeParameters.Count];
        for (var i = 0; i < gradients.Count; i++)
            expandedGradients[_parameterIndices[i]] = gradients[i];
        
        return expandedGradients;
    }

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => 
        inner.WithErrorDefinitionRecalculatedBasedOnValid(result);
}