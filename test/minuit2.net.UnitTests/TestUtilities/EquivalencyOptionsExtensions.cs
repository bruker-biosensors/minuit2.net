using AwesomeAssertions;
using AwesomeAssertions.Equivalency;
using static minuit2.net.UnitTests.TestUtilities.NumericAssertionExtensions;

namespace minuit2.net.UnitTests.TestUtilities;

internal static class EquivalencyOptionsExtensions
{
    public static EquivalencyOptions<double> WithSmartDoubleTolerance(
        this EquivalencyOptions<double> options,
        double relativeToleranceForNonZeros, 
        double? minimumToleranceForNonZeros = null,
        double? toleranceForZeros = null)
    {
        return options.WithSmartDoubleTolerance<double>(
            relativeToleranceForNonZeros,
            minimumToleranceForNonZeros,
            toleranceForZeros);
    }
    
    public static EquivalencyOptions<double[,]> WithSmartDoubleTolerance(
        this EquivalencyOptions<double[,]> options, 
        double relativeToleranceForNonZeros,
        double? minimumToleranceForNonZeros = null,
        double? toleranceForZeros = null)
    {
        return options.WithSmartDoubleTolerance<double[,]>(
            relativeToleranceForNonZeros,
            minimumToleranceForNonZeros,
            toleranceForZeros);
    }
    
    public static EquivalencyOptions<IMinimizationResult> WithSmartDoubleTolerance(
        this EquivalencyOptions<IMinimizationResult> options, 
        double relativeToleranceForNonZeros, 
        double? minimumToleranceForNonZeros = null,
        double? toleranceForZeros = null)
    {
        return options.WithSmartDoubleTolerance<IMinimizationResult>(
            relativeToleranceForNonZeros,
            minimumToleranceForNonZeros,
            toleranceForZeros);
    }

    private static EquivalencyOptions<T> WithSmartDoubleTolerance<T>(
        this EquivalencyOptions<T> options,
        double relativeToleranceForNonZeros,
        double? minimumToleranceForNonZeros,
        double? toleranceForZeros)
    {
        return options
            .Using<double>(ctx =>
            {
                var actual = ctx.Subject;
                var expected = ctx.Expectation;
                if (Math.Abs(expected) > double.Epsilon)
                {
                    var minimumTolerance = minimumToleranceForNonZeros ?? DefaultDoubleTolerance;
                    actual.Should().BeApproximately(expected, relativeToleranceForNonZeros, minimumTolerance);
                }
                else
                {
                    var tolerance = toleranceForZeros ?? DefaultDoubleTolerance;
                    actual.Should().BeApproximately(expected, tolerance);
                }
            })
            .WhenTypeIs<double>();
    }
}