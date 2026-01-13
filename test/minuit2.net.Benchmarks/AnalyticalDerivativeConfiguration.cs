namespace minuit2.net.Benchmarks;

public record AnalyticalDerivativeConfiguration(bool HasGradient, bool HasHessian, bool HasHessianDiagonal)
{
    public static IEnumerable<AnalyticalDerivativeConfiguration> All =>
    [
        new(false, false, false),
        new(true, false, false),
        new(true, true, false),
        new(true, true, true)
    ];
}