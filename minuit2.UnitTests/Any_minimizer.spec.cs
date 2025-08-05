using FluentAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public class Any_minimizer
{
    private static IEnumerable<IMinimizer> MinimizerTestCases()
    {
        yield return Minimizer.Migrad();
        yield return Minimizer.Simplex();
        yield return Minimizer.Combined();
    }
    
    [TestCaseSource(nameof(MinimizerTestCases))]
    public void when_called_with_parameter_configurations_that_mismatch_the_cost_function_parameters_throws_an_exception(
        IMinimizer minimizer)
    {
        ParameterConfiguration[] userParameters = [Variable("a", 0), Variable("b", 0)];
        var costParameters = new[] { "a", "b", "c" };
        var cost = CostFunction.LeastSquares([0, 1, 2], [0, 1, 2], costParameters, (_, _) => 0);
        
        Action action = () => _ = minimizer.Minimize(cost, userParameters);
        action.Should().Throw<ArgumentException>();
    }
}