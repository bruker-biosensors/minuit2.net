namespace minuit2.net;

public record MigradMinimizerConfiguration(
    MinimizationStrategy Strategy = MinimizationStrategy.Balanced,
    uint MaximumFunctionCalls = 0,
    double Tolerance = 0.1);