using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net.CostFunctions;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", 
    Justification = "Cancellation token is stored for use in runtime cancellation checks across multiple method calls.")]
internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken) : FCNWrap
{
    private int _numberOfFunctionCalls;

    public ConcurrentQueue<Exception> Exceptions { get; } = new();

    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions.
    public override double Up() => 1;

    public override bool HasGradient() => function.HasGradient;

    public override double Value(VectorDouble parameterValues)
    {
        Interlocked.Increment(ref _numberOfFunctionCalls);

        try
        {
            return ValueFor(parameterValues);
        }
        catch (Exception exception)
        {
            Exceptions.Enqueue(exception);
            RequestAbort();  // Immediate abort via exception is unsafe here; see FCNWrap.h for details
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
            AbortImmediately();
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