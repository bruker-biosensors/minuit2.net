using AwesomeAssertions;
using AwesomeAssertions.Execution;
using minuit2.net;
using minuit2.net.CostFunctions;
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
    
    [Test]
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_parameter_covariances_equal_to_those_for_the_isolated_component(
        [Values] bool hasGradient, 
        [Values] Strategy strategy)
    {
        var component = CubicPolynomial.LeastSquaresCost.WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component);
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var componentResult = _minimizer.Minimize(component, parameterConfigurations, minimizerConfiguration);
        var sumResult = _minimizer.Minimize(sum, parameterConfigurations, minimizerConfiguration);

        sumResult.ParameterCovarianceMatrix.Should().BeEquivalentTo(componentResult.ParameterCovarianceMatrix,
            options => options.WithRelativeDoubleTolerance(0.001));
    }

    [Test]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_yields_parameter_covariances_equivalent_to_those_for_the_isolated_components(
        [Values] bool hasGradient, 
        [Values] Strategy strategy)
    { 
        if (strategy == Strategy.Fast) 
            Assert.Ignore("Although the fast minimization strategy yields covariances that roughly agree (within ~10%), " +
                          "they are not strictly equivalent — even when using a minimal convergence tolerance to " +
                          "prevent early termination. Users should be aware of this limitation and are advised to " +
                          "apply error refinement after minimizing cost function sums with the fast strategy when " +
                          "precise parameter uncertainties are required (see test below).");
        
        var component1 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(1).WithGradient(hasGradient).Build();
        var component2 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(2).WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(1);
        var parameterConfigurations2 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(2);
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var component1Result = _minimizer.Minimize(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = _minimizer.Minimize(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = _minimizer.Minimize(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(),
            minimizerConfiguration);

        AssertCovariancesAreEquivalentBetween(sumResult, component1Result, component2Result);
    }


    [Test]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_using_the_fast_strategy_and_subsequently_applying_error_refinement_yields_parameter_covariances_equivalent_to_those_for_the_isolated_components(
        [Values] bool hasGradient)
    { 
        var component1 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(1).WithGradient(hasGradient).Build();
        var component2 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(2).WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(1);
        var parameterConfigurations2 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(2);
        var minimizerConfiguration = new MinimizerConfiguration(Strategy.Fast);

        var component1Result = MinimizeAndRefineErrors(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = MinimizeAndRefineErrors(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = MinimizeAndRefineErrors(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        AssertCovariancesAreEquivalentBetween(sumResult, component1Result, component2Result);
    }
    
    private IMinimizationResult MinimizeAndRefineErrors(
        ICostFunction cost,
        ParameterConfiguration[] parameterConfigurations, 
        MinimizerConfiguration? minimizerConfiguration)
    {
        var result = _minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);
        return HesseErrorCalculator.Refine(result, cost);
    }

    private static void AssertCovariancesAreEquivalentBetween(
        IMinimizationResult sum, 
        IMinimizationResult component1, 
        IMinimizationResult component2)
    {
        // Assumes components are completely independent (no shared parameters)
        
        const double relativeToleranceForNonZeros = 0.001;
        const double absoluteToleranceForZeros = 1e-8;

        var dim1 = component1.Parameters.Count;
        var dim2 = component2.Parameters.Count;
        var component1Section = sum.ParameterCovarianceMatrix.SubMatrix(0, dim1 - 1, 0, dim1 - 1);
        var component2Section = sum.ParameterCovarianceMatrix.SubMatrix(dim1, dim1 + dim2 - 1, dim1, dim1 + dim2 - 1);
        var offSection1 = sum.ParameterCovarianceMatrix.SubMatrix(0, dim1 - 1, dim1, dim1 + dim2 - 1);
        var offSection2 = sum.ParameterCovarianceMatrix.SubMatrix(dim1, dim1 + dim2 - 1, 0, dim1 - 1);
        
        using (new AssertionScope())
        {
            component1Section.Should().BeEquivalentTo(component1.ParameterCovarianceMatrix, options => options.WithRelativeDoubleTolerance(relativeToleranceForNonZeros));
            component2Section.Should().BeEquivalentTo(component2.ParameterCovarianceMatrix, options => options.WithRelativeDoubleTolerance(relativeToleranceForNonZeros));
            offSection1.Should().BeEquivalentTo(AllZeroMatrix(4,4), options => options.WithDoubleTolerance(absoluteToleranceForZeros));
            offSection2.Should().BeEquivalentTo(AllZeroMatrix(4,4), options => options.WithDoubleTolerance(absoluteToleranceForZeros));
        }
    }
    
    private static double[,] AllZeroMatrix(int rows, int columns) => new double[rows, columns];
}
