using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Equivalency;
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
    
    public AndConstraint<MinimizationResultAssertions> Match(
        IMinimizationResult otherResult, 
        Func<EquivalencyOptions<IMinimizationResult>, EquivalencyOptions<IMinimizationResult>>? options = null)
    {
        if (SkipFunctionCallsAssertions)
            return MatchExcludingFunctionCalls(otherResult, options);

        Subject.Should().BeEquivalentTo(otherResult, options ?? (o => o));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }

    public AndConstraint<MinimizationResultAssertions> MatchExcludingFunctionCalls(
        IMinimizationResult otherResult, 
        Func<EquivalencyOptions<IMinimizationResult>, EquivalencyOptions<IMinimizationResult>>? options = null)
    {
        var inputOptions = options ?? (o => o);
        Subject.Should().BeEquivalentTo(otherResult, o => inputOptions(o.Excluding(x => x.NumberOfFunctionCalls)));
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
    
    public AndConstraint<MinimizationResultAssertions> HaveFewerFunctionCallsThan(IMinimizationResult otherResult)
    {
        Subject.NumberOfFunctionCalls.Should().BeLessThan(otherResult.NumberOfFunctionCalls);
        return new AndConstraint<MinimizationResultAssertions>(this);
    }
}
#pragma warning restore CS0162 // Unreachable code detected