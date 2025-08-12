using minuit2.net.CostFunctions;

namespace minuit2.net.Minimizers;

public interface IMinimizer
{
    IMinimizationResult Minimize(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations,
        MinimizerConfiguration? minimizerConfiguration = null, 
        CancellationToken cancellationToken = default);
}