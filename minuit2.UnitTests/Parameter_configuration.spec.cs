using FluentAssertions;
using minuit2.net;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public class A_parameter_configuration
{
    private static ParameterConfiguration TestVariable(double value, double? lowerLimit, double? upperLimit) =>
        Variable("variable", value, lowerLimit, upperLimit);
    
    [Test]
    public void when_constructed_as_a_variable_with_a_lower_limit_larger_or_equal_to_the_value_throws_an_exception(
        [Values(1, 2)] double lowerLimit, 
        [Values(null, double.PositiveInfinity, 100)] double? upperLimit)
    {
        const double value = 1;
        Action action = () => _ = TestVariable(value, lowerLimit, upperLimit);
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
    
    [Test]
    public void when_constructed_as_a_variable_with_an_upper_limit_smaller_or_equal_to_the_value_throws_an_exception(
        [Values(null, double.NegativeInfinity, -100)] double? lowerLimit, 
        [Values(0, 1)] double? upperLimit)
    {
        const double value = 1;
        Action action = () => _ = TestVariable(value, lowerLimit, upperLimit);
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
}
