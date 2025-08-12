namespace minuit2.net.Minimizers;

public record MinimizerConfiguration(
    Strategy Strategy = Strategy.Balanced,
    uint MaximumFunctionCalls = 0,
    double Tolerance = 0.1);