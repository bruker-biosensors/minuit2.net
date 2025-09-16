using AwesomeAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.MinimizationProblems;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public abstract class Any_minimizer(IMinimizer minimizer)
{
    private readonly ConfigurableLeastSquaresProblem _defaultProblem = new CubicPolynomialLeastSquaresProblem();
    
    protected static IEnumerable<TestCaseData> WellPosedMinimizationProblems()
    {
        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        {
            yield return TestCase(new QuadraticPolynomialLeastSquaresProblem().Configured(), nameof(QuadraticPolynomialLeastSquaresProblem));
            yield return TestCase(new CubicPolynomialLeastSquaresProblem().Configured(), nameof(CubicPolynomialLeastSquaresProblem));
            yield return TestCase(new ExponentialDecayLeastSquaresProblem().Configured(), nameof(ExponentialDecayLeastSquaresProblem));
            yield return TestCase(new BellCurveLeastSquaresProblem().Configured(x => x.WithParameter(1).WithLimits(0, null)), nameof(BellCurveLeastSquaresProblem));
            continue;

            TestCaseData TestCase(ConfiguredProblem problem, string problemName) =>
                new TestCaseData(problem, strategy).SetArgDisplayNames(problemName, strategy.ToString());
        }
    }

    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    public void when_minimizing_a_well_posed_problem_converges_to_a_valid_cost_function_minimum_representing_the_optimum_parameter_values(
        ConfiguredProblem problem,
        Strategy strategy)
    { 
        // A minimal tolerance is used to enforce maximum accuracy (prevent early termination). 
        var minimizerConfiguration = new MinimizerConfiguration(strategy, Tolerance: 0);
        
        var result = minimizer.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
        
        result.ShouldFulfill(x =>
        {
            //x.ExitCondition.Should().Be(MinimizationExitCondition.Converged);
            //x.IsValid.Should().BeTrue();
            x.CostValue.Should().BeLessThan(problem.InitialCostValue());
            x.ParameterValues.Should().BeApproximately(problem.OptimumParameterValues, relativeTolerance: 0.01);
            x.FaultParameterValues.Should().BeNull();
        });
    }
    
    private static IEnumerable<TestCaseData> InvalidParameterConfigurationTestCases()
    {
        var cost = Any.InstanceOf<ICostFunction>();

        var missing = cost.Parameters.Skip(1).Select(AnyConfig);
        yield return new TestCaseData(cost, missing)
            .SetName("Missing parameter configurations");
        
        var duplicates = cost.Parameters.Select(AnyConfig).Concat([AnyConfig(cost.Parameters.First())]);
        yield return new TestCaseData(cost, duplicates)
            .SetName("Matching parameter configurations with additional duplicate configuration");
        
        var mismatching = cost.Parameters.Select(name => AnyConfig(name + Any.String()));
        yield return new TestCaseData(cost, mismatching)
            .SetName("Matching number of parameter configurations but mismatching names");
    }

    private static ParameterConfiguration AnyConfig(string name) => Variable(name, Any.Double());

    [TestCaseSource(nameof(InvalidParameterConfigurationTestCases))]
    public void when_asked_to_minimize_a_cost_function_with_invalid_parameter_configurations_throws_an_exception(
        ICostFunction cost, 
        IEnumerable<ParameterConfiguration> mismatchingParameterConfigurations)
    {
        Action action = () => _ = minimizer.Minimize(cost, mismatchingParameterConfigurations.ToList());
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void when_minimizing_a_cost_function_with_parameter_configurations_that_contain_unique_matches_for_all_cost_parameters_ignores_any_excess_configurations()
    {
        var costFunction = _defaultProblem.Cost.Build();
        var matchingParameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var excessParameterConfigurations = matchingParameterConfigurations.Concat([AnyConfig("excess")]).ToArray();

        var result = minimizer.Minimize(costFunction, excessParameterConfigurations);
        var referenceResult = minimizer.Minimize(costFunction, matchingParameterConfigurations);

        result.Should().BeEquivalentTo(referenceResult);
    }

    [Test]
    [Description("Ensures correct parameter-configuration-to-cost-function-parameter mapping.")]
    public void when_minimizing_a_cost_function_yields_the_same_result_independent_of_the_order_parameter_configurations_are_provided_in()
    {
        var cost = _defaultProblem.Cost.Build();
        var orderedConfigurations = _defaultProblem.ParameterConfigurations.Build(); 
        var disorderedConfigurations = _defaultProblem.ParameterConfigurations.InRandomOrder().Build();
        
        var resultForOrderedConfigurations = minimizer.Minimize(cost, orderedConfigurations);
        var resultForDisorderedConfigurations = minimizer.Minimize(cost, disorderedConfigurations);
        
        resultForDisorderedConfigurations.Should().BeEquivalentTo(resultForOrderedConfigurations);
    }
    
    [TestCase(double.NegativeInfinity, double.PositiveInfinity)]
    [TestCase(double.NaN, double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity, double.NaN)]
    [TestCase(double.NaN, double.NaN)]
    public void when_minimizing_a_cost_function_yields_the_same_result_for_unlimited_parameters_and_parameters_with_infinite_limits(
        double lowerLimit, 
        double upperLimit)
    {
        var cost = _defaultProblem.Cost.Build();
        var unlimitedParameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var parameterConfigurationsWithInfiniteLimits = _defaultProblem.ParameterConfigurations.WithLimits(lowerLimit, upperLimit).Build();

        var resultForUnlimited = minimizer.Minimize(cost, unlimitedParameterConfigurations);
        var resultForInfiniteLimits = minimizer.Minimize(cost, parameterConfigurationsWithInfiniteLimits);
        
        resultForInfiniteLimits.Should().BeEquivalentTo(resultForUnlimited);
    }
    
    [TestCase(-1E15, 1E15)]
    [TestCase(null, 1E15)]
    [TestCase(-1E15, null)]
    [Description("In the presence of parameter limits, parameter values are projected to internal values for the " +
                 "minimization (see Minuit docs). This projection runs into numeric problems when one or both limits " +
                 "become very large compared to the value. In such a case, we expect the result to be invalid.")]
    public void when_minimizing_a_cost_function_for_extreme_parameter_limits_causing_numerical_issues_in_the_internal_parameter_projection_yields_an_invalid_result(
        double? lowerLimit, 
        double? upperLimit)
    {
        var cost = _defaultProblem.Cost.Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.WithLimits(lowerLimit, upperLimit).Build();
        
        var result = minimizer.Minimize(cost, parameterConfigurations);
        
        result.IsValid.Should().BeFalse();
    }

    [Test]
    [Description("Ensures that the minimum cost value is independent of the error definition. This holds only if the " +
                 "minimization converges and doesn't terminate prematurely. Since Minuit defines convergence via the " +
                 "estimated vertical distance to the minimum (EDM), which scales with tolerance and error definition, " +
                 "this test sets a minimum tolerance to prevent early termination for large error definition values.")]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_the_same_cost_value()
    {
        var cost = _defaultProblem.Cost.WithErrorDefinition(Any.Double().Between(2, 5)).Build();
        var referenceCost = _defaultProblem.Cost.WithErrorDefinition(1).Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var minimizerConfiguration = new MinimizerConfiguration(Tolerance: 0);

        var result = minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);
        var referenceResult = minimizer.Minimize(referenceCost, parameterConfigurations, minimizerConfiguration);
        
        result.CostValue.Should().BeApproximately(referenceResult.CostValue);
    }
    
    private static IEnumerable<TestCaseData> BestValueOutsideLimitsParameterConfigurations()
    {
        // optimum value for parameter c0 is 10 (see CubicPolynomialLeastSquaresProblem.cs)
        yield return new TestCaseData(11.0, 10.5, 12.0, 10.5);
        yield return new TestCaseData(11.0, 10.5, null, 10.5);
        yield return new TestCaseData(9.0, 8.0, 9.5, 9.5);
        yield return new TestCaseData(9.0, null, 9.5, 9.5);
    }

    [TestCaseSource(nameof(BestValueOutsideLimitsParameterConfigurations))]
    public void when_minimizing_a_cost_function_with_optimal_parameter_values_located_outside_the_provided_parameters_limits_yields_a_result_with_the_affected_parameters_at_their_next_best_limit(
        double initialValue, 
        double? lowerLimit, 
        double? upperLimit, 
        double expectedValue)
    {
        var cost = _defaultProblem.Cost.Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations
            .WithParameter(0).WithValue(initialValue).WithLimits(lowerLimit, upperLimit)
            .Build();
        
        var result = minimizer.Minimize(cost, parameterConfigurations);
        
        result.ParameterValues.First().Should().BeApproximately(expectedValue);
    }
    
    [Test]
    public async Task when_cancelled_during_a_minimization_process_yields_a_result_with_manually_stopped_exit_condition()
    {
        var resetEvent = new ManualResetEvent(false);
        var cost = _defaultProblem.Cost.Build().ListeningToResetEvent(resetEvent);
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        
        var cts = new CancellationTokenSource();
        var task = Task.Run(() => minimizer.Minimize(cost, parameterConfigurations, cancellationToken: cts.Token), CancellationToken.None);
        await cts.CancelAsync();
        resetEvent.Set();
        var result = await task;
        
        result.ExitCondition.Should().Be(MinimizationExitCondition.ManuallyStopped);
    }
    
    [Test]
    public void when_running_into_the_function_call_limit_during_a_minimization_process_yields_a_result_with_function_calls_exhausted_exit_condition()
    {
        var cost = _defaultProblem.Cost.Build();
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var minimizerConfiguration = new MinimizerConfiguration(MaximumFunctionCalls: 1);
        
        var result = minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);

        result.ExitCondition.Should().Be(MinimizationExitCondition.FunctionCallsExhausted);
    }

    [Test]
    [Description("Ensures that the inner scaling of gradients by the error definition in the component cost function " +
                 "and the final rescaling works.")]
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_a_result_equivalent_to_the_result_for_the_isolated_component(
        [Values] bool hasGradient, 
        [Values] Strategy strategy)
    {
        var component = _defaultProblem.Cost.WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component);
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var componentResult = minimizer.Minimize(component, parameterConfigurations, minimizerConfiguration);
        var sumResult = minimizer.Minimize(sum, parameterConfigurations, minimizerConfiguration);
        
        sumResult.Should().BeEquivalentTo(componentResult, options => options
            .Excluding(x => x.NumberOfFunctionCalls)
            .Excluding(x => x.ParameterCovarianceMatrix)
            .WithRelativeDoubleTolerance(0.001));
    }

    [Test]
    [Description("Ensures that scaling and rescaling by the error definition works on a per-cost basis. While this " +
                 "generally holds, the Simplex minimizer fails to produce strictly equivalent results for more " +
                 "complex models (that's why this test uses a simple quadratic polynomial). This occurs because it " +
                 "can terminate prematurely — even with minimal convergence tolerance — due to its unsound " +
                 "convergence criterion (as noted in the Minuit documentation, where the convergence estimate is " +
                 "described as 'largely fantasy'). Users should be aware of this limitation and are advised to " +
                 "either choose alternative minimizers or apply additional minimization cycles when strict " +
                 "convergence and accurate results are required.")]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_yields_a_result_equivalent_to_the_results_for_the_isolated_components(
            [Values] bool hasGradient,
            [Values] Strategy strategy)
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var component1 = problem.Cost.WithParameterSuffixes("1").WithGradient(hasGradient).Build();
        var component2 = problem.Cost.WithParameterSuffixes("2").WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = problem.ParameterConfigurations.WithSuffix("1").Build();
        var parameterConfigurations2 = problem.ParameterConfigurations.WithSuffix("2").Build();
        var minimizerConfiguration = new MinimizerConfiguration(strategy, Tolerance: 0);

        var component1Result = minimizer.Minimize(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = minimizer.Minimize(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = minimizer.Minimize(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        sumResult.ShouldFulfill(x =>
        {
            x.CostValue.Should().BeApproximately(component1Result.CostValue + component2Result.CostValue);
            x.ParameterValues.Should().BeApproximately(component1Result.ParameterValues.Concat(component2Result.ParameterValues));
        });
    }

    [TestCase(double.NaN)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    [Description("Ensures that the result indicates process termination due to an non-finite value. The Minuit2 code " +
                 "silently fails in this case and just returns a minimum corresponding to the last valid iteration.")]
    public void when_the_cost_function_returns_a_non_finite_value_during_a_minimization_process_yields_an_invalid_result_with_non_finite_value_exit_condition(
        double nonFiniteValue)
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var cost = problem.Cost.Build().WithValueSwitchingTo(_ => nonFiniteValue);
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        var result = minimizer.Minimize(cost, parameterConfigurations);

        result.ShouldFulfill(x =>
        {
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteValue);
            x.FaultParameterValues.Should()
                .NotBeNull().And
                .Fulfill(p => cost.ValueFor(p).Should().Be(nonFiniteValue));
        });
    }
    
    [Test]
    public void when_the_cost_function_value_calculation_throws_an_exception_during_a_minimization_process_forwards_that_exception()
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var cost = problem.Cost.Build().WithValueSwitchingTo(_ => throw new TestException());
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        Action action = () => minimizer.Minimize(cost, parameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
}