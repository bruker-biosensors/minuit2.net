using System.Diagnostics.CodeAnalysis;
using static minuit2.net.MinimizationExitCondition;

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
            throw new MinimizationAbort(ManuallyStopped, parameterValues);
        
        var value = ValueFor(parameterValues);
        return double.IsFinite(value) ? value : throw new MinimizationAbort(NonFiniteValue, parameterValues);
    }
    
    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        var gradient = GradientFor(parameterValues);
        return gradient.All(double.IsFinite) ? gradient : throw new MinimizationAbort(NonFiniteGradient, parameterValues);
    }
    
    public override bool HasGradient() => function.HasGradient;

    private double ValueFor(VectorDouble parameterValues) =>
        function.ValueFor(parameterValues.AsReadOnly()) / function.ErrorDefinition;

    private VectorDouble GradientFor(VectorDouble parameterValues) =>
        new(function.GradientFor(parameterValues.AsReadOnly()).Select(g => g / function.ErrorDefinition));
}