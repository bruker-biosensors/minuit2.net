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
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    public override double Up() => 1;

    public override double CalculateValue(VectorDouble parameterValues)
    {
        Interlocked.Increment(ref _numberOfFunctionCalls);
        
        if (cancellationToken.IsCancellationRequested)
        {
            AbortWith(new MinimizationAbort(ManuallyStopped, parameterValues, _numberOfFunctionCalls));
            return double.NaN;  // return directly to skip (potentially expensive) value evaluation
        }
        
        try
        {
            var value = ValueFor(parameterValues);
            if (double.IsFinite(value)) return value;
            
            AbortWith(new MinimizationAbort(NonFiniteValue, parameterValues, _numberOfFunctionCalls));
        }
        catch (Exception exception)
        {
            AbortWith(exception);
        }

        return double.NaN;
    }

    public override VectorDouble CalculateGradient(VectorDouble parameterValues)
    {
        try
        {
            var gradient = GradientFor(parameterValues);
            if (gradient.All(double.IsFinite)) return gradient;

            AbortWith(new MinimizationAbort(NonFiniteGradient, parameterValues, _numberOfFunctionCalls));
        }
        catch (Exception exception)
        {
            AbortWith(exception);
        }

        return VectorDouble.Repeat(double.NaN, parameterValues.Count);
    }
    
    public override bool HasGradient() => function.HasGradient;
    
    private double ValueFor(VectorDouble parameterValues) =>
        function.ValueFor(parameterValues.AsReadOnly()) / function.ErrorDefinition;

    private VectorDouble GradientFor(VectorDouble parameterValues) =>
        new(function.GradientFor(parameterValues.AsReadOnly()).Select(g => g / function.ErrorDefinition));
    
    private void AbortWith(Exception exception)
    {
        Exceptions.Enqueue(exception);
        Abort();
    }
}