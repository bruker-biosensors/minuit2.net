using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net;

public static class MigradMinimizer
{
    public static IMinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MigradMinimizerConfiguration? minimizerConfiguration = null, 
        CancellationToken cancellationToken = default)
    {
        ThrowIfParametersAreNotMatchingBetween(costFunction, parameterConfigurations);
        
        minimizerConfiguration ??= new MigradMinimizerConfiguration();
        var result = CoreMinimize(costFunction, parameterConfigurations, minimizerConfiguration, cancellationToken);
        if (!costFunction.RequiresErrorDefinitionAutoScaling || result.ExitCondition == ManuallyStopped) return result;
        
        costFunction.AutoScaleErrorDefinitionBasedOn(result.ParameterValues.ToList(), result.Variables.ToList());
        HesseErrorCalculator.UpdateParameterCovariances(result, costFunction, minimizerConfiguration.Strategy);
        return result;
    }
    
    private static void ThrowIfParametersAreNotMatchingBetween(
        ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations)
    {
        if (parameterConfigurations.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException("The given parameter configurations do not match the parameter names defined " +
                                        $"by the cost function: {costFunction.Parameters}");
    }

    private static IMinimizationResult CoreMinimize(
        ICostFunction costFunction,
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MigradMinimizerConfiguration minimizerConfiguration, 
        CancellationToken cancellationToken)
    {
        using var cost = new CostFunctionAdapter(costFunction, cancellationToken);
        using var parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        using var migrad = new MnMigradWrap(cost, parameterState, minimizerConfiguration.Strategy.AsMnStrategy());
        
        try
        {
            var minimum = migrad.Run(minimizerConfiguration.MaximumFunctionCalls, minimizerConfiguration.Tolerance);
            var edmThreshold = EdmThresholdFor(minimum, minimizerConfiguration.Tolerance);
            return new MinimizationResult(minimum, costFunction, edmThreshold);
        }
        catch (OperationCanceledException)
        {
            return new CancelledMinimizationResult();
        }
    }

    private static double EdmThresholdFor(FunctionMinimum minimum, double tolerance)
    {
        // According to the Minuit2 documentation (https://root.cern.ch/doc/master/Minuit2Page.html),
        // the Migrad minimization stops by convergence when the estimated vertical distance to the minimum (EDM) gets
        // smaller than `0.001 * tolerance * up`.
        
        // According to a discussion in the ROOT forum (https://root-forum.cern.ch/t/minuit1-vs-minuit2-tolerance-and-edm/54421), 
        // there is an additional factor of 2 applied to the threshold to maintain compatibility between Minuit1 and 2.
        
        // The manual re-computation of the EDM threshold should not be necessary because the minimum property
        // IsAboveMaxEdm should report on convergence (according to the iminuit documentation). However, one can easily 
        // show that this is not the case (by forcing a minimization to abort early). For unknown reasons, the value of
        // this flag appears to be always false.
        
        return 0.002 * tolerance * minimum.Up();
    }
}