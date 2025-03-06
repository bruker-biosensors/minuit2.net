using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Primitives;
using minuit2.net;

namespace minuit2.UnitTests;

internal static class MinimizationResultAssertionExtensions
{
    public static MinimizationResultAssertions Should(this MinimizationResult actualValue) => new(actualValue);
}

[DebuggerStepThrough]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal class MinimizationResultAssertions(MinimizationResult value)
    : ObjectAssertions<MinimizationResult, MinimizationResultAssertions>(value)
{
    private const double RelativeTolerance = 0.001;
    
    public AndConstraint<MinimizationResultAssertions> HaveCostValue(double expectedValue)
    {
        Subject.CostValue.Should().BeApproximately(expectedValue, Math.Abs(expectedValue * RelativeTolerance));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
    
    public AndConstraint<MinimizationResultAssertions> HaveParameterValues(IReadOnlyCollection<double> expectedValues)
    {
        Subject.ParameterValues.Should().BeEquivalentTo(expectedValues, options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * RelativeTolerance)))
            .WhenTypeIs<double>());
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameterCovarianceMatrix(double[,] expectedValues)
    {
        Subject.ParameterCovarianceMatrix.Should().BeEquivalentTo(expectedValues, options => options
            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, Math.Abs(ctx.Expectation * RelativeTolerance)))
            .WhenTypeIs<double>());
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveIsValid(bool expectedValue)
    {
        Subject.IsValid.Should().Be(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
    
    public AndConstraint<MinimizationResultAssertions> HaveNumberOfVariables(int expectedValue)
    {
        Subject.NumberOfVariables.Should().Be(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveReachedFunctionCallLimit(bool expectedValue)
    {
        Subject.HasReachedFunctionCallLimit.Should().Be(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCallsGreaterThan(int expectedValue)
    {
        Subject.NumberOfFunctionCalls.Should().BeGreaterThan(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveConverged(bool expectedValue)
    {
        Subject.HasConverged.Should().Be(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}
