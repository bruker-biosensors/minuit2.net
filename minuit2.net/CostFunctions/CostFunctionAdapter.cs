using System.Diagnostics.CodeAnalysis;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net.CostFunctions;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local",
    Justification =
        "Cancellation token is stored for use in runtime cancellation checks across multiple method calls.")]
internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken) : FCNWrap
{
    private int _numberOfFunctionCalls;

    public MinimizationAbort? AbortReason { get; private set; }

    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    public override double Up() => 1;

    public override double Cost(VectorDouble parameterValues)
    {
        Interlocked.Increment(ref _numberOfFunctionCalls);

        if (cancellationToken.IsCancellationRequested)
            this.AbortMinimization(ManuallyStopped, parameterValues, _numberOfFunctionCalls, string.Empty);

        try
        {
            double value = ValueFor(parameterValues);
            if (!double.IsFinite(value)) this.AbortMinimization(NonFiniteValue, parameterValues, _numberOfFunctionCalls, "Non-finite value.");
            return value;
        }
        catch (Exception ex)
        {
            AbortMinimization(MinimizationError, parameterValues, _numberOfFunctionCalls, ex.Message);
            return base.Cost(parameterValues);
        }
    }

    public override VectorDouble CalculateGradient(VectorDouble parameterValues)
    {
        try
        {
            var gradient = GradientFor(parameterValues);
            if (!gradient.All(double.IsFinite))
                AbortMinimization(NonFiniteGradient, parameterValues, _numberOfFunctionCalls, "Non-finite gradient.");
            return gradient;
        }
        catch(Exception ex)
        {
            AbortMinimization(MinimizationError, parameterValues, _numberOfFunctionCalls, ex.Message);
            return base.CalculateGradient(parameterValues);
        }
    }

    public override bool HasGradient() => function.HasGradient;

    private double ValueFor(VectorDouble parameterValues) =>
        function.ValueFor(parameterValues.AsReadOnly()) / function.ErrorDefinition;

    private VectorDouble GradientFor(VectorDouble parameterValues) =>
        new(function.GradientFor(parameterValues.AsReadOnly()).Select(g => g / function.ErrorDefinition));

    private void AbortMinimization(
        MinimizationExitCondition exitCondition,
        IEnumerable<double> parameterValues,
        int numberOfFunctionCalls,
        string message)
    {
        if (this.AbortReason is not null) return;
        this.AbortReason = new MinimizationAbort(exitCondition, parameterValues, numberOfFunctionCalls);
        this.Abort(exitCondition == ManuallyStopped || exitCondition == NonFiniteGradient || exitCondition == NonFiniteValue, message);
    }
}
