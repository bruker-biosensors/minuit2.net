using FluentAssertions;
using FluentAssertions.Equivalency;
using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

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
    
    public static EquivalencyAssertionOptions<IMinimizationResult> WithRelativeDoubleTolerance(
        this EquivalencyAssertionOptions<IMinimizationResult> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<IMinimizationResult>(relativeTolerance);
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