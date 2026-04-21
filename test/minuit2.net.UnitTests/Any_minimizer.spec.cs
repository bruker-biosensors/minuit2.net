using AwesomeAssertions;
using ExampleProblems;
using ExampleProblems.CustomProblems;
using ExampleProblems.MinuitTutorialProblems;
using ExampleProblems.NISTProblems;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.net.UnitTests.TestUtilities;
using static minuit2.net.MinimizationExitCondition;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.net.UnitTests;

public abstract class Any_minimizer(IMinimizer minimizer)
{
    protected static IEnumerable<TestCaseData> WellPosedMinimizationProblems()
    {
        foreach (Strategy strategy in Enum.GetValues(typeof(Strategy)))
        {
            yield return TestCase(new QuadraticPolynomialProblem().WithVariablesAnywhereCloseToOptimumValues(), nameof(QuadraticPolynomialProblem));
            yield return TestCase(new CubicPolynomialProblem().WithVariablesAnywhereCloseToOptimumValues(), nameof(CubicPolynomialProblem));
            yield return TestCase(new ExponentialDecayProblem().WithVariablesAnywhereCloseToOptimumValues(), nameof(ExponentialDecayProblem));
            yield return TestCase(new BellCurveProblem().WithVariablesAnywhereCloseToOptimumValues(), nameof(BellCurveProblem));
            yield return TestCase(new NumericalPendulumProblem(), nameof(NumericalPendulumProblem));
            yield return TestCase(new SurfaceBiosensorBindingKineticsProblem(), nameof(SurfaceBiosensorBindingKineticsProblem));
            continue;

            TestCaseData TestCase(IProblem problem, string problemName) =>
                new TestCaseData(problem, strategy).SetName($"{problemName} ({strategy})");
        }
    }

    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    public void when_minimizing_a_well_posed_problem_finds_the_optimum_parameter_values(
        IProblem problem,
        Strategy strategy)
    {
        var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);

        var result = minimizer.Minimize(problem, minimizerConfiguration);

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

