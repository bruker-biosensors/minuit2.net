using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Collections;
using static minuit2.UnitTests.TestUtilities.NumericAssertionExtensions;

namespace minuit2.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class GenericCollectionAssertionExtensions
{
    public static AndConstraint<GenericCollectionAssertions<T>> Fulfill<T>(
        this GenericCollectionAssertions<T> parent,
        Action<IEnumerable<T>> assertion)
    {
        return parent.Satisfy(assertion);
    }

    public static AndConstraint<GenericCollectionAssertions<double>> BeApproximately(
        this GenericCollectionAssertions<double> parent,
        IEnumerable<double> expectation,
        [StringSyntax("CompositeFormat")] string because = "",
        params object[] becauseArgs)
    {
        return parent.BeApproximately(expectation, DefaultRelativeDoubleTolerance, because, becauseArgs);
    }
    
    public static AndConstraint<GenericCollectionAssertions<double>> BeApproximately(
        this GenericCollectionAssertions<double> parent,
        IEnumerable<double> expectation,
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