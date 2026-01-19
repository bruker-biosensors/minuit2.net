using AwesomeAssertions;
using ConstrainedNonDeterminism;
using ExampleProblems;
using ExampleProblems.CustomProblems;
using ExampleProblems.MinuitTutorialProblems;
using ExampleProblems.NISTProblems;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.net.UnitTests.TestUtilities;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.net.UnitTests;

public abstract class Any_minimizer(IMinimizer minimizer)
{
    private readonly ConfigurableLeastSquaresProblem _defaultProblem = new CubicPolynomialProblem();
    
    protected static IEnumerable<TestCaseData> WellPosedMinimizationProblems()
    {
        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        {
            yield return TestCase(new QuadraticPolynomialProblem().Configured(), nameof(QuadraticPolynomialProblem));
            yield return TestCase(new CubicPolynomialProblem().Configured(), nameof(CubicPolynomialProblem));
            yield return TestCase(new ExponentialDecayProblem().Configured(x => x.WithParameter(1).WithLimits(0, null)), nameof(ExponentialDecayProblem));
            yield return TestCase(new BellCurveProblem().Configured(x => x.WithParameter(1).WithLimits(0, null)), nameof(BellCurveProblem));
            yield return TestCase(new NumericalPendulumProblem(), nameof(NumericalPendulumProblem));
            yield return TestCase(new SurfaceBiosensorBindingKineticsProblem(), nameof(SurfaceBiosensorBindingKineticsProblem));
            continue;

            TestCaseData TestCase(IConfiguredProblem problem, string problemName) =>
                new TestCaseData(problem, strategy).SetName($"{problemName} ({strategy})");
        }
    }

    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    public void when_minimizing_a_well_posed_problem_finds_the_optimum_parameter_values(
        IConfiguredProblem problem,
        Strategy strategy)
    { 
        var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);
        
