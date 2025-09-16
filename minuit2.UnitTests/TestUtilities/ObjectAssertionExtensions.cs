using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Primitives;
using static minuit2.UnitTests.TestUtilities.NumericAssertionExtensions;

namespace minuit2.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class ObjectAssertionExtensions
{
    public static AndConstraint<ObjectAssertions> ShouldFulfill<T>(this T subject, Action<T> assertion) =>
        subject.Should().Satisfy(assertion);
    
    public static AndConstraint<ObjectAssertions> BeApproximately(
        this ObjectAssertions parent, 
        double[,] expectation, 
        [StringSyntax("CompositeFormat")] string because = "", 
        params object[] becauseArgs)
    {
        return parent.BeApproximately(expectation, DefaultRelativeDoubleTolerance, because, becauseArgs);
    }
    
    public static AndConstraint<ObjectAssertions> BeApproximately(
        this ObjectAssertions parent, 
        double[,] expectation, 
        double relativeTolerance,
        [StringSyntax("CompositeFormat")] string because = "", 
        params object[] becauseArgs)
    {
        return parent.BeEquivalentTo(
            expectation, 
            options => options.WithRelativeDoubleTolerance(relativeTolerance), 
            because,
            becauseArgs);
    }
}