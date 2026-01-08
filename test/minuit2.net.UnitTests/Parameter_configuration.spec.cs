using AwesomeAssertions;
using ConstrainedNonDeterminism;

namespace minuit2.net.UnitTests;

[TestFixture]
public class A_parameter_configuration
{
    public class When_constructed_as_a_variable
    {
        private static ParameterConfiguration TestVariable(double value, double? lowerLimit, double? upperLimit) =>
            ParameterConfiguration.Variable(Any.String(), value, lowerLimit, upperLimit);
        
        [Test]
        public void is_not_fixed()
        {
            var value = Any.Double();
            var lowerLimit = Any.Double().SmallerThan(value).OrNull();
            var upperLimit = Any.Double().GreaterThan(value).OrNull();
            
            var variable = TestVariable(value, lowerLimit, upperLimit);
            
            variable.IsFixed.Should().BeFalse();
        }

        [Test]
        public void exposes_the_given_value()
        {
            var value = Any.Double();
            var lowerLimit = Any.Double().SmallerThan(value).OrNull();
            var upperLimit = Any.Double().GreaterThan(value).OrNull();
            
            var variable = TestVariable(value, lowerLimit, upperLimit);
            
            variable.Value.Should().Be(value);
        }
        
        [Test]
        public void exposes_the_given_lower_limit()
        {
            var lowerLimit = Any.Double();
            var value = Any.Double().GreaterThan(lowerLimit);
            var upperLimit = Any.Double().GreaterThan(value).OrNull();
            
            var variable = TestVariable(value, lowerLimit, upperLimit);
            
            variable.LowerLimit.Should().Be(lowerLimit);
        }
        
        [Test]
        public void exposes_the_given_upper_limit()
        {
            var upperLimit = Any.Double();
            var value = Any.Double().SmallerThan(upperLimit);
            var lowerLimit = Any.Double().SmallerThan(value).OrNull();
            
            var variable = TestVariable(value, lowerLimit, upperLimit);
            
            variable.UpperLimit.Should().Be(upperLimit);
        }

        private static IEnumerable<TestCaseData> InvalidLowerLimitTestCases()
        {
            double value = Any.Double();
            var upperLimit = Any.Double().GreaterThan(value).OrNull();
            yield return new TestCaseData(value, value, upperLimit);
            yield return new TestCaseData(value, Any.Double().GreaterThan(value), upperLimit);
        }
        
        [TestCaseSource(nameof(InvalidLowerLimitTestCases))]
        public void with_a_lower_limit_larger_or_equal_to_the_value_throws_an_exception(
            double value, double lowerLimit, double? upperLimit)
        {
            Action action = () => _ = TestVariable(value, lowerLimit, upperLimit);
            action.Should().Throw<ArgumentException>();
        }

        private static IEnumerable<TestCaseData> InvalidUpperLimitTestCases()
        {
            double value = Any.Double();
            var lowerLimit = Any.Double().SmallerThan(value).OrNull();
            yield return new TestCaseData(value, lowerLimit, value);
            yield return new TestCaseData(value, lowerLimit, Any.Double().SmallerThan(value));
        }

        [TestCaseSource(nameof(InvalidUpperLimitTestCases))]
        public void with_an_upper_limit_smaller_or_equal_to_the_value_throws_an_exception(
            double value, double? lowerLimit, double upperLimit)
        {
            Action action = () => _ = TestVariable(value, lowerLimit, upperLimit);
            action.Should().Throw<ArgumentException>();
        }
    }

    public class When_constructed_as_a_constant
    {
        private static ParameterConfiguration TestConstant(double value) => 
            ParameterConfiguration.Fixed(Any.String(), value);
        
        [Test]
        public void is_fixed()
        {
            var variable = TestConstant(Any.Double());
            variable.IsFixed.Should().BeTrue();
        }

        [Test]
        public void exposes_the_given_value()
        {
            var value = Any.Double();
            var variable = TestConstant(value);
            variable.Value.Should().Be(value);
        }
        
        [Test]
        public void exposes_no_limits()
        {
            var variable = TestConstant(Any.Double());
            variable.LowerLimit.Should().BeNull();
            variable.UpperLimit.Should().BeNull();
        }
    }
}
