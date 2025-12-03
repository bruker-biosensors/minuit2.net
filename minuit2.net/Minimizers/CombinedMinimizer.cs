namespace minuit2.net.Minimizers;

internal class CombinedMinimizer : MnMinimizer
{
    protected override RunResult MnMinimize(
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