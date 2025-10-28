using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net.CostFunctions;

[SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", 
    Justification = "Cancellation token is stored for use in runtime cancellation checks across multiple method calls.")]
internal sealed class CostFunctionAdapter(ICostFunction function, CancellationToken cancellationToken) : FCNWrap
{
    private const double AbortValue = double.NaN;  // Non-finite return values trigger termination of the C++ process.
    
    private int _numberOfFunctionCalls;
    private int _numberOfExceptions;

    public ConcurrentQueue<Exception> Exceptions { get; } = new();

    // We always forward a neutral error definition (Up) of 1 to the C++ code. Instead, we scale the output values of
    // the inner ValueFor() and GradientFor() by the error definition directly. This allows us to dynamically adjust
    // error definitions, e.g. to get meaningful parameter uncertainties for least squares cost functions with unknown
    // data errors (see LeastSquaresWithUnknownYError.cs).
    public override double Up() => 1;

    public override double Cost(VectorDouble parameterValues)
    {
        // Ensures abort on non-finite gradients and skips cost evaluation for any post-abort calls.
        if (_numberOfExceptions > 0) return AbortValue; 
        
        Interlocked.Increment(ref _numberOfFunctionCalls);

        if (cancellationToken.IsCancellationRequested)
        {
            Register(new MinimizationAbort(ManuallyStopped, parameterValues, _numberOfFunctionCalls));
            return AbortValue;
        }
        
        try
        {
            var value = ValueFor(parameterValues);
            if (double.IsFinite(value)) return value;
            
            Register(new MinimizationAbort(NonFiniteValue, parameterValues, _numberOfFunctionCalls));
            return AbortValue;
        }
        catch (Exception exception)
        {
            Register(exception);
            return AbortValue;
        }
    }

    public override VectorDouble Gradient(VectorDouble parameterValues)
    {
        try
        {
            var gradient = GradientFor(parameterValues);
            if (gradient.All(double.IsFinite)) return gradient;

            Register(new MinimizationAbort(NonFiniteGradient, parameterValues, _numberOfFunctionCalls));
            return AbortGradientFor(parameterValues);
        }
        catch (Exception exception)
        {
            Register(exception);
            return AbortGradientFor(parameterValues);
        }
    }
    
    public override bool HasGradient() => function.HasGradient;

    private void Register(Exception exception)
    {
        Interlocked.Increment(ref _numberOfExceptions);
        Exceptions.Enqueue(exception);
    }
    
    private double ValueFor(VectorDouble parameterValues) =>
        function.ValueFor(parameterValues.AsReadOnly()) / function.ErrorDefinition;

    private VectorDouble GradientFor(VectorDouble parameterValues) =>
        new(function.GradientFor(parameterValues.AsReadOnly()).Select(g => g / function.ErrorDefinition));
    
    private static VectorDouble AbortGradientFor(VectorDouble parameterValues) => 
        VectorDouble.Repeat(AbortValue, parameterValues.Count);
}