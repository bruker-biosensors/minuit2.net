using minuit2.net.Exceptions;

namespace minuit2.net.CostFunctions;

internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken)
    : FCNWrap, ICostFunctionMonitor
{
    private double _bestValue = double.PositiveInfinity;

    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    public override double Up() => 1;

    public override double Cost(VectorDouble parameterValues)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var value = function.ValueFor(parameterValues) / function.ErrorDefinition;
        if (!double.IsFinite(value))
        {
            IssueParameterValues = parameterValues.ToArray();
            throw new NonFiniteCostValueException();
        }

        if (value < _bestValue)
        {
            _bestValue = value;
            BestParameterValues = parameterValues.ToArray();
        }
        
        NumberOfFunctionCalls++;
        return value;
    }

    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        var gradients = new VectorDouble(function.GradientFor(parameterValues).Select(g => g / function.ErrorDefinition));
        if (!gradients.All(double.IsFinite))
        {
            IssueParameterValues = parameterValues.ToArray();
            throw new NonFiniteCostGradientException();
        }

        return gradients;
    }

    public override bool HasGradient() => function.HasGradient;

    // Monitoring
    public int NumberOfFunctionCalls { get; private set; }
    public IReadOnlyCollection<double>? BestParameterValues { get; private set; }
    public IReadOnlyCollection<double>? IssueParameterValues { get; private set; }
}