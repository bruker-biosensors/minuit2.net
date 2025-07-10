using FluentAssertions;
using minuit2.net;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

public class A_least_squares_cost_function
{
    private static List<double> AnyValues(uint count) =>
        Enumerable.Range(0, (int)count).Select(_ => (double)Any.Double()).ToList();
    
    [Test]
    public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception()
    {
        var xCount = Any.UnsignedInteger();
        var yCount = Any.UnsignedInteger().OtherThan(xCount);
        
        var construction = void () => _ = new LeastSquares(AnyValues(xCount), AnyValues(yCount), [], (_, _) => 0);
        construction.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void when_constructed_with_a_collection_of_y_errors_mismatching_the_number_of_y_values_throws_an_exception()
    {
        var valueCount = Any.UnsignedInteger();
        var errorCount = Any.UnsignedInteger().OtherThan(valueCount);
        
        var construction = void () => _ = new LeastSquares(AnyValues(valueCount), AnyValues(valueCount), AnyValues(errorCount), [], (_, _) => 0);
        construction.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void with_a_uniform_y_error_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
    {
        var constantModelLevel = Any.Double();
        var valueCount = Any.UnsignedInteger().SmallerThan(100);
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        var yError = Any.Double();
        var cost = new LeastSquares(xValues, yValues, yError, ["level"], (_, p) => p[0]);
        
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
        var valueCount = Any.UnsignedInteger().SmallerThan(100);
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
        var valueCount = Any.UnsignedInteger().SmallerThan(100);
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        var cost = new LeastSquares(xValues, yValues, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
            .Select(y => y - constantModelLevel)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
}