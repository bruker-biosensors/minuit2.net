using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Numeric;

namespace minuit2.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class NumericAssertionExtensions
{
    public static DoubleAssertionConfigurator BeApproximately(
        this NumericAssertions<double> parent,
        double expectedValue,
        string because = "",
        params object[] becauseArgs)
    {
        return new DoubleAssertionConfigurator(parent, expectedValue, because, becauseArgs);
    }

    internal class DoubleAssertionConfigurator(
            NumericAssertions<double> parent,
            double expectedValue,
            string because,
            params object[] becauseArgs)
    {
        private AndConstraint<NumericAssertions<double>> WithTolerance(double tolerance) =>
            parent.BeApproximately(expectedValue, tolerance, because, becauseArgs);

        public AndConstraint<NumericAssertions<double>> WithRelativeTolerance(
            double relativeTolerance, 
            double minimumTolerance = 1E-8)
        {
            var tolerance = Math.Abs(expectedValue * relativeTolerance);
            return WithTolerance(Math.Max(tolerance, minimumTolerance));
        }
    }
}