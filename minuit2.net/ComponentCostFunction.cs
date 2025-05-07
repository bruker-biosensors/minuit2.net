namespace minuit2.net;

internal class ComponentCostFunction(ICostFunction inner, IList<string> parameters) : ICostFunction
{
    // To achieve proper scaling of component gradients (analytical and numerically approximated), both the function
    // values and gradients have to be scaled by 1/ErrorDefinition in place. Yet, doing so necessitates re-scaling of
    // the final function values (after minimization). This must be done in the hosting composite class.
    
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