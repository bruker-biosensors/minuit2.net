namespace minuit2.net.Minimizers;

internal class SimplexMinimizer : MnMinimizer
{
    protected override FunctionMinimum MnMinimize(
        FCNWrap costFunction, 
        MnUserParameterState parameterState, 
        MnStrategy strategy,
        uint maximumFunctionCalls, 
        double tolerance)
    {
        using var simplex = new MnSimplexWrap(costFunction, parameterState, strategy);
        return simplex.Run(maximumFunctionCalls, tolerance);
    }
}