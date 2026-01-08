using AwesomeAssertions;
using AwesomeAssertions.Equivalency;
using static minuit2.net.UnitTests.TestUtilities.NumericAssertionExtensions;

namespace minuit2.net.UnitTests.TestUtilities;

internal static class EquivalencyOptionsExtensions
{
    public static EquivalencyOptions<double> WithRelativeDoubleTolerance(
        this EquivalencyOptions<double> options,
        double relativeTolerance)
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
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, relativeTolerance, DefaultMinimumDoubleTolerance))
            .WhenTypeIs<double>();
    }
}