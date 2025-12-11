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

    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values by
    // the error definition directly (see below). This allows us to adjust error definitions dynamically.
    public override double Up() => 1;

    public override bool HasGradient() => function.HasGradient;

    public override bool HasHessian() => function.HasHessian;

    public override bool HasG2() => function.HasHessianDiagonal;

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
            return AbortWith(exception);
        }
    }
    
    public override VectorDouble Hessian(VectorDouble parameterValues)
    {
        try
        {
            return HessianFor(parameterValues);
        }
        catch (Exception exception)
        {
            return AbortWith(exception);
        }
    }
    
    public override VectorDouble G2(VectorDouble parameterValues)
    {
        try
        {
            return HessianDiagonalFor(parameterValues);
        }
        catch (Exception exception)
        {
            return AbortWith(exception);
        }
    }

    private double ValueFor(VectorDouble parameterValues)
    {
        if (cancellationToken.IsCancellationRequested)
            throw new MinimizationAbort(ManuallyStopped, parameterValues, _numberOfFunctionCalls);

        var value = ErrorDefinitionAdjusted(function.ValueFor(parameterValues.AsReadOnly()));
        return double.IsFinite(value)
            ? value
            : throw new MinimizationAbort(NonFiniteValue, parameterValues, _numberOfFunctionCalls);
    }

    private VectorDouble GradientFor(VectorDouble parameterValues)
    {
        var gradient = new VectorDouble(function.GradientFor(parameterValues.AsReadOnly()).Select(ErrorDefinitionAdjusted));
        return gradient.All(double.IsFinite)
            ? gradient
            : throw new MinimizationAbort(NonFiniteGradient, parameterValues, _numberOfFunctionCalls);
    }
    
    private VectorDouble HessianFor(VectorDouble parameterValues)
    {
        var hessian = new VectorDouble(function.HessianFor(parameterValues.AsReadOnly()).Select(ErrorDefinitionAdjusted));
        return hessian.All(double.IsFinite)
            ? hessian
            : throw new MinimizationAbort(NonFiniteHessian, parameterValues, _numberOfFunctionCalls);
    }
    
    private VectorDouble HessianDiagonalFor(VectorDouble parameterValues)
    {
        var g2 = new VectorDouble(function.HessianDiagonalFor(parameterValues.AsReadOnly()).Select(ErrorDefinitionAdjusted));
        return g2.All(double.IsFinite)
            ? g2
            : throw new MinimizationAbort(NonFiniteHessianDiagonal, parameterValues, _numberOfFunctionCalls);
    }

    private double ErrorDefinitionAdjusted(double value) => value / function.ErrorDefinition;
    
    private VectorDouble AbortWith(Exception exception)
    {
        Exceptions.Enqueue(exception);
        Abort();
        return [];
    }
}