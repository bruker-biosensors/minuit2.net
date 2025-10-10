namespace minuit2.net.CostFunctions;

internal class ComponentCostFunction(ICostFunction inner, IList<string> compositeParameters) : ICostFunction
{
    // To achieve proper scaling of component gradients (both analytical and numerically approximated), the function
    // values and gradients have to be scaled by 1 / ErrorDefinition within the local ValueFor and GradientFor methods.
    // This is because different components may have different ErrorDefinitions, so there is no global scaling that
    // could be applied by the hosting composite cost function.
    // Yet doing so requires re-scaling of the final function values (after minimization). This must be done by the
    // hosting composite cost function.
    
    private readonly int[] _parameterIndices = inner.Parameters.Select(compositeParameters.IndexOf).ToArray();
    
    protected readonly IList<string> CompositeParameters = compositeParameters;
    
    protected double[] Belonging(IReadOnlyList<double> parameterValues)
    {
        var belonging = new double[_parameterIndices.Length];
        for (var i = 0; i < _parameterIndices.Length; i++) 
            belonging[i] = parameterValues[_parameterIndices[i]];
        
        return belonging;
    }

    public IReadOnlyList<string> Parameters => inner.Parameters;
    public bool HasGradient => inner.HasGradient;
    public double ErrorDefinition => inner.ErrorDefinition;

    public double ValueFor(IReadOnlyList<double> compositeParameterValues) =>
        inner.ValueFor(Belonging(compositeParameterValues)) / ErrorDefinition;

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> compositeParameterValues)
    {
        var gradients = inner.GradientFor(Belonging(compositeParameterValues));
        var expandedGradients = new double[CompositeParameters.Count];
        for (var i = 0; i < gradients.Count; i++)
            expandedGradients[_parameterIndices[i]] = gradients[i] / ErrorDefinition;
        
        return expandedGradients;
    }
}