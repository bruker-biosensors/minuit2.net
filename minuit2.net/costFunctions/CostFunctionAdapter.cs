namespace minuit2.net.costFunctions;

internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken = default)
    : FCNWrap
{
    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to auto-scale parameter
    // uncertainties for missing data y-errors after minimization using the Hesse algorithm (impossible otherwise),
    // plus it makes handling of individual cost function and cost function sums consistent (mind that this means that
    // the cost function value resulting from the minimization must be rescaled by the error definition!).
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