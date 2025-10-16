namespace minuit2.net.Minimizers;

internal class SimplexMinimizer : MnMinimizer
{
    protected override MinimizationRunner BuildMinimizer(
        FcnFacade costFunction,
        MnUserParameterState parameterState,
        MnStrategy strategy)
    {
        return new MnSimplexWrap(costFunction, parameterState, strategy);
    }
}
