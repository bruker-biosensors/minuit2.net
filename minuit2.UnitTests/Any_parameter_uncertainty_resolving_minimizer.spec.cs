using AwesomeAssertions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

public abstract class Any_parameter_uncertainty_resolving_minimizer(IMinimizer minimizer) : Any_minimizer(minimizer)
{
    private readonly IMinimizer _minimizer = minimizer;

    [Test]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_parameter_covariances_that_directly_scale_with_the_error_definition()
    {
        var cost = CubicPolynomial.LeastSquaresCost.WithAnyErrorDefinitionBetween(2, 5).Build();
        var referenceCost = CubicPolynomial.LeastSquaresCost.WithErrorDefinition(1).Build();
        
        var result = _minimizer.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);
        var referenceResult = _minimizer.Minimize(referenceCost, CubicPolynomial.ParameterConfigurations.Defaults);
        
        result.ParameterCovarianceMatrix.Should().BeEquivalentTo(referenceResult.ParameterCovarianceMatrix.MultipliedBy(cost.ErrorDefinition), 
            options => options.WithRelativeDoubleTolerance(0.001));
    }
}