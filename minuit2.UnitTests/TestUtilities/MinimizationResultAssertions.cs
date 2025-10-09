using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using FluentAssertions.Primitives;
using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

internal static class MinimizationResultAssertionExtensions
{
    public static MinimizationResultAssertions Should(this IMinimizationResult actualValue) => new(actualValue);
}

[DebuggerStepThrough]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal class MinimizationResultAssertions(IMinimizationResult value)
    : ObjectAssertions<IMinimizationResult, MinimizationResultAssertions>(value)
{
    private const double DefaultRelativeTolerance = 0.001;

    public AndConstraint<MinimizationResultAssertions> HaveCostValue(double expectedValue)
    {
        Subject.CostValue.Should().BeApproximately(expectedValue, Math.Abs(expectedValue * DefaultRelativeTolerance));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameters(IEnumerable<string> expectedValues)
    {
        Subject.Parameters.Should().Equal(expectedValues);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameterValues(IReadOnlyCollection<double> expectedValues)
    {
        Subject.ParameterValues.Should().BeEquivalentTo(expectedValues, options => options.WithRelativeDoubleTolerance(DefaultRelativeTolerance));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameterCovarianceMatrix(double[,] expectedValues, double? relativeTolerance = null)
    {
        Subject.ParameterCovarianceMatrix.Should().BeEquivalentTo(expectedValues, options => options.WithRelativeDoubleTolerance(relativeTolerance ?? DefaultRelativeTolerance));
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

    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCallsCloseTo(int expectedValue)
    {
        const int tolerance = 20;
        Subject.NumberOfFunctionCalls.Should().BeInRange(expectedValue - tolerance, expectedValue + tolerance);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveExitCondition(MinimizationExitCondition exitCondition)
    {
        Subject.ExitCondition.Should().Be(exitCondition);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}
