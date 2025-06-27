using FluentAssertions;
using minuit2.net;

namespace minuit2.UnitTests;

public class A_parameter_configuration
{
    private static ParameterConfiguration TestParameterConfiguration(double value, double? lowerLimit, double? upperLimit) =>
        new("p_test", value, false, lowerLimit, upperLimit);
    
    [Test]
    public void when_constructed_with_a_lower_limit_larger_or_equal_to_the_value_throws_an_exception(
        [Values(1, 2)] double lowerLimit, 
        [Values(null, double.PositiveInfinity, 100)] double? upperLimit)
    {
        const double value = 1;
        Action action = () => _ = TestParameterConfiguration(value, lowerLimit, upperLimit);
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
    
    [Test]
    public void when_constructed_with_an_upper_limit_smaller_or_equal_to_the_value_throws_an_exception(
        [Values(null, double.NegativeInfinity, -100)] double? lowerLimit, 
        [Values(0, 1)] double? upperLimit)
    {
        const double value = 1;
        Action action = () => _ = TestParameterConfiguration(value, lowerLimit, upperLimit);
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
    
    [Test]
    public void when_updated_such_that_the_new_value_exceeds_the_limits_throws_an_exception(
        [Values(-2, -1, 1, 2)] double newValue)
    {
        var validConfig = TestParameterConfiguration(0, -1, 1);
        Action action = () => _ = validConfig with { Value = newValue };
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
    
    [Test]
    public void when_updated_such_that_the_new_lower_limit_becomes_larger_or_equal_to_the_value_throws_an_exception(
        [Values(0, 1, 2)] double newLowerLimit)
    {
        var validConfig = TestParameterConfiguration(0, -1, 1);
        Action action = () => _ = validConfig with { LowerLimit = newLowerLimit };
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
    
    [Test]
    public void when_updated_such_that_the_new_upper_limit_becomes_smaller_or_equal_to_the_value_throws_an_exception(
        [Values(-2, -1, 0)] double newUpperLimit)
    {
        var validConfig = TestParameterConfiguration(0, -1, 1);
        Action action = () => _ = validConfig with { UpperLimit = newUpperLimit };
        action.Should().ThrowExactly<InvalidParameterConfiguration>();
    }
}
