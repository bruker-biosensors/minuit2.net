namespace minuit2.net;

public record MigradMinimizerConfiguration(
    Strategy Strategy = Strategy.Balanced,
    uint MaximumFunctionCalls = 0,
    double Tolerance = 0.1);