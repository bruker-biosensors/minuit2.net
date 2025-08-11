using FluentAssertions;
using minuit2.net;
using minuit2.net.costFunctions;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public class A_migrad_minimizer
{
    [Test]
    public void when_called_with_parameter_configurations_that_mismatch_the_cost_function_parameters_throws_an_exception()
    {
        ParameterConfiguration[] userParameters = [Variable("a", 0), Variable("b", 0)];
        var costParameters = new[] { "a", "b", "c" };
        var cost = CostFunction.LeastSquares([0, 1, 2], [0, 1, 2], costParameters, (_, _) => 0);

        Action action = () => _ = MigradMinimizer.Minimize(cost, userParameters);
        action.Should().Throw<ArgumentException>();
    }
}