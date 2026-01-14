using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Collections;
using static minuit2.net.UnitTests.TestUtilities.NumericAssertionExtensions;

namespace minuit2.net.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class GenericCollectionAssertionExtensions
{
    public static AndConstraint<GenericCollectionAssertions<double>> BeApproximately(
        this GenericCollectionAssertions<double> parent,
        IEnumerable<double> expectation,
        [StringSyntax("CompositeFormat")] string because = "",
        params object[] becauseArgs)
    {
        return parent.BeApproximately(
            expectation,
            DefaultRelativeDoubleTolerance,
            DefaultDoubleTolerance,
            DefaultDoubleTolerance,
            because,
            becauseArgs);
    }
    
    public static AndConstraint<GenericCollectionAssertions<double>> BeApproximately(
        this GenericCollectionAssertions<double> parent,
        IEnumerable<double> expectation,
        double relativeToleranceForNonZeros,
        double? minimumToleranceForNonZeros = null,
        double? toleranceForZeros = null,
        [StringSyntax("CompositeFormat")] string because = "",
        params object[] becauseArgs)
    {
        return parent.BeEquivalentTo(
            expectation, 
            options => options.WithSmartDoubleTolerance(relativeToleranceForNonZeros, minimumToleranceForNonZeros, toleranceForZeros), 
            because, 
            becauseArgs);
    }
}