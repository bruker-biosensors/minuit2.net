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
    
    public AndConstraint<MigradResultAssertions> HaveCostValue(double expectedValue)
    {
        Subject.CostValue.Should().BeApproximately(expectedValue, Math.Abs(expectedValue * RelativeTolerance));
        return new AndConstraint<MigradResultAssertions>(this);
    }
    
    public AndConstraint<MigradResultAssertions> HaveParameterValues(IReadOnlyCollection<double> expectedValues)
    {
        Subject.ParameterValues.Should().BeEquivalentTo(expectedValues, options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * RelativeTolerance)))
            .WhenTypeIs<double>());
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> HaveParameterCovarianceMatrix(double[,] expectedValues)
    {
        Subject.ParameterCovarianceMatrix.Should().BeEquivalentTo(expectedValues, options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * RelativeTolerance)))
            .WhenTypeIs<double>());
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> HaveIsValid(bool expectedIsValid)
    {
        Subject.IsValid.Should().Be(expectedIsValid);
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> HaveReachedFunctionCallLimit(bool expectedHasReachedCallLimit)
    {
        Subject.HasReachedFunctionCallLimit.Should().Be(expectedHasReachedCallLimit);
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> HaveNumberOfFunctionCallsGreaterThan(int expectedMinimumNumber)
    {
        Subject.NumberOfFunctionCalls.Should().BeGreaterThan(expectedMinimumNumber);
        return new AndConstraint<MigradResultAssertions>(this);
    }

    public AndConstraint<MigradResultAssertions> HaveConverged(bool expectedHasConverged)
    {
        Subject.HasConverged.Should().Be(expectedHasConverged);
        return new AndConstraint<MigradResultAssertions>(this);
    }
}
