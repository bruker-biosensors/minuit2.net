using FluentAssertions;
using FluentAssertions.Equivalency;

namespace minuit2.UnitTests;

internal static class EquivalencyAssertionOptionsExtensions
{
    public static EquivalencyAssertionOptions<double> WithinRelativeTolerance(
        this EquivalencyAssertionOptions<double> options, double relativeTolerance)
    {
        return options.WithinRelativeTolerance<double>(relativeTolerance);
    }
    
    public static EquivalencyAssertionOptions<double[,]> WithinRelativeTolerance(
        this EquivalencyAssertionOptions<double[,]> options, double relativeTolerance)
    {
        return options.WithinRelativeTolerance<double[,]>(relativeTolerance);
    }

    private static EquivalencyAssertionOptions<T> WithinRelativeTolerance<T>(this EquivalencyAssertionOptions<T> options, double relativeTolerance)
    {
        return options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * relativeTolerance)))
            .WhenTypeIs<double>();
    }
}