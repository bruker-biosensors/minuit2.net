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
    private static IEnumerable<TestCaseData> WellDefinedMinimizationProblems()
    {
        foreach (var strategy in Enum.GetValues(typeof(Strategy)))
            yield return new TestCaseData(new CubicPolynomialLeastSquaresProblem(), strategy)
                .SetName($"Cubic polynomial least squares problem using {strategy} strategy");
    }
    
    [TestCaseSource(nameof(WellDefinedMinimizationProblems))]
    public void when_minimizing_a_well_defined_problem_converges_to_a_valid_cost_function_minimum_representing_the_optimum_parameter_values(
        IMinimizationProblem problem,
        Strategy strategy)
    { 
        // A minimal tolerance is used to enforce maximum accuracy (prevent early termination). 
        var minimizerConfiguration = new MinimizerConfiguration(strategy, Tolerance: 0);
        var result = minimizer.Minimize(problem.Cost, problem.ParameterConfigurations, minimizerConfiguration);
        result.Should()
            .HaveExitCondition(MinimizationExitCondition.Converged).And
            .HaveIsValid(true).And
            .HaveParameters(problem.Cost.Parameters).And
            .HaveParameterValues(problem.OptimumParameterValues, relativeTolerance: 0.01);
    }
    
    private static IEnumerable<TestCaseData> MismatchingParameterConfigurationTestCases()
    {
        var cost = Any.InstanceOf<ICostFunction>();

        var mismatchingNames = cost.Parameters.Select(name => ParameterConfiguration(name + Any.String()));
        yield return new TestCaseData(cost, mismatchingNames).SetName("Mismatching parameter names");

        var tooFew = cost.Parameters.Skip(1).Select(ParameterConfiguration);
        yield return new TestCaseData(cost, tooFew).SetName("Too few parameter configurations");

        var tooMany = cost.Parameters.Select(ParameterConfiguration).Concat([ParameterConfiguration(Any.String())]);
        yield return new TestCaseData(cost, tooMany).SetName("Too many parameter configurations");
    }
    
    private static ParameterConfiguration ParameterConfiguration(string name) => Variable(name, Any.Double());
    
    [TestCaseSource(nameof(MismatchingParameterConfigurationTestCases))]
    public void when_asked_to_minimize_a_cost_function_with_mismatching_parameter_configurations_throws_an_exception(
        ICostFunction cost, IEnumerable<ParameterConfiguration> mismatchingParameterConfigurations)
    {
        Action action = () => _ = minimizer.Minimize(cost, mismatchingParameterConfigurations.ToList());
        action.Should().Throw<ArgumentException>();
    }
    
    [Test, Description("Ensures correct parameter-configuration-to-cost-function-parameter mapping.")]
    public void when_minimizing_a_cost_function_yields_the_same_result_independent_of_the_order_parameter_configurations_are_provided_in()
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var orderedConfigurations = CubicPolynomial.ParameterConfigurations.Defaults; 
        var disorderedConfigurations = CubicPolynomial.ParameterConfigurations.Defaults.InRandomOrder().ToArray();
        
        var resultForOrderedConfigurations = minimizer.Minimize(cost, orderedConfigurations);
        var resultForDisorderedConfigurations = minimizer.Minimize(cost, disorderedConfigurations);
        
        resultForDisorderedConfigurations.Should().BeEquivalentTo(resultForOrderedConfigurations);
    }
    
    [TestCase(double.NegativeInfinity, double.PositiveInfinity)]
    [TestCase(double.NaN, double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity, double.NaN)]
    [TestCase(double.NaN, double.NaN)]
    [Description("Ensures minimizer handles infinite parameter limits the same way as if there were no limits.")]
    public void when_minimizing_a_cost_function_yields_the_same_result_for_unlimited_parameters_and_parameters_with_infinite_limits(
        double lowerLimit, double upperLimit)
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var unlimitedParameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var parameterConfigurationsWithInfiniteLimits = CubicPolynomial.ParameterConfigurations.Defaults.WithLimits(lowerLimit, upperLimit);

        var resultForUnlimited = minimizer.Minimize(cost, unlimitedParameterConfigurations);
        var resultForInfiniteLimits = minimizer.Minimize(cost, parameterConfigurationsWithInfiniteLimits);
        
        resultForInfiniteLimits.Should().HaveIsValid(true).And.BeEquivalentTo(resultForUnlimited);
    }
    
    [TestCase(-1E15, 1E15)]
    [TestCase(null, 1E15)]
    [TestCase(-1E15, null)]
    [Description("In the presence of parameter limits, parameter values are projected to internal values for the " +
                 "minimization (see Minuit docs). This projection runs into numeric problems when one or both limits " +
                 "become very large compared to the value. In such a case, we expect the result to be invalid.")]
    public void when_minimizing_a_cost_function_for_extreme_parameter_limits_causing_numerical_issues_in_the_internal_parameter_projection_yields_an_invalid_result(
        double? lowerLimit, double? upperLimit)
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults.WithLimits(lowerLimit, upperLimit);
        
        var result = minimizer.Minimize(cost, parameterConfigurations);
        
        result.Should().HaveIsValid(false);
    }
    
    [Test, Description("Ensures that the minimum cost value is independent of the error definition. " +
                       "This holds only if the minimization converges and doesn't terminate prematurely. " +
                       "Since Minuit defines convergence via the estimated vertical distance to the minimum (EDM), " +
                       "which scales with tolerance and error definition, this test sets a minimum tolerance to " +
                       "prevent early termination for large error definition values.")]
    public void when_minimizing_the_same_cost_function_with_varying_error_definitions_yields_the_same_cost_value()
    {
        var cost = CubicPolynomial.LeastSquaresCost.WithAnyErrorDefinitionBetween(2, 5).Build();
        var referenceCost = CubicPolynomial.LeastSquaresCost.WithErrorDefinition(1).Build();
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var minimizerConfiguration = new MinimizerConfiguration(Tolerance: 0);

        var result = minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);
        var referenceResult = minimizer.Minimize(referenceCost, parameterConfigurations, minimizerConfiguration);
        
        result.CostValue.Should().BeApproximately(referenceResult.CostValue).WithRelativeTolerance(0.001);
    }
    
    private static IEnumerable<TestCaseData> BestValueOutsideLimitsParameterConfigurations()
    {
        // best/true value for parameter c0 is 10 (see CubicPolynomial.cs)
        yield return new TestCaseData(CubicPolynomial.ParameterConfigurations.C0.WithValue(11).WithLimits(10.5, 12), 10.5);
        yield return new TestCaseData(CubicPolynomial.ParameterConfigurations.C0.WithValue(11).WithLimits(10.5, null), 10.5);
        yield return new TestCaseData(CubicPolynomial.ParameterConfigurations.C0.WithValue(9).WithLimits(8, 9.5), 9.5);
        yield return new TestCaseData(CubicPolynomial.ParameterConfigurations.C0.WithValue(9).WithLimits(null, 9.5), 9.5);
    }

    [TestCaseSource(nameof(BestValueOutsideLimitsParameterConfigurations))]
    public void when_minimizing_a_cost_function_optimal_parameter_values_located_outside_the_provided_parameters_limits_yields_a_result_with_affected_parameters_at_their_next_best_limit(
        ParameterConfiguration parameterConfiguration, double expectedValue)
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        ParameterConfiguration[] parameterConfigurations =
        [
            parameterConfiguration,
            CubicPolynomial.ParameterConfigurations.C1,
            CubicPolynomial.ParameterConfigurations.C2,
            CubicPolynomial.ParameterConfigurations.C3
        ];
        
        var result = minimizer.Minimize(cost, parameterConfigurations);
        
        result.ParameterValues.First().Should().BeApproximately(expectedValue).WithRelativeTolerance(0.001);
    }
    
    [Test]
    public async Task when_cancelled_during_a_minimization_process_yields_a_result_with_manually_stopped_exit_condition()
    {
        var resetEvent = new ManualResetEvent(false);
        var cost = CubicPolynomial.LeastSquaresCost.Build().ListeningToResetEvent(resetEvent);
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        
        var cts = new CancellationTokenSource();
        var task = Task.Run(() => minimizer.Minimize(cost, parameterConfigurations, cancellationToken: cts.Token), CancellationToken.None);
        await cts.CancelAsync();
        resetEvent.Set();
        
        var result = await task;
        result.Should().HaveExitCondition(MinimizationExitCondition.ManuallyStopped);
    }
    
    [Test]
    public void when_running_into_the_function_call_limit_during_a_minimization_process_yields_a_result_with_function_calls_exhausted_exit_condition()
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var minimizerConfiguration = new MinimizerConfiguration(MaximumFunctionCalls: 1);
        
        var result = minimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);

        result.Should().HaveExitCondition(MinimizationExitCondition.FunctionCallsExhausted);
    }
    
    [Test]
    public void when_the_cost_function_throws_an_exception_during_a_minimization_process_forwards_that_exception()
    {
        var cost = CostFunction.LeastSquares([0], [0], [], ModelThrowing<TestException>());
        Action action = () => minimizer.Minimize(cost, []);
        action.Should().ThrowExactly<TestException>();
    }

    private static Func<double, IList<double>, double> ModelThrowing<T>() where T : Exception, new() =>
        (_, _) => throw new T();
    
    private class TestException : Exception;

    [Test, Description("Ensures that the inner scaling of gradients by the error definition in the component cost " +
                       "function and the final rescaling works.")]
    public void when_minimizing_a_cost_function_sum_with_a_single_component_yields_a_result_equivalent_to_the_result_for_the_isolated_component(
        [Values] bool hasGradient, 
        [Values] Strategy strategy)
    {
        var component = CubicPolynomial.LeastSquaresCost.WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component);
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var minimizerConfiguration = new MinimizerConfiguration(strategy);

        var componentResult = minimizer.Minimize(component, parameterConfigurations, minimizerConfiguration);
        var sumResult = minimizer.Minimize(sum, parameterConfigurations, minimizerConfiguration);
        
        sumResult.Should().BeEquivalentTo(componentResult, options => options
            .Excluding(x => x.NumberOfFunctionCalls)
            .Excluding(x => x.ParameterCovarianceMatrix)
            .WithRelativeDoubleTolerance(0.001));
    }

    [Test, Description("Ensures that scaling and rescaling by the error definition works on a per-cost basis.")]
    public void when_minimizing_a_cost_function_sum_of_independent_components_with_different_error_definitions_yields_a_result_equivalent_to_the_results_for_the_isolated_components(
            [Values] bool hasGradient,
            [Values] Strategy strategy)
    {
        // While this statement generally holds, the Simplex minimizer fails to produce strictly equivalent results
        // for more complex models (that's why a very simple model is used in this test). This occurs because it can
        // terminate prematurely — even with minimal convergence tolerance — due to its unsound convergence criterion
        // (as noted in the Minuit documentation, where the convergence estimate is described as "largely fantasy").
        // Users should be aware of this limitation and are advised to either choose alternative minimizers or apply
        // additional minimization cycles when strict convergence and accurate results are required.
        
        var component1 = QuadraticPolynomial.LeastSquaresCost.WithParameterSuffix(1).WithGradient(hasGradient).Build();
        var component2 = QuadraticPolynomial.LeastSquaresCost.WithParameterSuffix(2).WithGradient(hasGradient).WithErrorDefinition(2).Build();
        var sum = CostFunction.Sum(component1, component2);
        var parameterConfigurations1 = QuadraticPolynomial.ParameterConfigurations.DefaultsWithSuffix(1);
        var parameterConfigurations2 = QuadraticPolynomial.ParameterConfigurations.DefaultsWithSuffix(2);
        var minimizerConfiguration = new MinimizerConfiguration(strategy, Tolerance: 0);

        var component1Result = minimizer.Minimize(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = minimizer.Minimize(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = minimizer.Minimize(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        sumResult.Should()
            .HaveCostValue(component1Result.CostValue + component2Result.CostValue).And
            .HaveParameterValues(component1Result.ParameterValues.Concat(component2Result.ParameterValues).ToArray());
    }
}