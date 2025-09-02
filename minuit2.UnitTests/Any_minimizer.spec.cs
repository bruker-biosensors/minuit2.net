using FluentAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public abstract class Any_minimizer(IMinimizer minimizer)
{
    private static IEnumerable<TestCaseData> MismatchingParameterConfigurationTestCases()
    {
        var cost = Any.InstanceOf<ICostFunction>();

        var mismatchingNames = cost.Parameters.Select(name => ParameterConfiguration(name + Any.String()));
        yield return new TestCaseData(cost, mismatchingNames).SetName("Mismatching parameter names");

        var tooFew = cost.Parameters.Skip(1).Select(ParameterConfiguration);
        yield return new TestCaseData(cost, tooFew).SetName("Too few parameter configurations");

        var tooMany = cost.Parameters.Select(ParameterConfiguration).Concat([ParameterConfiguration(Any.String())]);
        yield return new TestCaseData(cost, tooMany).SetName("Too many parameter configurations");
    }
    
    private static ParameterConfiguration ParameterConfiguration(string name) => Variable(name, Any.Double());
    
    [TestCaseSource(nameof(MismatchingParameterConfigurationTestCases))]
    public void when_asked_to_minimize_a_cost_function_with_mismatching_parameter_configurations_throws_an_exception(
        ICostFunction cost, IEnumerable<ParameterConfiguration> mismatchingParameterConfigurations)
    {
        Action action = () => _ = minimizer.Minimize(cost, mismatchingParameterConfigurations.ToList());
        action.Should().Throw<ArgumentException>();
    }
}