using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

internal static class MinimizationResultAssertionExtensions
{
    public static MinimizationResultAssertions Should(this IMinimizationResult actualValue) => new(actualValue);
}

[DebuggerStepThrough]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal class MinimizationResultAssertions(IMinimizationResult value)
    : ObjectAssertions<IMinimizationResult, MinimizationResultAssertions>(value, AssertionChain.GetOrCreate())
{
    public AndConstraint<MinimizationResultAssertions> HaveCostValue(double expectedValue, double relativeTolerance = 0.001)
    {
        Subject.CostValue.Should().BeApproximately(expectedValue, Math.Abs(expectedValue * relativeTolerance));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameters(IEnumerable<string> expectedValues)
    {
        Subject.Parameters.Should().Equal(expectedValues);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameterValues(IReadOnlyCollection<double> expectedValues, double relativeTolerance = 0.001)
    {
        Subject.ParameterValues.Should().BeEquivalentTo(expectedValues, options => options.WithRelativeDoubleTolerance(relativeTolerance));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveParameterCovarianceMatrix(double[,] expectedValues, double relativeTolerance = 0.001)
    {
        Subject.ParameterCovarianceMatrix.Should().BeEquivalentTo(expectedValues, options => options.WithRelativeDoubleTolerance(relativeTolerance));
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
        const int tolerance = 6;
        Subject.NumberOfFunctionCalls.Should().BeInRange(expectedValue - tolerance, expectedValue + tolerance);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveExitCondition(MinimizationExitCondition exitCondition)
    {
        Subject.ExitCondition.Should().Be(exitCondition);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}
