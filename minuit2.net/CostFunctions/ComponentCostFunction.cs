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
    public bool HasHessian => inner.HasHessian;
    public bool HasHessianDiagonal => inner.HasHessianDiagonal;
    public double ErrorDefinition => inner.ErrorDefinition;

    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        return inner.ValueFor(Belonging(parameterValues));
    }

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var innerGradient = inner.GradientFor(Belonging(parameterValues));
        var outerGradient = new double[compositeParameters.Count];
        for (var i = 0; i < inner.Parameters.Count; i++)
            outerGradient[_parameterIndices[i]] = innerGradient[i];
        
        return outerGradient;
    }

    public IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues)
    {
        var innerHessian = inner.HessianFor(Belonging(parameterValues));
        var outerHessian = new double[compositeParameters.Count * compositeParameters.Count];
        for (var j = 0; j < inner.Parameters.Count; j++)
        for (var k = 0; k < inner.Parameters.Count; k++)
        {
            var innerIndex = j * inner.Parameters.Count + k;
            var outerIndex = _parameterIndices[j] * compositeParameters.Count + _parameterIndices[k];
            outerHessian[outerIndex] = innerHessian[innerIndex];
        }
        
        return outerHessian;
    }

    public IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues)
    {
        var innerHessianDiagonal = inner.HessianDiagonalFor(Belonging(parameterValues));
        var outerHessianDiagonal = new double[compositeParameters.Count];
        for (var i = 0; i < inner.Parameters.Count; i++)
            outerHessianDiagonal[_parameterIndices[i]] = innerHessianDiagonal[i];
        
        return outerHessianDiagonal;
    }

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => 
        inner.WithErrorDefinitionRecalculatedBasedOnValid(result);
}