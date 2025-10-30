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

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
[SuppressMessage("ReSharper", "HeuristicUnreachableCode", Justification = "Depends on USE_OPENMP compile-time constant")]
#pragma warning disable CS0162 // Unreachable code detected
internal class MinimizationResultAssertions(IMinimizationResult value)
    : ObjectAssertions<IMinimizationResult, MinimizationResultAssertions>(value, AssertionChain.GetOrCreate())
{
    #if USE_OPENMP
        private const int EqualFunctionCallsTolerance = 2;
        private const int SimilarFunctionCallsTolerance = 22;
    #else
        private const int EqualFunctionCallsTolerance = 0;
        private const int SimilarFunctionCallsTolerance = 6;
    #endif

    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCalls(int expectedValue)
    {
        if (EqualFunctionCallsTolerance > 0)
            Subject.NumberOfFunctionCalls.Should().BeInRange(
                expectedValue - EqualFunctionCallsTolerance,
                expectedValue + EqualFunctionCallsTolerance);
        else
            Subject.NumberOfFunctionCalls.Should().Be(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCallsCloseTo(int expectedValue)
    {
        Subject.NumberOfFunctionCalls.Should().BeInRange(
            expectedValue - SimilarFunctionCallsTolerance,
            expectedValue + SimilarFunctionCallsTolerance);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}
#pragma warning restore CS0162 // Unreachable code detected