namespace minuit2.net.CostFunctions;

internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken = default)
    : FCNWrap
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
        return function.ValueFor(parameterValues) / function.ErrorDefinition;
    }

    public override VectorDouble Gradient(VectorDouble parameterValues) =>
        new(function.GradientFor(parameterValues).Select(g => g / function.ErrorDefinition));

    public override bool HasGradient() => function.HasGradient;
}