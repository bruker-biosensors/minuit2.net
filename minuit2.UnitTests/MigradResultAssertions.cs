using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Primitives;
using minuit2.net;

namespace minuit2.UnitTests;

internal static class MigradResultAssertionExtensions
{
    public static MigradResultAssertions Should(this MigradResult actualValue) => new(actualValue);
}

[DebuggerStepThrough]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal class MigradResultAssertions(MigradResult value) : ObjectAssertions<MigradResult, MigradResultAssertions>(value, null)
{
    private const double RelativeTolerance = 0.001;
    
    public AndConstraint<MigradResultAssertions> HaveBestValues(IReadOnlyCollection<double> expectedValues)
    {
        Subject.BestValues.Should().BeEquivalentTo(expectedValues, options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * RelativeTolerance)))
            .WhenTypeIs<double>());
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> HaveCovarianceMatrix(double[,] expectedValues)
    {
        Subject.CovarianceMatrix.Should().BeEquivalentTo(expectedValues, options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * RelativeTolerance)))
            .WhenTypeIs<double>());
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> BeValid()
    {
        Subject.IsValid.Should().BeTrue();
        return new AndConstraint<MigradResultAssertions>(this);
    }
}
