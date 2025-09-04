using AwesomeAssertions;
using AwesomeAssertions.Equivalency;
using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

internal static class EquivalencyAssertionOptionsExtensions
{
    public static EquivalencyOptions<double> WithRelativeDoubleTolerance(
        this EquivalencyOptions<double> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<double>(relativeTolerance);
    }
    
    public static EquivalencyOptions<double[,]> WithRelativeDoubleTolerance(
        this EquivalencyOptions<double[,]> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<double[,]>(relativeTolerance);
    }
    
    public static EquivalencyOptions<IMinimizationResult> WithRelativeDoubleTolerance(
        this EquivalencyOptions<IMinimizationResult> options, double relativeTolerance)
    {
        return options.WithRelativeDoubleTolerance<IMinimizationResult>(relativeTolerance);
    }

    private static EquivalencyOptions<T> WithRelativeDoubleTolerance<T>(this EquivalencyOptions<T> options, double relativeTolerance)
    {
        return options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * relativeTolerance)))
            .WhenTypeIs<double>();
    }
    
    public static EquivalencyOptions<double[,]> WithDoubleTolerance(
        this EquivalencyOptions<double[,]> options, double relativeTolerance)
    {
        return options.WithDoubleTolerance<double[,]>(relativeTolerance);
    }
    
    private static EquivalencyOptions<T> WithDoubleTolerance<T>(this EquivalencyOptions<T> options, double tolerance)
    {
        return options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, tolerance))
            .WhenTypeIs<double>();
    }
}