namespace minuit2.net.Minimizers;

internal class CombinedMinimizer : MnMinimizer
{
    protected override MinimizationRunner BuildMinimizer(
        FcnFacade costFunction,
        MnUserParameterState parameterState,
        MnStrategy strategy)
    {
        return new MnMinimizeWrap(costFunction, parameterState, strategy);
    }
}
