using System.Diagnostics.CodeAnalysis;
using minuit2.net.Exceptions;

namespace minuit2.net.CostFunctions;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", 
    Justification = "Cancellation token is stored for use in runtime cancellation checks across multiple method calls.")]
internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken) : FCNWrap
{
    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    public override double Up() => 1;

    public override double Cost(VectorDouble parameterValues)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new MinimizationCancelledException(parameterValues);
        
        var value = function.ValueFor(parameterValues) / function.ErrorDefinition;
        return double.IsFinite(value) ? value : throw new NonFiniteCostValueException(parameterValues);
    }

    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        var gradients = new VectorDouble(function.GradientFor(parameterValues).Select(g => g / function.ErrorDefinition));
        return gradients.All(double.IsFinite) ? gradients : throw new NonFiniteCostGradientException(parameterValues);
    }

    public override bool HasGradient() => function.HasGradient;
}