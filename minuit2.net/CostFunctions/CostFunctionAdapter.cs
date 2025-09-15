namespace minuit2.net.CostFunctions;

internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken)
    : FCNWrap, ICostFunctionMonitor
{
    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    // Mind that this means that the final cost function value must be rescaled by the error definition!
    public override double Up() => 1;

    public override double Cost(VectorDouble parameterValues)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var value = function.ValueFor(parameterValues) / function.ErrorDefinition;
        if (!double.IsFinite(value) && NonFiniteValueParametersValues is null)
            NonFiniteValueParametersValues = parameterValues;
        
        return value;
    }

    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        var gradients = new VectorDouble(function.GradientFor(parameterValues).Select(g => g / function.ErrorDefinition));
        if (!gradients.All(double.IsFinite) && NonFiniteGradientParameterValues is null)
            NonFiniteGradientParameterValues = parameterValues;
        
        return gradients;
    }

    public override bool HasGradient() => function.HasGradient;
    
    public IEnumerable<double>? NonFiniteValueParametersValues { get; private set; }
    public IEnumerable<double>? NonFiniteGradientParameterValues { get; private set; }
}