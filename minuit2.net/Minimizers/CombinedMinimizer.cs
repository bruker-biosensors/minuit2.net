namespace minuit2.net.Minimizers;

internal class CombinedMinimizer : MnMinimizer
{
    protected override FunctionMinimum MnMinimize(
        FCNWrap costFunction, 
        MnUserParameterState parameterState, 
        MnStrategy strategy,
        uint maximumFunctionCalls, 
        double tolerance)
    {
        using var combined = new MnMinimizeWrap(costFunction, parameterState, strategy);
        return combined.Run(maximumFunctionCalls, tolerance);
    }
}