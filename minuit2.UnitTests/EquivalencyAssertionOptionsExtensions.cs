using FluentAssertions;
using FluentAssertions.Equivalency;
using minuit2.net;

namespace minuit2.UnitTests;

internal static class EquivalencyAssertionOptionsExtensions
{
    public static EquivalencyAssertionOptions<double> WithRelativeDoubleTolerance(
        this EquivalencyAssertionOptions<double> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<double>(relativeTolerance);
    }
    
    public static EquivalencyAssertionOptions<double[,]> WithRelativeDoubleTolerance(
        this EquivalencyAssertionOptions<double[,]> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<double[,]>(relativeTolerance);
    }
    
    public static EquivalencyAssertionOptions<MinimizationResult> WithRelativeDoubleTolerance(
        this EquivalencyAssertionOptions<MinimizationResult> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<MinimizationResult>(relativeTolerance);
    }

    private static EquivalencyAssertionOptions<T> WithRelativeDoubleTolerance<T>(this EquivalencyAssertionOptions<T> options, double relativeTolerance)
    {
        return options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * relativeTolerance)))
            .WhenTypeIs<double>();
    }
    
    public static EquivalencyAssertionOptions<double[,]> WithDoubleTolerance(
        this EquivalencyAssertionOptions<double[,]> options, double relativeTolerance)
    {
        return options.WithDoubleTolerance<double[,]>(relativeTolerance);
    }
    
    private static EquivalencyAssertionOptions<T> WithDoubleTolerance<T>(this EquivalencyAssertionOptions<T> options, double tolerance)
    {
        return options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, tolerance))
            .WhenTypeIs<double>();
    }
}