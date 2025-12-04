using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net.CostFunctions;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local",
    Justification = "Cancellation token is stored for use in runtime cancellation checks across multiple method calls.")]
internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken) : FCNWrap
{
    private int _numberOfFunctionCalls;
    private int _shouldReturnNaNValue;
    private bool ShouldReturnNanValue => _shouldReturnNaNValue > 0;

    public ConcurrentQueue<Exception> Exceptions { get; } = new();

    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions.
    public override double Up() => 1;

    public override bool HasGradient() => function.HasGradient;

    public override double Value(VectorDouble parameterValues)
    {
        Interlocked.Increment(ref _numberOfFunctionCalls);

        // When OpenMP is enabled, this method is invoked concurrently by multiple threads. Terminating the calling
        // C++ process via an exception is unsafe in this case: across threads and the C#/C++ boundary it leads to
        // crashes/memory corruption.
        // Instead, we rely on the fact that the C++ processes terminate gracefully when encountering non‑finite values.
        // Note: Termination may occur only after a few additional invocations.
        if (ShouldReturnNanValue) return double.NaN;

        try
        {
            return ValueFor(parameterValues);
        }
        catch (Exception exception)
        {
            Exceptions.Enqueue(exception);
            Interlocked.Increment(ref _shouldReturnNaNValue);
            return double.NaN;
        }
    }

    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        try
        {
            return GradientFor(parameterValues);
        }
        catch (Exception exception)
        {
            Exceptions.Enqueue(exception);
            Abort();
            return [];
        }
    }

    private double ValueFor(VectorDouble parameterValues)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new MinimizationAbort(ManuallyStopped, parameterValues, _numberOfFunctionCalls);

        var value = function.ValueFor(parameterValues.AsReadOnly()) / function.ErrorDefinition;
        return double.IsFinite(value)
            ? value
            : throw new MinimizationAbort(NonFiniteValue, parameterValues, _numberOfFunctionCalls);
    }

    private VectorDouble GradientFor(VectorDouble parameterValues)
    {
        var gradient = new VectorDouble(function.GradientFor(parameterValues.AsReadOnly()).Select(g => g / function.ErrorDefinition));
        return gradient.All(double.IsFinite)
            ? gradient
            : throw new MinimizationAbort(NonFiniteGradient, parameterValues, _numberOfFunctionCalls);
    }
}