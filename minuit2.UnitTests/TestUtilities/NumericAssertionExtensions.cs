using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Numeric;

namespace minuit2.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class NumericAssertionExtensions
{
    private const double DefaultRelativeDoubleTolerance = 0.001;
    private const double DefaultMinimumDoubleTolerance = 1E-8;

    public static AndConstraint<NumericAssertions<double>> BeApproximately(
        this NumericAssertions<double> parent, 
        double expectedValue,
        [StringSyntax("CompositeFormat")] string because = "", 
        params object[] becauseArgs)
    {
        return parent.BeApproximately(
            expectedValue, 
            DefaultRelativeDoubleTolerance, 
            DefaultMinimumDoubleTolerance, 
            because, 
            becauseArgs);
    }
    
    
    public static AndConstraint<NumericAssertions<double>> BeApproximately(
        this NumericAssertions<double> parent, 
        double expectedValue, 
        double relativeTolerance,
        double minimumTolerance,
        [StringSyntax("CompositeFormat")] string because = "", 
        params object[] becauseArgs)
    {
        var tolerance = Math.Abs(expectedValue * relativeTolerance);
        return parent.BeApproximately(expectedValue, Math.Max(tolerance, minimumTolerance), because, becauseArgs);
    }
}