        var result = minimizer.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);

        result.ParameterValues.Should().BeApproximately(problem.OptimumParameterValues,
            relativeToleranceForNonZeros: 0.01, toleranceForZeros: 0.01);
    }
    
    private static IEnumerable<TestCaseData> ChallengingMinimizationProblems()
    {
        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        foreach (DerivativeConfiguration derivativeConfiguration in Enum.GetValues(typeof(DerivativeConfiguration)))
        {
            // Minuit tutorial problems
            yield return TestCase(new RosenbrockProblem(derivativeConfiguration), nameof(RosenbrockProblem));
            yield return TestCase(new WoodProblem(derivativeConfiguration), nameof(WoodProblem));
            yield return TestCase(new PowellProblem(derivativeConfiguration), nameof(PowellProblem));
            yield return TestCase(new FletcherPowellProblem(derivativeConfiguration), nameof(FletcherPowellProblem));
            yield return TestCase(new GoldsteinPriceProblem(derivativeConfiguration), nameof(GoldsteinPriceProblem));
            
            // NIST problems ranked as difficult 
            yield return TestCase(new Mgh09Problem(derivativeConfiguration), nameof(Mgh09Problem));
            yield return TestCase(new ThurberProblem(derivativeConfiguration), nameof(ThurberProblem));
            yield return TestCase(new BoxBodProblem(derivativeConfiguration), nameof(BoxBodProblem));
            yield return TestCase(new Rat42Problem(derivativeConfiguration), nameof(Rat42Problem));
            yield return TestCase(new Rat43Problem(derivativeConfiguration), nameof(Rat43Problem));
            yield return TestCase(new Bennett5Problem(derivativeConfiguration), nameof(Bennett5Problem));
            continue;

            TestCaseData TestCase(IConfiguredProblem problem, string problemName) => 
                new TestCaseData(problem, strategy).SetName($"{problemName}.{derivativeConfiguration} ({strategy})");
        }
    }

    [Test]
    public Task fails_for_the_following_challenging_problems()
    {
        var report = new List<string>();
        foreach (var testCase in ChallengingMinimizationProblems().OrderBy(x => x.TestName))
        {
            var problem = (IConfiguredProblem)testCase.Arguments[0]!;
            var strategy = (Strategy)testCase.Arguments[1]!;
            
            var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);
            var result = minimizer.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);

            try
            {
                result.ParameterValues.Should().BeApproximately(problem.OptimumParameterValues, 
                    relativeToleranceForNonZeros: 0.01, toleranceForZeros: 0.01);
            }
            catch (Exception)
            {
                report.Add(testCase.TestName!);
            }
        }
        
        return Verify(report);
    }
    
    private static IEnumerable<TestCaseData> InvalidParameterConfigurationTestCases()
    {
        var cost = Any.InstanceOf<ICostFunction>();

        var missing = cost.Parameters.Skip(1).Select(AnyConfig);
        yield return new TestCaseData(cost, missing)
            .SetName("Missing parameter configurations");
        
        var duplicates = cost.Parameters.Select(AnyConfig).Concat([AnyConfig(cost.Parameters[0])]);
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

        result.Should().Match(referenceResult);
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
        
        resultForDisorderedConfigurations.Should().Match(resultForOrderedConfigurations);
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

        resultForInfiniteLimits.Should().Match(resultForUnlimited);
    }
    
    [TestCase(-1E15, 1E15)]
    [TestCase(null, 1E15)]
    [TestCase(-1E15, null)]
    [TestCase(-1e3, 1e10, Description = "Best value are much closer to the lower limit.")]
    [Description("When parameter limits are specified, values are projected to an internal representation for the " +
                 "purpose of minimization (see the Minuit documentation). This projection can suffer from numerical " +
                 "instability when one or both limits are extremely large compared to the parameter value, or — under " +
                 "double-sided limits — when the optimal value lies disproportionately close to one of the bounds. " +
                 "In such cases, we expect an invalid minimization result.")]
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
        // the optimum value for parameter c0 is 10 (see CubicPolynomialLeastSquaresProblem.cs)
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
        
        result.ParameterValues[0].Should().BeApproximately(expectedValue);
    }
    
    [Test]
    public void when_cancelled_during_a_minimization_process_yields_an_invalid_result_with_manually_stopped_exit_condition_and_undefined_covariances_representing_the_last_state_of_the_process()
    {
        var cts = new CancellationTokenSource();
        const int numberOfFunctionCallsBeforeCancellation = 25;
        var cost = _defaultProblem.Cost.Build().WithAutoCancellation(cts, numberOfFunctionCallsBeforeCancellation);
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();

        var result = minimizer.Minimize(cost, parameterConfigurations, cancellationToken: cts.Token);

        result.ShouldFulfill(x =>
        {
            x.Should().HaveNumberOfFunctionCalls(numberOfFunctionCallsBeforeCancellation + 1);
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(MinimizationExitCondition.ManuallyStopped);
            x.ParameterCovarianceMatrix.Should().BeNull();
            
            var computedCostValue = cost.ValueFor(x.ParameterValues);
            var initialCostValue = cost.ValueFor(parameterConfigurations);
            x.CostValue.Should()
                .Be(computedCostValue).And
                .BeLessThanOrEqualTo(initialCostValue);
        });
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
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_a_result_matching_the_result_for_the_isolated_component(
        [Values] bool hasGradient, 
        [Values] Strategy strategy)
    {
        var component = _defaultProblem.Cost.WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component);
        var parameterConfigurations = _defaultProblem.ParameterConfigurations.Build();
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var componentResult = minimizer.Minimize(component, parameterConfigurations, minimizerConfiguration);
        var sumResult = minimizer.Minimize(sum, parameterConfigurations, minimizerConfiguration);
        
        sumResult.Should().MatchExcludingFunctionCalls(componentResult, options => options
            .Excluding(x => x.ParameterCovarianceMatrix)  // is null for non-gradient-based minimizers (Simplex)
            .WithSmartDoubleTolerance(0.001));
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
        var problem = new QuadraticPolynomialProblem();
        var component1 = problem.Cost.WithParameterSuffixes("1").WithGradient(hasGradient).Build();
        var component2 = problem.Cost.WithParameterSuffixes("2").WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = problem.ParameterConfigurations.WithSuffix("1").Build();
        var parameterConfigurations2 = problem.ParameterConfigurations.WithSuffix("2").Build();
        var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);

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
    [Description("Ensures that the result indicates process termination due to an non-finite cost value. The Minuit2 " +
                 "code silently fails in this case with an undefined exit condition.")]
    public void when_the_cost_function_returns_a_non_finite_value_during_a_minimization_process_yields_an_invalid_result_with_non_finite_value_exit_condition_and_undefined_covariances(
        double nonFiniteValue)
    {
        var problem = new QuadraticPolynomialProblem();
        const int numberOfValidFunctionCalls = 5;
        var cost = problem.Cost.Build().WithValueOverride(_ => nonFiniteValue, numberOfValidFunctionCalls);
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        var result = minimizer.Minimize(cost, parameterConfigurations);

        result.ShouldFulfill(x =>
        {
            x.Should().HaveNumberOfFunctionCalls(numberOfValidFunctionCalls + 1);
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteValue);
            x.ParameterCovarianceMatrix.Should().BeNull();
        });
    }
    
    [Test]
    public void when_the_cost_function_value_calculation_throws_an_exception_during_a_minimization_process_forwards_that_exception()
    {
        var problem = new QuadraticPolynomialProblem();
        var cost = problem.Cost.Build().WithValueOverride(_ => throw new TestException());
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        
        Action action = () => minimizer.Minimize(cost, parameterConfigurations);
        
        action.Should().ThrowExactly<TestException>();
    }
}