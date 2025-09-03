using FluentAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public abstract class Any_minimizer(IMinimizer minimizer)
{
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
    public void when_minimizing_a_cost_function_for_extreme_parameter_limits_leading_to_numerical_issues_in_the_internal_parameter_projection_yields_an_invalid_result(
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
    
    [Test]
    public async Task when_minimization_is_cancelled_during_the_process_yields_a_result_with_manually_stopped_exit_condition()
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
}