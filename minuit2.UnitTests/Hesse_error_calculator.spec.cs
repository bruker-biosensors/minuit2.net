using AwesomeAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

public class The_hesse_error_calculator
{
    [Test]
    public void when_asked_to_refine_a_minimization_result_for_an_unrelated_cost_function_throws_an_exception()
    {
        var minimizationResult = Any.InstanceOf<IMinimizationResult>();
        var unrelatedCostFunction = Any.InstanceOf<ICostFunction>();
        
        Action action = () => _ = HesseErrorCalculator.Refine(minimizationResult, unrelatedCostFunction);
        action.Should().Throw<ArgumentException>();
    }
}