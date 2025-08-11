using FluentAssertions;
using minuit2.net;
using minuit2.net.costFunctions;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.UnitTests;

public class A_cost_function
{
    [Test, Description("Ensures correct parameter-configuration-to-cost-function-parameter mapping.")]
    public void when_minimized_yields_the_same_result_independent_of_the_order_parameter_configurations_are_provided_in()
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var orderedConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var disorderedConfigurations = CubicPolynomial.ParameterConfigurations.Defaults.InRandomOrder().ToArray();

        var resultForOrderedConfigurations = MigradMinimizer.Minimize(cost, orderedConfigurations);
        var resultForDisorderedConfigurations = MigradMinimizer.Minimize(cost, disorderedConfigurations);

        resultForDisorderedConfigurations.Should().BeEquivalentTo(resultForOrderedConfigurations);
    }

    [TestCase(double.NegativeInfinity, double.PositiveInfinity)]
    [TestCase(double.NaN, double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity, double.NaN)]
    [TestCase(double.NaN, double.NaN)]
    [Description("Ensures minimizer handles infinite parameter limits the same way as if there were no limits.")]
    public void can_be_minimized_with_infinite_parameter_limits(double lowerLimit, double upperLimit)
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        ParameterConfiguration[] parameterConfigurations =
        [
            CubicPolynomial.ParameterConfigurations.C0.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C1.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C2.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C3.WithLimits(lowerLimit, upperLimit),
        ];

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.Should().HaveIsValid(true);
    }

    [TestCase(-1E15, 1E15)]
    [TestCase(null, 1E15)]
    [TestCase(-1E15, null)]
    [Description("In the presence of parameter limits, parameter values are projected to internal values for the " +
                 "minimization (see Minuit docs). This projection runs into numeric problems when one or both limits " +
                 "become very large compared to the value. In such a case, we expect the result to be invalid.")]
    public void when_minimized_with_parameter_limits_leading_to_numerical_issues_in_the_internal_parameter_projection_yields_an_invalid_result(
        double? lowerLimit, double? upperLimit)
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        ParameterConfiguration[] parameterConfigurations =
        [
            CubicPolynomial.ParameterConfigurations.C0.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C1,
            CubicPolynomial.ParameterConfigurations.C2,
            CubicPolynomial.ParameterConfigurations.C3
        ];

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.Should().HaveIsValid(false);
    }

    private static IEnumerable<CubicPolynomial.LeastSquaresBuilder> CostFunctionWithVaryingErrorDefinitionTestCases()
    {
        yield return CubicPolynomial.LeastSquaresCost;
        yield return CubicPolynomial.LeastSquaresCost.WithMissingYErrors();
        yield return CubicPolynomial.LeastSquaresCost.WithGradient();
        yield return CubicPolynomial.LeastSquaresCost.WithGradient().WithMissingYErrors();
    }

    [TestCaseSource(nameof(CostFunctionWithVaryingErrorDefinitionTestCases))]
    public void when_minimized_yields_the_same_cost_value_independent_of_its_error_definition(
        CubicPolynomial.LeastSquaresBuilder costBuilder)
    {
        var referenceCost = costBuilder.WithErrorDefinition(1).Build();
        var cost = costBuilder.WithAnyErrorDefinitionBetween(2, 5).Build();

        var referenceResult = MigradMinimizer.Minimize(referenceCost, CubicPolynomial.ParameterConfigurations.Defaults);
        var result = MigradMinimizer.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);

        result.CostValue.Should().BeApproximately(referenceResult.CostValue, 1E-10);
    }

    [TestCaseSource(nameof(CostFunctionWithVaryingErrorDefinitionTestCases))]
    public void when_minimized_yields_parameter_covariances_that_directly_scale_with_the_error_definition(
        CubicPolynomial.LeastSquaresBuilder costBuilder)
    {
        var referenceCost = costBuilder.WithErrorDefinition(1).Build();
        var cost = costBuilder.WithAnyErrorDefinitionBetween(2, 5).Build();

        var referenceResult = MigradMinimizer.Minimize(referenceCost, CubicPolynomial.ParameterConfigurations.Defaults);
        var result = MigradMinimizer.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);

        result.ParameterCovarianceMatrix.Should().BeEquivalentTo(referenceResult.ParameterCovarianceMatrix.MultipliedBy(cost.ErrorDefinition), 
            options => options.WithRelativeDoubleTolerance(0.001));
    }

    [Test]
    public async Task when_minimized_but_minimization_is_cancelled_during_the_process_yields_a_result_with_manually_stopped_exit_condition(
        [Values] bool hasYErrors, [Values] bool hasGradient, [Values] Strategy strategy)
    {
        var resetEvent = new ManualResetEvent(false);
        var cost = CubicPolynomial.LeastSquaresCost
            .WithYErrors(hasYErrors)
            .WithGradient(hasGradient)
            .Build()
            .ListeningToResetEvent(resetEvent);
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var minimizerConfiguration = new MigradMinimizerConfiguration(strategy);

        var cts = new CancellationTokenSource();
        var task = Task.Run(() => MigradMinimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration, cts.Token), CancellationToken.None);
        await cts.CancelAsync();
        resetEvent.Set();

        var result = await task;
        result.Should().HaveExitCondition(ManuallyStopped);
    }

    [Test]
    public void when_minimized_with_a_function_call_limit_lower_than_the_number_of_required_calls_yields_a_result_with_calls_exhausted_exit_condition()
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var minimizerConfiguration = new MigradMinimizerConfiguration(MaximumFunctionCalls: 1);

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations, minimizerConfiguration);

        result.Should().HaveExitCondition(FunctionCallsExhausted);
    }

    [TestCase(false, 100),
     TestCase(true, 78),
     Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario.")]
    public void when_minimized_yields_the_expected_result(bool hasGradient, int expectedFunctionCalls)
    {
        var cost = CubicPolynomial.LeastSquaresCost.WithGradient(hasGradient).Build();

        var result = MigradMinimizer.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(4).And
            .HaveNumberOfFunctionCallsCloseTo(expectedFunctionCalls).And
            .HaveCostValue(12.49).And
            .HaveParameters(["c0", "c1", "c2", "c3"]).And
            .HaveParameterValues([9.974, -1.959, 0.9898, -0.09931]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 0.005623, -0.004301, 0.000881, -5.271e-05 },
                { -0.004301, 0.004923, -0.001177, 7.655e-05 },
                { 0.000881, -0.001177, 0.0003037, -2.067e-05 },
                { -5.271e-05, 7.655e-05, -2.067e-05, 1.45e-06 }
            });
    }

    [TestCase(false, 31),
     TestCase(true, 31),
     Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario.")]
    public void when_minimized_with_fixed_parameters_yields_the_expected_result(bool hasGradient, int expectedFunctionCalls)
    {
        var cost = CubicPolynomial.LeastSquaresCost.WithGradient(hasGradient).Build();
        ParameterConfiguration[] parameterConfigurations =
        [
            CubicPolynomial.ParameterConfigurations.C0,
            CubicPolynomial.ParameterConfigurations.C1.Fixed(),
            CubicPolynomial.ParameterConfigurations.C2,
            CubicPolynomial.ParameterConfigurations.C3.Fixed()
        ];

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(2).And
            .HaveNumberOfFunctionCallsCloseTo(expectedFunctionCalls).And
            .HaveCostValue(437.7).And
            .HaveParameters(["c0", "c1", "c2", "c3"]).And
            .HaveParameterValues([9.411, -1.97, 1.088, -0.11]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 0.001092, 0.0, -1.918e-05, 0.0 },
                { 0.0, 0.0, 0.0, 0.0 },
                { -1.918e-05, 0.0, 6.211e-07, 0.0 },
                { 0.0, 0.0, 0.0, 0.0 }
            });
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
    public void when_minimized_with_limited_parameters_and_optimal_values_are_located_outside_the_limits_yields_a_result_with_affected_parameters_at_their_next_best_limit(
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

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.ParameterValues.First().Should().BeApproximately(expectedValue, expectedValue * 0.001);
    }

    [Test, Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario.")]
    public void when_minimized_with_limited_parameters_yields_the_expected_result()
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        ParameterConfiguration[] parameterConfigurations =
        [
            CubicPolynomial.ParameterConfigurations.C0.WithLimits(CubicPolynomial.ParameterConfigurations.C0.Value - 0.25, null),
            CubicPolynomial.ParameterConfigurations.C1,
            CubicPolynomial.ParameterConfigurations.C2,
            CubicPolynomial.ParameterConfigurations.C3.WithLimits(null, CubicPolynomial.ParameterConfigurations.C3.Value + 0.005)
        ];

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(4).And
            .HaveNumberOfFunctionCallsCloseTo(444).And
            .HaveCostValue(62.34).And
            .HaveParameters(["c0", "c1", "c2", "c3"]).And
            .HaveParameterValues([10.5, -2.39, 1.082, -0.105]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 7.023e-09, -3.654e-09, 3.124e-10, -1.261e-14 },
                { -3.654e-09, 0.0002602, -3.344e-05, 5.468e-09 },
                { 3.124e-10, -3.344e-05, 4.594e-06, -1.873e-09 },
                { -1.261e-14, 5.468e-09, -1.873e-09, 1.211e-10 }
            }, relativeTolerance: 0.003);
    }

    [Test, Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario; " +
                       "That the resulting parameter covariances are different from the numerical-gradient case (above) " +
                       "is due to terminating at the parameter limits; Covariances are not robust/trustworthy in this situation.")]
    public void with_an_analytical_gradient_when_minimized_with_limited_parameters_yields_the_expected_result()
    {
        var cost = CubicPolynomial.LeastSquaresCost.WithGradient().Build();
        ParameterConfiguration[] parameterConfigurations =
        [
            CubicPolynomial.ParameterConfigurations.C0.WithLimits(CubicPolynomial.ParameterConfigurations.C0.Value - 0.25, null),
            CubicPolynomial.ParameterConfigurations.C1,
            CubicPolynomial.ParameterConfigurations.C2,
            CubicPolynomial.ParameterConfigurations.C3.WithLimits(null, CubicPolynomial.ParameterConfigurations.C3.Value + 0.005)
        ];

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(4).And
            .HaveNumberOfFunctionCallsCloseTo(156).And
            .HaveCostValue(62.34).And
            .HaveParameters(["c0", "c1", "c2", "c3"]).And
            .HaveParameterValues([10.5, -2.39, 1.082, -0.105]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 5.482e-09, -2.933e-09, 2.507e-10, -7.976e-15 },
                { -2.933e-09, 0.0002602, -3.343e-05, 4.309e-09 },
                { 2.507e-10, -3.343e-05, 4.589e-06, -1.476e-09 },
                { -7.976e-15, 4.309e-09, -1.476e-09, 9.18e-11 }
            });
    }

    [Test, Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario.")]
    public void with_missing_data_uncertainties_when_minimized_yields_the_expected_result([Values] bool hasGradient)
    {
        var cost = CubicPolynomial.LeastSquaresCost.WithMissingYErrors().WithGradient(hasGradient).Build();

        var result = MigradMinimizer.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(4).And
            .HaveCostValue(0.1249).And
            .HaveParameters(["c0", "c1", "c2", "c3"]).And
            .HaveParameterValues([9.974, -1.959, 0.9898, -0.09931]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 0.004391, -0.003358, 0.0006878, -4.115e-05 },
                { -0.003358, 0.003843, -0.0009193, 5.977e-05 },
                { 0.0006878, -0.0009193, 0.0002371, -1.614e-05 },
                { -4.115e-05, 5.977e-05, -1.614e-05, 1.132e-06 }
            });
    }

    [Test]
    public void when_minimized_forwards_exceptions_thrown_by_the_model_function()
    {
        var cost = CostFunction.LeastSquares([0], [0], [], ModelFunctionThrowing<TestException>());
        Action action = () => MigradMinimizer.Minimize(cost, []);
        action.Should().ThrowExactly<TestException>();
    }

    private static Func<double, IList<double>, double> ModelFunctionThrowing<T>() where T : Exception, new()
        => (_, _) => throw new T();

    private class TestException : Exception;
}
