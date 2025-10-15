namespace minuit2.net.Minimizers;

internal class MigradMinimizer : MnMinimizer
{
    protected sealed override MinimizationRunner BuildMinimizer(
        FCNWrap costFunction,
        MnUserParameterState parameterState,
        MnStrategy strategy)
    {
        return new MnMigradWrap(costFunction, parameterState, strategy);
    }
}
