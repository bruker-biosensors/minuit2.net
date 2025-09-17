using AwesomeAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.MinimizationProblems;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

public abstract class Any_parameter_uncertainty_resolving_minimizer(IMinimizer minimizer) : Any_minimizer(minimizer)
{
    private readonly IMinimizer _minimizer = minimizer;
    private readonly ConfigurableLeastSquaresProblem _defaultProblem = new CubicPolynomialLeastSquaresProblem();

    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    public void when_minimizing_a_well_posed_problem_yields_parameter_values_that_agree_with_the_optimum_values_within_3_sigma_tolerance(
        ConfiguredProblem problem,
        Strategy strategy)
    { 
        var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);
        
        var result = _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
        
        result.ParameterValues.Select((value, index) => (value, index)).Should().AllSatisfy(p =>
        {
            var optimumValue = problem.OptimumParameterValues.ElementAt(p.index);
            var tolerance = 3 * Math.Sqrt(result.ParameterCovarianceMatrix[p.index, p.index]);
            p.value.Should().BeApproximately(optimumValue, tolerance);
        });
    }
    
    [Test]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_parameter_covariances_that_directly_scale_with_the_error_definition()
    {
        var cost = _defaultProblem.Cost.WithErrorDefinition(Any.Double().Between(2, 5)).Build();
        var referenceCost = _defaultProblem.Cost.WithErrorDefinition(1).Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        var referenceResult = _minimizer.Minimize(referenceCost, parameterConfigurations);
        
        result.ParameterCovarianceMatrix.Should()
            .BeApproximately(referenceResult.ParameterCovarianceMatrix.MultipliedBy(cost.ErrorDefinition));
    }
    
    [Test]
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_parameter_covariances_equal_to_those_for_the_isolated_component(
        [Values] bool hasGradient, 
        [Values] Strategy strategy)
    {
        var component = _defaultProblem.Cost.WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component);
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var componentResult = _minimizer.Minimize(component, parameterConfigurations, minimizerConfiguration);
        var sumResult = _minimizer.Minimize(sum, parameterConfigurations, minimizerConfiguration);

        sumResult.ParameterCovarianceMatrix.Should().BeApproximately(componentResult.ParameterCovarianceMatrix);
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
                          "precise parameter uncertainties are required (see complementary test).");
        
        var component1 = _defaultProblem.Cost.WithParameterSuffixes("1").WithGradient(hasGradient).Build();
        var component2 = _defaultProblem.Cost.WithParameterSuffixes("2").WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = _defaultProblem.ParameterConfigurations.WithSuffix("1").Build();
        var parameterConfigurations2 = _defaultProblem.ParameterConfigurations.WithSuffix("2").Build();
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var component1Result = _minimizer.Minimize(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = _minimizer.Minimize(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = _minimizer.Minimize(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        sumResult.ParameterCovarianceMatrix.Should()
            .BeApproximately(component1Result.ParameterCovarianceMatrix.BlockConcat(component2Result.ParameterCovarianceMatrix));
    }
    
    [Test]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_using_the_fast_strategy_and_subsequently_applying_error_refinement_yields_parameter_covariances_equivalent_to_those_for_the_isolated_components(
        [Values] bool hasGradient)
    { 
        var component1 = _defaultProblem.Cost.WithParameterSuffixes("1").WithGradient(hasGradient).Build();
        var component2 = _defaultProblem.Cost.WithParameterSuffixes("2").WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = _defaultProblem.ParameterConfigurations.WithSuffix("1").Build();
        var parameterConfigurations2 = _defaultProblem.ParameterConfigurations.WithSuffix("2").Build();
        var minimizerConfiguration = new MinimizerConfiguration(Strategy.Fast);

        var component1Result = MinimizeAndRefineErrors(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = MinimizeAndRefineErrors(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = MinimizeAndRefineErrors(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        sumResult.ParameterCovarianceMatrix.Should()
            .BeApproximately(component1Result.ParameterCovarianceMatrix.BlockConcat(component2Result.ParameterCovarianceMatrix));
    }
    
    private IMinimizationResult MinimizeAndRefineErrors(
        ICostFunction cost,
        ParameterConfiguration[] parameterConfigurations, 
        MinimizerConfiguration? minimizerConfiguration)
    {
        var result = _minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);
        return HesseErrorCalculator.Refine(result, cost);
    }
    
    [TestCase(double.NaN)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    public void when_the_cost_function_gradient_returns_non_finite_values_during_a_minimization_process_yields_an_invalid_result_with_non_finite_gradient_exit_condition(
        double nonFiniteValue)
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var cost = problem.Cost.WithGradient().Build().WithGradientOverride(_ => [nonFiniteValue, 1, 1]);
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteGradient);
            x.IssueParameterValues.Should().NotBeNull();
        });
    }
    
    [Test]
    public void when_the_cost_function_gradient_throws_an_exception_during_a_minimization_process_forwards_that_exception()
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var cost = problem.Cost.WithGradient().Build().WithGradientOverride(_ => throw new TestException());
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        Action action = () => _minimizer.Minimize(cost, parameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
}