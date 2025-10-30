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
internal class MinimizationResultAssertions(IMinimizationResult value)
    : ObjectAssertions<IMinimizationResult, MinimizationResultAssertions>(value, AssertionChain.GetOrCreate())
{
    #if USE_OPENMP
        private const int NumberOfFunctionCallsTolerance = 12;
    #else
        private const int NumberOfFunctionCallsTolerance = 6;
    #endif
    
    public AndConstraint<MinimizationResultAssertions> HaveNumberOfFunctionCallsCloseTo(int expectedValue)
    {
        Subject.NumberOfFunctionCalls.Should().BeInRange(
            expectedValue - NumberOfFunctionCallsTolerance,
            expectedValue + NumberOfFunctionCallsTolerance);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}