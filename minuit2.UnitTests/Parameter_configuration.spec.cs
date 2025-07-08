using FluentAssertions;
using minuit2.net;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

[TestFixture]
public class A_parameter_configuration
{
    public class When_constructed_as_a_variable
    {
        private static ParameterConfiguration TestVariable(double value, double? lowerLimit, double? upperLimit) =>
            Variable("variable", value, lowerLimit, upperLimit);

        [Test]
        public void is_not_fixed()
        {
            var variable = TestVariable(1, null, null);
            variable.IsFixed.Should().BeFalse();
        }

        [Test]
        public void exposes_the_given_value()
        {
            const int value = 1;
            var variable = TestVariable(value, null, null);
            variable.Value.Should().Be(value);
        }
        
        [Test]
        public void exposes_the_given_lower_limit()
        {
            const int lowerLimit = 0;
            var variable = TestVariable(1, lowerLimit, null);
            variable.LowerLimit.Should().Be(lowerLimit);
        }
        
        [Test]
        public void exposes_the_given_upper_limit()
        {
            const int upperLimit = 2;
            var variable = TestVariable(1, null, upperLimit);
            variable.UpperLimit.Should().Be(upperLimit);
        }
        
        [Test]
        public void with_a_lower_limit_larger_or_equal_to_the_value_throws_an_exception(
            [Values(1, 2)] double lowerLimit, 
            [Values(null, double.PositiveInfinity, 100)] double? upperLimit)
        {
            const double value = 1;
            Action action = () => _ = TestVariable(value, lowerLimit, upperLimit);
            action.Should().ThrowExactly<InvalidParameterConfiguration>();
        }
    
        [Test]
        public void with_an_upper_limit_smaller_or_equal_to_the_value_throws_an_exception(
            [Values(null, double.NegativeInfinity, -100)] double? lowerLimit, 
            [Values(0, 1)] double? upperLimit)
        {
            const double value = 1;
            Action action = () => _ = TestVariable(value, lowerLimit, upperLimit);
            action.Should().ThrowExactly<InvalidParameterConfiguration>();
        }
    }

    public class When_constructed_as_a_constant
    {
        private static ParameterConfiguration TestConstant(double value) => Fixed("constant", value);
        
        [Test]
        public void is_fixed()
        {
            var variable = TestConstant(1);
            variable.IsFixed.Should().BeTrue();
        }

        [Test]
        public void exposes_the_given_value()
        {
            const int value = 1;
            var variable = TestConstant(value);
            variable.Value.Should().Be(value);
        }
        
        [Test]
        public void exposes_no_limits()
        {
            var variable = TestConstant(1);
            variable.LowerLimit.Should().BeNull();
            variable.UpperLimit.Should().BeNull();
        }
    }
}
