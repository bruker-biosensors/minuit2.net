using FluentAssertions;
using minuit2.net;

namespace minuit2.UnitTests;

public class LeastSquaresTests
{
    private static List<double> SomeValues(int count) => Enumerable.Range(0, count).Select(i => (double)i).ToList();
    
    [TestCase(0,1)]
    [TestCase(1,10)]
    [TestCase(10,9)]
    public void A_least_squares_cost_function_when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
        int xValueCount, int yValueCount)
    {
        var xValues = SomeValues(xValueCount);
        var yValues = SomeValues(yValueCount);
        
        var construction = void () => _ = new LeastSquares(xValues, yValues, 0, (_, _) => 0, []);
        construction.Should().Throw<ArgumentException>();
    }
}