            TestCaseData TestCase(IProblem problem, string problemName) =>
                new TestCaseData(problem, strategy).SetName($"{problemName}.{derivativeConfiguration} ({strategy})");
        }
    }

    [Test, Explicit]
    public Task fails_for_the_following_challenging_problems()
    {
        var report = new List<string>();
        foreach (var testCase in ChallengingMinimizationProblems().OrderBy(x => x.TestName))
        {
            var problem = (IProblem)testCase.Arguments[0]!;
            var strategy = (Strategy)testCase.Arguments[1]!;

            var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);
            var result = minimizer.Minimize(problem, minimizerConfiguration);

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
        var problem = new CubicPolynomialProblem();
        var excessParameterConfigurations = problem.ParameterConfigurations.Concat([AnyConfig("excess")]).ToArray();

        var result = minimizer.Minimize(problem.Cost, excessParameterConfigurations);
        var referenceResult = minimizer.Minimize(problem);

        result.Should().Match(referenceResult);
    }

    [Test]
    [Description("Ensures correct parameter-configuration-to-cost-function-parameter mapping.")]
    public void when_minimizing_a_cost_function_yields_the_same_result_independent_of_the_order_parameter_configurations_are_provided_in()
    {
        var problem = new CubicPolynomialProblem();
        var disorderedConfigurations = problem.ParameterConfigurations.InRandomOrder().ToList();

        var result = minimizer.Minimize(problem.Cost, disorderedConfigurations);
        var referenceResult = minimizer.Minimize(problem);

        result.Should().Match(referenceResult);
    }

    [TestCase(double.NegativeInfinity, double.PositiveInfinity)]
    [TestCase(double.NaN, double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity, double.NaN)]
    [TestCase(double.NaN, double.NaN)]
    public void when_minimizing_a_cost_function_yields_the_same_result_for_unlimited_parameters_and_parameters_with_infinite_limits(
        double lowerLimit,
        double upperLimit)
    {
        var problem = new CubicPolynomialProblem();
        var parameterConfigurationsWithInfiniteLimits = problem.ParameterConfigurations
            .Select(x => x.WithLimits(lowerLimit, upperLimit)).ToList();

        var result = minimizer.Minimize(problem.Cost, parameterConfigurationsWithInfiniteLimits);
        var referenceResult = minimizer.Minimize(problem);

        result.Should().Match(referenceResult);
    }

    [TestCase(-1E16, 1E16)]
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
        var problem = new CubicPolynomialProblem();
        var parameterConfigurationsWithExtremeLimits = problem.ParameterConfigurations
            .Select(x => x.WithLimits(lowerLimit, upperLimit)).ToList();

        var result = minimizer.Minimize(problem.Cost, parameterConfigurationsWithExtremeLimits);

        result.IsValid.Should().BeFalse();
    }

    [Test]
    [Description("Ensures that the minimum cost value is independent of the error definition. This holds only if the " +
                 "minimization converges and doesn't terminate prematurely. Since Minuit defines convergence via the " +
                 "estimated vertical distance to the minimum (EDM), which scales with tolerance and error definition, " +
                 "this test sets a minimum tolerance to prevent early termination for large error definition values.")]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_the_same_cost_value()
    {
        var problem = new CubicPolynomialProblem(errorDefinitionInSigma: Any.Double().Between(2, 5));
        var referenceProblem = new CubicPolynomialProblem(errorDefinitionInSigma: 1);
        var minimizerConfiguration = new MinimizerConfiguration(Tolerance: 0);

        var result = minimizer.Minimize(problem, minimizerConfiguration);
        var referenceResult = minimizer.Minimize(referenceProblem, minimizerConfiguration);

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
        var problem = new CubicPolynomialProblem(c0: Variable("c0", initialValue, lowerLimit, upperLimit));

        var result = minimizer.Minimize(problem);

        result.ParameterValues[0].Should().BeApproximately(expectedValue);
    }

    [Test]
    public void when_cancelled_during_a_minimization_process_yields_an_invalid_result_with_manually_stopped_exit_condition_and_undefined_covariances_representing_the_last_state_of_the_process()
    {
        var cts = new CancellationTokenSource();
        const int numberOfFunctionCallsBeforeCancellation = 25;
        var problem = new CubicPolynomialProblem();
        var cost = problem.Cost.WithAutoCancellation(cts, numberOfFunctionCallsBeforeCancellation);

        var result = minimizer.Minimize(cost, problem.ParameterConfigurations, cancellationToken: cts.Token);

        result.ShouldFulfill(x =>
        {
            x.Should().HaveNumberOfFunctionCalls(numberOfFunctionCallsBeforeCancellation + 1);
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(ManuallyStopped);
            x.ParameterCovarianceMatrix.Should().BeNull();

            var computedCostValue = cost.ValueFor(x.ParameterValues);
            var initialCostValue = cost.ValueFor(problem.ParameterConfigurations);
            x.CostValue.Should()
                .Be(computedCostValue).And
                .BeLessThanOrEqualTo(initialCostValue);
        });
    }

    [Test]
    public void when_running_into_the_function_call_limit_during_a_minimization_process_yields_a_result_with_function_calls_exhausted_exit_condition()
    {
        var problem = new CubicPolynomialProblem();
        var minimizerConfiguration = new MinimizerConfiguration(MaximumFunctionCalls: 1);

        var result = minimizer.Minimize(problem, minimizerConfiguration);

        result.ExitCondition.Should().Be(FunctionCallsExhausted);
    }

    [Test]
    [Description("Ensures that the inner scaling of gradients by the error definition in the component cost function " +
                 "and the final rescaling works.")]
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_a_result_matching_the_result_for_the_isolated_component(
        [Values] DerivativeConfiguration derivativeConfiguration,
        [Values] Strategy strategy)
    {
        var problem = new CubicPolynomialProblem(derivativeConfiguration: derivativeConfiguration, errorDefinitionInSigma: 2);
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var result = minimizer.Minimize(problem, minimizerConfiguration);
        var sumResult = minimizer.Minimize(CostFunction.Sum(problem.Cost), problem.ParameterConfigurations, minimizerConfiguration);

        sumResult.Should().MatchExcludingFunctionCalls(result, options => options
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
            [Values] DerivativeConfiguration derivativeConfiguration,
            [Values] Strategy strategy)
    {
        var problem1 = new QuadraticPolynomialProblem(derivativeConfiguration: derivativeConfiguration);
        var problem2 = new QuadraticPolynomialProblem(
            c0: QuadraticPolynomialProblem.DefaultC0.WithSuffix("2"),
            c1: QuadraticPolynomialProblem.DefaultC1.WithSuffix("2"),
            c2: QuadraticPolynomialProblem.DefaultC2.WithSuffix("2"),
            derivativeConfiguration: derivativeConfiguration,
            errorDefinitionInSigma: 2);
        var problemSum = Problem.Sum(problem1, problem2);
        var minimizerConfiguration = new MaximumAccuracyMinimizerConfiguration(strategy);

        var problem1Result = minimizer.Minimize(problem1, minimizerConfiguration);
        var problem2Result = minimizer.Minimize(problem2, minimizerConfiguration);
        var problemSumResult = minimizer.Minimize(problemSum, minimizerConfiguration);

        problemSumResult.ShouldFulfill(x =>
        {
            x.CostValue.Should().BeApproximately(problem1Result.CostValue + problem2Result.CostValue);
            x.ParameterValues.Should().BeApproximately(problem1Result.ParameterValues.Concat(problem2Result.ParameterValues));
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
        var cost = problem.Cost.WithValueOverride(_ => nonFiniteValue, numberOfValidFunctionCalls);

        var result = minimizer.Minimize(cost, problem.ParameterConfigurations);

        result.ShouldFulfill(x =>
        {
            x.Should().HaveNumberOfFunctionCalls(numberOfValidFunctionCalls + 1);
            x.IsValid.Should().BeFalse();
            x.ExitCondition.Should().Be(NonFiniteValue);
            x.ParameterCovarianceMatrix.Should().BeNull();
        });
    }

    [Test]
    public void when_the_cost_function_value_calculation_throws_an_exception_during_a_minimization_process_forwards_that_exception()
    {
        var problem = new QuadraticPolynomialProblem();
        var cost = problem.Cost.WithValueOverride(_ => throw new TestException());

        Action action = () => minimizer.Minimize(cost, problem.ParameterConfigurations);

        action.Should().ThrowExactly<TestException>();
    }
}
