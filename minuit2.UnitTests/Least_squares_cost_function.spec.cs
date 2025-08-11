using FluentAssertions;
using minuit2.net;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

public class A_least_squares_cost_function
{
    private static int AnyCount(int min = 10, int max = 100) => Any.Integer().Between(min, max);
    private static List<double> AnyValues(int count) => Enumerable.Range(0, count).Select(_ => (double)Any.Double()).ToList();
    
    [Test]
    public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
        [Values(-1, 1)] int countBiasDirection)
    {
        var xCount = AnyCount(10, 50);
        var yCount = xCount + countBiasDirection * AnyCount(1, 10);
        
        var construction = void () => _ = new LeastSquaresWithUniformYError(AnyValues(xCount), AnyValues(yCount), 1, [], (_, _) => 0);
        construction.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void when_constructed_with_a_collection_of_y_errors_mismatching_the_number_of_y_values_throws_an_exception(
        [Values(-1, 1)] int countBiasDirection)
    {
        var valueCount = AnyCount(10, 50);
        var errorCount = valueCount + countBiasDirection * AnyCount(1, 10);
        
        var construction = void () => _ = new LeastSquares(AnyValues(valueCount), AnyValues(valueCount), AnyValues(errorCount), [], (_, _) => 0);
        construction.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void with_a_uniform_y_error_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
    {
        var constantModelLevel = Any.Double();
        var valueCount = AnyCount();
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        var yError = Any.Double();
        var cost = new LeastSquaresWithUniformYError(xValues, yValues, yError, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
            .Select(y => (y - constantModelLevel) / yError)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
    
    [Test]
    public void with_individual_y_errors_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
    {
        var constantModelLevel = Any.Double();
        var valueCount = AnyCount();
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        var yErrors = AnyValues(valueCount);
        var cost = new LeastSquares(xValues, yValues, yErrors, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
                .Zip(yErrors, (y, yError) => (y - constantModelLevel) / yError)
                .Select(r => r * r)
                .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }

    [Test]
    public void without_y_errors_when_asked_for_its_cost_value_returns_the_sum_of_squared_unweighted_residuals()
    {
        var constantModelLevel = Any.Double();
        var valueCount = AnyCount();
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        var cost = new LeastSquaresWithUniformYError(xValues, yValues, 1, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
            .Select(y => y - constantModelLevel)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
}