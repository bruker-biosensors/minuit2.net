using FluentAssertions;
using minuit2.net;

namespace minuit2.UnitTests;

public class A_least_squares_cost_function
{
    private static List<double> SomeValues(int count) => Enumerable.Range(0, count).Select(i => (double)i).ToList();
    
    private static List<double> SequenceMatching(IList<double> values) =>
        Enumerable.Range(0, values.Count).Select(i => (double)i).ToList();
    
    [TestCase(0,1)]
    [TestCase(1,10)]
    [TestCase(10,9)]
    public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
        int xValueCount, int yValueCount)
    {
        var xValues = SomeValues(xValueCount);
        var yValues = SomeValues(yValueCount);
        
        var construction = void () => _ = new LeastSquares(xValues, yValues, [], (_, _) => 0);
        construction.Should().Throw<ArgumentException>();
    }
    
    [TestCase(0,1)]
    [TestCase(1,10)]
    [TestCase(10,9)]
    public void when_constructed_with_a_collection_of_y_errors_mismatching_the_number_of_y_values_throws_an_exception(
        int yValueCount, int yErrorCount)
    {
        var yValues = SomeValues(yValueCount);
        var yErrors = SomeValues(yErrorCount);

        var construction = void () => _ = new LeastSquares(SequenceMatching(yValues), yValues, yErrors, [], (_, _) => 0);
        construction.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void with_a_uniform_y_error_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals(
        [Values(-2, 0, 1)] double constantModelLevel)
    {
        IList<double> yValues = [1, -2, 3, -4, 5];
        const double yError = 0.1;
        var cost = new LeastSquares(SequenceMatching(yValues), yValues, yError, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
            .Select(y => (y - constantModelLevel) / yError)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
    
    [Test]
    public void with_individual_y_errors_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals(
        [Values(-2, 0, 1)] double constantModelLevel)
    {
        IList<double> yValues = [1, -2, 3, -4, 5];
        IList<double> yErrors = [0.1, 0.2, 0.3, 0.4, 0.5];
        var cost = new LeastSquares(SequenceMatching(yValues), yValues, yErrors, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
                .Zip(yErrors, (y, yError) => (y - constantModelLevel) / yError)
                .Select(r => r * r)
                .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }

    [Test]
    public void without_y_errors_when_asked_for_its_cost_value_returns_the_sum_of_squared_unweighted_residuals(
        [Values(-2, 0, 1)] double constantModelLevel)
    {
        IList<double> yValues = [1, -2, 3, -4, 5];
        var cost = new LeastSquares(SequenceMatching(yValues), yValues, ["level"], (_, p) => p[0]);
        
        var expectedValue = yValues
            .Select(y => y - constantModelLevel)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
}