using AwesomeAssertions;
using ExampleProblems;
using ExampleProblems.CustomProblems;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.net.UnitTests.TestUtilities;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.MinimizationExitCondition;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.net.UnitTests;

public abstract class Any_gradient_based_minimizer(IMinimizer minimizer) : Any_minimizer(minimizer)
{
    private readonly IMinimizer _minimizer = minimizer;

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
        IProblem problem,
        Strategy strategy)
    { 
        var result = _minimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeTrue();
            x.ExitCondition.Should().Be(Converged);
            x.CostValue.Should().BeLessThan(problem.InitialCostValue());
        });
    }
    
    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    public void when_minimizing_a_well_posed_problem_yields_parameter_values_that_agree_with_the_optimum_values_within_3_sigma_tolerance(
        IProblem problem,
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
        var problem = new CubicPolynomialProblem(errorDefinitionInSigma: Any.Double().Between(2, 5));
        var referenceProblem = new CubicPolynomialProblem(errorDefinitionInSigma: 1);
        
        var result = _minimizer.Minimize(problem);
        var referenceResult = _minimizer.Minimize(referenceProblem);
        
        result.ParameterCovarianceMatrix.Should()
            .NotBeNull().And
            .BeApproximately(referenceResult.ParameterCovarianceMatrix.MultipliedBy(problem.Cost.ErrorDefinition));
    }

    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_gradient_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls()
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: WithGradient);
        var referenceProblem = new CubicPolynomialProblem(derivativeConfiguration: WithoutDerivatives);

        var result = _minimizer.Minimize(problem);
        var referenceResult = _minimizer.Minimize(referenceProblem);

        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithSmartDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
    }
    
    [TestCase(double.NaN)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    public void when_minimizing_a_cost_function_with_an_analytical_gradient_that_returns_non_finite_values_during_the_process_yields_an_invalid_result_with_non_finite_gradient_exit_condition_and_undefined_covariances(
        double nonFiniteValue)
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: WithGradient);
        var parameterConfigurations = problem.ParameterConfigurations;
        var cost = problem.Cost.WithGradientOverride(_ =>
            Enumerable.Repeat(1.0, parameterConfigurations.Count - 1).Concat([nonFiniteValue]).ToArray());
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(NonFiniteGradient);
            x.ParameterCovarianceMatrix.Should().BeNull();
        });
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_gradient_that_throws_an_exception_during_the_process_forwards_that_exception()
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: WithGradient);
        var cost = problem.Cost.WithGradientOverride(_ => throw new TestException());
        
        Action action = () => _minimizer.Minimize(cost, problem.ParameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls()
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: WithGradientAndHessian);
        var referenceProblem = new CubicPolynomialProblem(derivativeConfiguration: WithGradient);
        
        var result = _minimizer.Minimize(problem);
        var referenceResult = _minimizer.Minimize(referenceProblem);
        
        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithSmartDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
    }
    
    [TestCase(double.NaN)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_that_returns_non_finite_values_during_the_process_yields_an_invalid_result_with_non_finite_hessian_exit_condition_and_undefined_covariances(
        double nonFiniteValue)
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: WithGradientAndHessian);
        var parameterConfigurations = problem.ParameterConfigurations;
        var cost = problem.Cost.WithHessianOverride(_ => 
            Enumerable.Repeat(nonFiniteValue, parameterConfigurations.Count * parameterConfigurations.Count).ToArray());
        
        var result = _minimizer.Minimize(cost, parameterConfigurations);
        
        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(NonFiniteHessian);
            x.ParameterCovarianceMatrix.Should().BeNull();
        });
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_that_throws_an_exception_during_the_process_forwards_that_exception()
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: WithGradientAndHessian);
        var cost = problem.Cost.WithHessianOverride(_ => throw new TestException());
        
        Action action = () => _minimizer.Minimize(cost, problem.ParameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
    
    [Test]
    public void when_minimizing_a_cost_function_with_an_analytical_hessian_that_is_not_positive_definite_for_the_initial_parameter_values_and_some_parameters_are_limited_yields_a_result_matching_the_result_obtained_for_numerical_approximation()
    {
        // For the default initial parameter values [2, 1, 0], the cost function Hessian is not positive definite;
        // The rate parameter has a lower limit of 0 by default.
        var problem = new ExponentialDecayProblem(derivativeConfiguration: WithGradientAndHessian);
        var referenceProblem = new ExponentialDecayProblem(derivativeConfiguration: WithoutDerivatives);
        
        var result = _minimizer.Minimize(problem);
        var referenceResult = _minimizer.Minimize(referenceProblem);

        result.Should().MatchExcludingFunctionCalls(referenceResult, options => options.WithSmartDoubleTolerance(0.001));
    }
    
    [Test]
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_parameter_covariances_equal_to_those_for_the_isolated_component(
        [Values] DerivativeConfiguration derivativeConfiguration, 
        [Values] Strategy strategy)
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: derivativeConfiguration, errorDefinitionInSigma: 2);
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var result = _minimizer.Minimize(problem, minimizerConfiguration);
        var sumResult = _minimizer.Minimize(CostFunction.Sum(problem.Cost), problem.ParameterConfigurations, minimizerConfiguration);

        sumResult.ParameterCovarianceMatrix.Should()
            .NotBeNull().And
            .BeApproximately(result.ParameterCovarianceMatrix);
    }

    [Test]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_yields_parameter_covariances_equivalent_to_those_for_the_isolated_components(
        [Values] DerivativeConfiguration derivativeConfiguration, 
        [Values] Strategy strategy)
    { 
        if (strategy == Strategy.Fast) 
            Assert.Ignore("Although the fast minimization strategy yields covariances that roughly agree (within ~10%), " +
                          "they are not strictly equivalent — even when using a minimal convergence tolerance to " +
                          "prevent early termination. Users should be aware of this limitation and are advised to " +
                          "apply error refinement after minimizing cost function sums with the fast strategy when " +
                          "precise parameter uncertainties are required (see complementary test).");

        var problem1 = new CubicPolynomialProblem(derivativeConfiguration: derivativeConfiguration);
        var problem2 = new CubicPolynomialProblem(
            c0: CubicPolynomialProblem.DefaultC0.WithSuffix("2"),
            c1: CubicPolynomialProblem.DefaultC1.WithSuffix("2"),
            c2: CubicPolynomialProblem.DefaultC2.WithSuffix("2"),
            c3: CubicPolynomialProblem.DefaultC3.WithSuffix("2"),
            derivativeConfiguration: derivativeConfiguration, 
            errorDefinitionInSigma: 2);
        var sumProblem = Problem.Sum(problem1, problem2);
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var problem1Result = _minimizer.Minimize(problem1, minimizerConfiguration);
        var problem2Result = _minimizer.Minimize(problem2, minimizerConfiguration);
        var sumProblemResult = _minimizer.Minimize(sumProblem, minimizerConfiguration);

        sumProblemResult.ParameterCovarianceMatrix.Should()
            .NotBeNull().And
            .BeApproximately(problem1Result.ParameterCovarianceMatrix.BlockConcat(problem2Result.ParameterCovarianceMatrix));
    }
    
    [Test]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_using_the_fast_strategy_and_subsequently_applying_error_refinement_yields_parameter_covariances_equivalent_to_those_for_the_isolated_components(
        [Values] DerivativeConfiguration derivativeConfiguration)
    {
        var problem1 = new CubicPolynomialProblem(derivativeConfiguration: derivativeConfiguration);
        var problem2 = new CubicPolynomialProblem(
            c0: CubicPolynomialProblem.DefaultC0.WithSuffix("2"),
            c1: CubicPolynomialProblem.DefaultC1.WithSuffix("2"),
            c2: CubicPolynomialProblem.DefaultC2.WithSuffix("2"),
            c3: CubicPolynomialProblem.DefaultC3.WithSuffix("2"),
            derivativeConfiguration: derivativeConfiguration, 
            errorDefinitionInSigma: 2);
        var sumProblem = Problem.Sum(problem1, problem2);
        var minimizerConfiguration = new MinimizerConfiguration(Strategy.Fast);

        var problem1Result = _minimizer.MinimizeAndRefineErrors(problem1, minimizerConfiguration);
        var problem2Result = _minimizer.MinimizeAndRefineErrors(problem2, minimizerConfiguration);
        var sumProblemResult = _minimizer.MinimizeAndRefineErrors(sumProblem, minimizerConfiguration);

        sumProblemResult.ParameterCovarianceMatrix.Should()
            .NotBeNull().And
            .BeApproximately(problem1Result.ParameterCovarianceMatrix.BlockConcat(problem2Result.ParameterCovarianceMatrix));
    }

    [Test]
    public void when_minimizing_a_cost_function_sum_where_only_some_components_have_an_analytical_hessian_yields_the_same_result_as_if_none_had_an_analytical_gradient(
        [Values] Strategy strategy)
    {
        var problem = Problem.Sum(Problem1(WithGradientAndHessian), Problem2());
        var referenceProblem = Problem.Sum(Problem1(WithoutDerivatives), Problem2());
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        
        var result = _minimizer.Minimize(problem, minimizerConfiguration);
        var referenceResult = _minimizer.Minimize(referenceProblem, minimizerConfiguration);
        
        result.Should().Match(referenceResult);
        return;

        QuadraticPolynomialProblem Problem1(DerivativeConfiguration derivativeConfiguration) =>
            new(derivativeConfiguration: derivativeConfiguration);
        
        QuadraticPolynomialProblem Problem2() =>
            new(c1: QuadraticPolynomialProblem.DefaultC1.WithSuffix("2"), 
                c2: QuadraticPolynomialProblem.DefaultC2.WithSuffix("2"));
    }

    [Test]
    public void when_minimizing_a_cost_function_sum_where_all_components_have_analytical_hessians_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls(
        [Values(1, 2)] double errorDefinitionOfComponent1,
        [Values] Strategy strategy)
    {
        var problem = Problem.Sum(Problem1(WithGradientAndHessian), Problem2(WithGradientAndHessian));
        var referenceProblem = Problem.Sum(Problem1(WithoutDerivatives), Problem2(WithoutDerivatives));
        var minimizerConfiguration = new MinimizerConfiguration(strategy);
        
        var result = _minimizer.Minimize(problem, minimizerConfiguration);
        var referenceResult = _minimizer.Minimize(referenceProblem, minimizerConfiguration);
        
        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithSmartDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
        return;

        QuadraticPolynomialProblem Problem1(DerivativeConfiguration derivativeConfiguration) =>
            new(derivativeConfiguration: derivativeConfiguration, errorDefinitionInSigma: errorDefinitionOfComponent1);

        QuadraticPolynomialProblem Problem2(DerivativeConfiguration derivativeConfiguration) =>
            new(c1: QuadraticPolynomialProblem.DefaultC1.WithSuffix("2"), 
                c2: QuadraticPolynomialProblem.DefaultC2.WithSuffix("2"), 
                derivativeConfiguration: derivativeConfiguration);
    }
}