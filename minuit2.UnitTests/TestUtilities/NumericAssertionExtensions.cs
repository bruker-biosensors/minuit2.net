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
        bool considerNaNsEqual = true,
        string because = "",
        params object[] becauseArgs)
    {
        return new DoubleAssertionConfigurator(parent, expectedValue, considerNaNsEqual, because, becauseArgs);
    }

    internal class DoubleAssertionConfigurator(
            NumericAssertions<double> parent,
            double expectedValue,
            bool considerNaNsEqual,
            string because,
            params object[] becauseArgs)
    {
        public AndConstraint<NumericAssertions<double>> WithTolerance(double tolerance)
        {
            if (considerNaNsEqual && double.IsNaN(parent.Subject) && double.IsNaN(expectedValue))
                return new AndConstraint<NumericAssertions<double>>(parent);
            
            return parent.BeApproximately(expectedValue, tolerance, because, becauseArgs);
        }

        public AndConstraint<NumericAssertions<double>> WithRelativeTolerance(double relativeTolerance) =>
            WithTolerance(Math.Abs(expectedValue * relativeTolerance));
    }
}