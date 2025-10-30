namespace minuit2.net.Minimizers;

internal class MigradMinimizer : MnMinimizer
{
    protected sealed override FunctionMinimum MnMinimize(
        FCNWrap costFunction, 
        MnUserParameterState parameterState, 
        MnStrategy strategy, 
        uint maximumFunctionCalls, 
        double tolerance)
    {
        using var migrad = new MnMigradWrap(costFunction, parameterState, strategy);
        return migrad.Run(maximumFunctionCalls, tolerance);
    }
}