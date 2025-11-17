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
[SuppressMessage("ReSharper", "RedundantIfElseBlock", Justification = "Depends on USE_OPENMP compile-time constant")]
#pragma warning disable CS0162 // Unreachable code detected
internal class MinimizationResultAssertions(IMinimizationResult value)
    : ObjectAssertions<IMinimizationResult, MinimizationResultAssertions>(value, AssertionChain.GetOrCreate())
{
#if USE_OPENMP
    private const bool SkipFunctionCallsAssertions = true;
#else
    private const bool SkipFunctionCallsAssertions = false;
#endif
    private const int SimilarFunctionCallsTolerance = 6;

    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCalls(int expectedValue)
    {
        if (!SkipFunctionCallsAssertions)
            Subject.NumberOfFunctionCalls.Should().Be(expectedValue);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCallsCloseTo(int expectedValue)
    {
        if (!SkipFunctionCallsAssertions)
            Subject.NumberOfFunctionCalls.Should().BeInRange(
                expectedValue - SimilarFunctionCallsTolerance,
                expectedValue + SimilarFunctionCallsTolerance);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}
#pragma warning restore CS0162 // Unreachable code detected