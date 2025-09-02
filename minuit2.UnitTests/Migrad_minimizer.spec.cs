using FluentAssertions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

[TestFixture]
public class The_migrad_minimizer() : Any_minimizer(Migrad)
{
    private static readonly IMinimizer Migrad = Minimizer.Migrad;

    private static IEnumerable<CubicPolynomial.LeastSquaresBuilder> CostFunctionWithVaryingErrorDefinitionTestCases()
    {
        yield return CubicPolynomial.LeastSquaresCost;
        yield return CubicPolynomial.LeastSquaresCost.WithMissingYErrors();
        yield return CubicPolynomial.LeastSquaresCost.WithGradient();
        yield return CubicPolynomial.LeastSquaresCost.WithGradient().WithMissingYErrors();
    }
    
    [TestCaseSource(nameof(CostFunctionWithVaryingErrorDefinitionTestCases))]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_the_same_cost_value(
        CubicPolynomial.LeastSquaresBuilder costBuilder)
    {
        var referenceCost = costBuilder.WithErrorDefinition(1).Build();
        var cost = costBuilder.WithAnyErrorDefinitionBetween(2, 5).Build();
        
        var referenceResult = Migrad.Minimize(referenceCost, CubicPolynomial.ParameterConfigurations.Defaults);
        var result = Migrad.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);
        
        result.CostValue.Should().BeApproximately(referenceResult.CostValue, 1E-10);
    }
    
    [TestCaseSource(nameof(CostFunctionWithVaryingErrorDefinitionTestCases))]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_parameter_covariances_that_directly_scale_with_the_error_definition(
        CubicPolynomial.LeastSquaresBuilder costBuilder)
    {
        var referenceCost = costBuilder.WithErrorDefinition(1).Build();
        var cost = costBuilder.WithAnyErrorDefinitionBetween(2, 5).Build();
        
        var referenceResult = Migrad.Minimize(referenceCost, CubicPolynomial.ParameterConfigurations.Defaults);
        var result = Migrad.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);
        
        result.ParameterCovarianceMatrix.Should().BeEquivalentTo(referenceResult.ParameterCovarianceMatrix.MultipliedBy(cost.ErrorDefinition), 
            options => options.WithRelativeDoubleTolerance(0.001));
    }
}