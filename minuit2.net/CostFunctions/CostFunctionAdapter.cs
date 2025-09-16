namespace minuit2.net.CostFunctions;

internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken)
    : FCNWrap, ICostFunctionMonitor
{
    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    public override double Up() => 1;

    public override double Cost(VectorDouble parameterValues)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        LastParameterValues = parameterValues.ToArray();
        var value = function.ValueFor(parameterValues) / function.ErrorDefinition;
        if (!double.IsFinite(value)) throw new NonFiniteCostValueException();
        
        NumberOfValidFunctionCalls++;
        LastValidParameterValues = parameterValues.ToArray();
        return value;
    }

    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        LastParameterValues = parameterValues.ToArray();
        var gradients = new VectorDouble(function.GradientFor(parameterValues).Select(g => g / function.ErrorDefinition));
        return gradients.All(double.IsFinite) ? gradients : throw new NonFiniteCostGradientException();
    }

    public override bool HasGradient() => function.HasGradient;

    public IReadOnlyCollection<double> LastParameterValues { get; private set; } = [];
    public IReadOnlyCollection<double> LastValidParameterValues { get; private set; } = [];
    public int NumberOfValidFunctionCalls { get; private set; }
}