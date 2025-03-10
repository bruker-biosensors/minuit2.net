using FluentAssertions;
using minuit2.net;

namespace minuit2.UnitTests;

public class LeastSquaresTests
{
    private static List<double> SomeValues(int count) => Enumerable.Range(0, count).Select(i => (double)i).ToList();
    private static List<string> SomeParameters(int count) => Enumerable.Range(65, count).Select(i => ((char)i).ToString()).ToList();
    
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


    [TestCase(0)]
    [TestCase(3)]
    public void A_least_squares_cost_function_when_constructed_with_fewer_parameter_names_than_the_number_of_model_parameters_throws_an_exception(
        int parameterCount)
    {
        Func<double ,IList<double>, double> cubicPoly = (x, c) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;
        var tooFewParameters = SomeParameters(parameterCount);
        
        var construction = void () => _ = new LeastSquares(SomeValues(3), SomeValues(3), 0, cubicPoly, tooFewParameters);
        construction.Should().Throw<ArgumentException>();
    }
}