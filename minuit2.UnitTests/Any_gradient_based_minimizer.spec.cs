using AwesomeAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.MinimizationProblems;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public abstract class Any_gradient_based_minimizer(IMinimizer minimizer) : Any_minimizer(minimizer)
{
    private readonly IMinimizer _minimizer = minimizer;
    private readonly ConfigurableLeastSquaresProblem _defaultProblem = new CubicPolynomialLeastSquaresProblem();
    
    [Test]
    public void when_asked_to_minimize_a_cost_function_with_an_analytical_gradient_that_returns_the_wrong_size_throws_an_exception(
        [Values(1, 3)] int flawedGradientSize)
    {
        var cost = new ModelEvaluatingCostFunction(1, ["offset", "slope"], (x, p) => p[0] + p[1] * x,
            modelGradient: (_, _) => Enumerable.Repeat(1.0, flawedGradientSize).ToArray());
        var parameterConfigurations = new[] { Variable("offset", 1), Variable("slope", 1) };

        Action action = () => _minimizer.Minimize(cost, parameterConfigurations);

        action.Should().Throw<InvalidCostFunctionException>().WithMessage("*gradient*");
    }
    
    [Test]
    public void when_asked_to_minimize_a_cost_function_with_an_analytical_hessian_of_wrong_size_throws_an_exception(
        [Values(3, 5)] int flawedHessianSize)
    {
        var cost = new ModelEvaluatingCostFunction(1, ["offset", "slope"], (x, p) => p[0] + p[1] * x,
            modelHessian: (_, _) => Enumerable.Repeat(1.0, flawedHessianSize).ToArray());
        var parameterConfigurations = new[] { Variable("offset", 1), Variable("slope", 1) };

        Action action = () => _minimizer.Minimize(cost, parameterConfigurations);

        action.Should().Throw<InvalidCostFunctionException>().WithMessage("*Hessian*");
    }

    [Test]
    public void when_asked_to_minimize_a_cost_function_with_an_analytical_hessian_diagonal_of_wrong_size_throws_an_exception(
        [Values(1, 3)] int flawedHessianDiagonalSize)
    {
        var cost = new ModelEvaluatingCostFunction(1, ["offset", "slope"], (x, p) => p[0] + p[1] * x,
            modelHessianDiagonal: (_, _) => Enumerable.Repeat(1.0, flawedHessianDiagonalSize).ToArray());
        var parameterConfigurations = new[] { Variable("offset", 1), Variable("slope", 1) };

        Action action = () => _minimizer.Minimize(cost, parameterConfigurations);

        action.Should().Throw<InvalidCostFunctionException>().WithMessage("*Hessian diagonal*");
    }
    
    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    [Description("This test should apply to all minimizers. It was put here to exclude the Simplex minimizer that " +
                 "occasionally reports non-convergence — even at the true minimum — due to its unreliable convergence " +
                 "criteria (see the Minuit docs: '...it would not even know if it did converge').")]
    public void when_minimizing_a_well_posed_problem_converges_to_a_valid_cost_function_minimum(
        IConfiguredProblem problem,
        Strategy strategy)
    { 
        var result = _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeTrue();
            x.ExitCondition.Should().Be(MinimizationExitCondition.Converged);
            x.CostValue.Should().BeLessThan(problem.InitialCostValue());
        });
    }
    
    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    public void when_minimizing_a_well_posed_problem_yields_parameter_values_that_agree_with_the_optimum_values_within_3_sigma_tolerance(
        IConfiguredProblem problem,
        Strategy strategy)
    { 
        var result = _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);

        result.ParameterValues.Select((value, index) => (value, index)).Should().AllSatisfy(p =>
        {
            var optimumValue = problem.OptimumParameterValues.ElementAt(p.index);
            var tolerance = 3 * Math.Sqrt(result.ParameterCovarianceMatrix?[p.index, p.index] ?? double.NaN);
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
            .NotBeNull().And
            .BeApproximately(referenceResult.ParameterCovarianceMatrix.MultipliedBy(cost.ErrorDefinition));
    }

    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_gradient_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls()
    {
        var cost = _defaultProblem.Cost.WithGradient().Build();
        var referenceCost = _defaultProblem.Cost.Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        var referenceResult = _minimizer.Minimize(referenceCost, parameterConfigurations);

        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithRelativeDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
    }
    
    [TestCase(double.NaN)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    public void when_minimizing_a_cost_function_with_an_analytical_gradient_that_returns_non_finite_values_during_the_process_yields_an_invalid_result_with_non_finite_gradient_exit_condition_and_undefined_covariances(
        double nonFiniteValue)
    {
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var cost = _defaultProblem.Cost.WithGradient().Build().WithGradientOverride(_ =>
            Enumerable.Repeat(1.0, parameterConfigurations.Length - 1).Concat([nonFiniteValue]).ToArray());
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteGradient);
            x.ParameterCovarianceMatrix.Should().BeNull();
        });
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_gradient_that_throws_an_exception_during_the_process_forwards_that_exception()
    {
        var cost = _defaultProblem.Cost.WithGradient().Build().WithGradientOverride(_ => throw new TestException());
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        
        Action action = () => _minimizer.Minimize(cost, parameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls()
    {
        var cost = _defaultProblem.Cost.WithHessian().Build();
        var referenceCost = _defaultProblem.Cost.WithGradient().Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        var referenceResult = _minimizer.Minimize(referenceCost, parameterConfigurations);
        
        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithRelativeDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
    }
    
    [TestCase(double.NaN)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_that_returns_non_finite_values_during_the_process_yields_an_invalid_result_with_non_finite_hessian_exit_condition_and_undefined_covariances(
        double nonFiniteValue)
    {
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var cost = _defaultProblem.Cost.WithHessian().Build().WithHessianOverride(_ => 
            Enumerable.Repeat(nonFiniteValue, parameterConfigurations.Length * parameterConfigurations.Length).ToArray());
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteHessian);
            x.ParameterCovarianceMatrix.Should().BeNull();
        });
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_that_throws_an_exception_during_the_process_forwards_that_exception()
    {
        var cost = _defaultProblem.Cost.WithHessian().Build().WithHessianOverride(_ => throw new TestException());
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        
        Action action = () => _minimizer.Minimize(cost, parameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
    
    [Test, 
     Description("This test ensures the Hessian (diagonal) is regularized during minimizer seeding to prevent the " +
                 "minimizer from initially stepping away from the minimum (and eventually failing).")]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_that_is_not_positive_definite_for_the_initial_parameter_values_yields_a_result_matching_the_result_obtained_for_numerical_approximation()
    {
        // For the initial parameter values [2, 1, 0], the Hessian is not positive definite. Consequently, the initial
        // Newton step points in the wrong direction — away from the local minimum. To prevent this, the initial
        // Hessian (or its diagonal approximation) must be regularized to ensure positive definiteness during minimizer
        // seeding. Without this safeguard, the minimizer will fail in this case (cf. https://github.com/root-project/root/issues/20665). 
        var problem = new ExponentialDecayLeastSquaresProblem();
        var parameterConfigurations = problem.ParameterConfigurations
            .WithParameter(1).WithLimits(0, null)
            .Build();
        
        var cost = problem.Cost.WithHessian().Build();
        var referenceCost = problem.Cost.Build();
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        var referenceResult = _minimizer.Minimize(referenceCost, parameterConfigurations);

        result.Should().MatchExcludingFunctionCalls(referenceResult, options => options.WithRelativeDoubleTolerance(0.001));
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

        sumResult.ParameterCovarianceMatrix.Should()
            .NotBeNull().And
            .BeApproximately(componentResult.ParameterCovarianceMatrix);
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
            .NotBeNull().And
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
            .NotBeNull().And
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
    
    [Test]
    public void when_minimizing_a_cost_function_sum_where_only_some_components_have_an_analytical_hessian_yields_the_same_result_as_if_none_had_an_analytical_gradient(
        [Values] Strategy strategy)
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        // second component shares offset parameter with the first component
        var component2 = problem.Cost.WithParameterSuffixes("2", [1, 2]).Build();
        var cost = CostFunction.Sum(problem.Cost.WithHessian().Build(), component2);
        var referenceCost = CostFunction.Sum(problem.Cost.Build(), component2);
        
        var parameterConfigurations = problem.ParameterConfigurations.Build().Concat(
            problem.ParameterConfigurations.WithSuffix("2").Build()).ToArray();
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        
        var result = _minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);
        var referenceResult = _minimizer.Minimize(referenceCost, parameterConfigurations, minimizerConfiguration);
        
        result.Should().Match(referenceResult);
    }

    [Test]
    public void when_minimizing_a_cost_function_sum_where_all_components_have_analytical_hessians_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls(
        [Values(1, 2)] double errorDefinitionOfComponent1,
        [Values] Strategy strategy)
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        // second component shares offset parameter with the first component
        var cost = CostFunction.Sum(
            problem.Cost.WithErrorDefinition(errorDefinitionOfComponent1).WithHessian().Build(), 
            problem.Cost.WithParameterSuffixes("2", [1, 2]).WithHessian().Build());
        var referenceCost = CostFunction.Sum(
            problem.Cost.WithErrorDefinition(errorDefinitionOfComponent1).Build(), 
            problem.Cost.WithParameterSuffixes("2", [1, 2]).Build());
        
        var parameterConfigurations = problem.ParameterConfigurations.Build().Concat(
            problem.ParameterConfigurations.WithSuffix("2").Build()).ToArray();
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        
        var result = _minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);
        var referenceResult = _minimizer.Minimize(referenceCost, parameterConfigurations, minimizerConfiguration);
        
        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithRelativeDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
    }
}