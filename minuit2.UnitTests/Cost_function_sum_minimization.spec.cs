using FluentAssertions;
using FluentAssertions.Execution;
using minuit2.net;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.UnitTests;

public class A_cost_function_sum
{
    [Test, Description("Ensures that the inner scaling of gradients by the error definition and the final rescaling works.")]
    public void with_a_single_component_when_minimized_yields_a_result_equivalent_to_the_result_for_the_isolated_component(
        [Values] bool hasGradient, [Values] Strategy strategy)
    {
        var component = CubicPolynomial.LeastSquaresCost.WithGradient(hasGradient).Build().WithErrorDefinition(4);
        var sum = new CostFunctionSum(component);

        var minimizerConfiguration = new MigradMinimizerConfiguration(strategy);
        var componentResult = MigradMinimizer.Minimize(component, CubicPolynomial.ParameterConfigurations.Defaults, minimizerConfiguration);
        var sumResult = MigradMinimizer.Minimize(sum, CubicPolynomial.ParameterConfigurations.Defaults, minimizerConfiguration);
        
        sumResult.Should().BeEquivalentTo(componentResult, options => options
            .Excluding(x => x.NumberOfFunctionCalls)
            .WithRelativeDoubleTolerance(0.001));
    }
    
    [Test, Description("Ensures that scaling and rescaling by the error definition works on a per-cost basis.")]
    public void of_independent_components_with_different_error_definitions_when_minimized_yields_a_result_equivalent_to_the_results_for_the_isolated_components(
        [Values] bool hasGradient, [Values] Strategy strategy)
    {
        if (strategy == Strategy.Fast)
            Assert.Ignore("The fast minimization strategy currently leads to inconsistent covariances. " +
                          "In iminuit, this is resolved by calling the Hesse algorithm after minimization. " +
                          "Once the additional Hesse call is added here, the skipped tests could be re-enabled.");
        
        var component1 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(1).WithGradient(hasGradient).Build();
        var component2 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(2).WithGradient(hasGradient).Build().WithErrorDefinition(4);
        var sum = new CostFunctionSum(component1, component2);

        var parameterConfigurations1 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(1);
        var parameterConfigurations2 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(2);

        var minimizerConfiguration = new MigradMinimizerConfiguration(strategy);
        var component1Result = MigradMinimizer.Minimize(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = MigradMinimizer.Minimize(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = MigradMinimizer.Minimize(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        using (new AssertionScope())
        {
            sumResult.Should()
                .HaveCostValue(component1Result.CostValue + component2Result.CostValue).And
                .HaveParameterValues(component1Result.ParameterValues.Concat(component2Result.ParameterValues).ToArray());

            const double relativeToleranceForNonZeros = 0.001;
            const double absoluteToleranceForZeros = 1e-8;
            sumResult.ParameterCovarianceMatrix.SubMatrix(0,3,0,3).Should().BeEquivalentTo(component1Result.ParameterCovarianceMatrix, options => options.WithRelativeDoubleTolerance(relativeToleranceForNonZeros));
            sumResult.ParameterCovarianceMatrix.SubMatrix(4,7,4,7).Should().BeEquivalentTo(component2Result.ParameterCovarianceMatrix, options => options.WithRelativeDoubleTolerance(relativeToleranceForNonZeros));
            sumResult.ParameterCovarianceMatrix.SubMatrix(4,7,0,3).Should().BeEquivalentTo(AllZeroMatrix(4,4), options => options.WithDoubleTolerance(absoluteToleranceForZeros));
            sumResult.ParameterCovarianceMatrix.SubMatrix(0,3,4,7).Should().BeEquivalentTo(AllZeroMatrix(4,4), options => options.WithDoubleTolerance(absoluteToleranceForZeros));
        }
    }
    
    [Test, Description("Ensures that auto-scaling of the error definition for cost functions with missing y-errors " +
                       "and, hence, parameter covariances works (on a per-cost basis).")]
    public void of_independent_components_with_some_components_missing_data_uncertainties_when_minimized_yields_a_result_equivalent_to_the_results_for_the_isolated_components(
        [Values] bool hasGradient, [Values] Strategy strategy)
    {
        if (strategy == Strategy.Fast)
            Assert.Ignore("The fast minimization strategy currently leads to inconsistent cost values, parameter values and covariances. " +
                          "This might be solved by using a lower tolerance for the minimizer and/or by calling the Hesse algorithm after minimization. " +
                          "This should be investigated, and if solvable should probably asserted in a separate test.");
        
        var component1 = CubicPolynomial.LeastSquaresCost.WithMissingYErrors().WithParameterSuffix(1).WithGradient(hasGradient).Build();
        var component2 = CubicPolynomial.LeastSquaresCost.WithParameterSuffix(2).WithGradient(hasGradient).Build();
        var sum = new CostFunctionSum(component1, component2);

        var parameterConfigurations1 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(1);
        var parameterConfigurations2 = CubicPolynomial.ParameterConfigurations.DefaultsWithSuffix(2);

        var minimizerConfiguration = new MigradMinimizerConfiguration(strategy);
        var component1Result = MigradMinimizer.Minimize(component1, parameterConfigurations1, minimizerConfiguration);
        var component2Result = MigradMinimizer.Minimize(component2, parameterConfigurations2, minimizerConfiguration);
        var sumResult = MigradMinimizer.Minimize(sum, parameterConfigurations1.Concat(parameterConfigurations2).ToArray(), minimizerConfiguration);

        using (new AssertionScope())
        { 
            sumResult.Should()
                .HaveCostValue(component1Result.CostValue + component2Result.CostValue).And
                .HaveParameterValues(component1Result.ParameterValues.Concat(component2Result.ParameterValues).ToArray());
            
            const double relativeToleranceForNonZeros = 0.002;
            const double absoluteToleranceForZeros = 1e-8;
            sumResult.ParameterCovarianceMatrix.SubMatrix(0,3,0,3).Should().BeEquivalentTo(component1Result.ParameterCovarianceMatrix, options => options.WithRelativeDoubleTolerance(relativeToleranceForNonZeros));
            sumResult.ParameterCovarianceMatrix.SubMatrix(4,7,4,7).Should().BeEquivalentTo(component2Result.ParameterCovarianceMatrix, options => options.WithRelativeDoubleTolerance(relativeToleranceForNonZeros));
            sumResult.ParameterCovarianceMatrix.SubMatrix(4,7,0,3).Should().BeEquivalentTo(AllZeroMatrix(4,4), options => options.WithDoubleTolerance(absoluteToleranceForZeros));
            sumResult.ParameterCovarianceMatrix.SubMatrix(0,3,4,7).Should().BeEquivalentTo(AllZeroMatrix(4,4), options => options.WithDoubleTolerance(absoluteToleranceForZeros));
        }
    }
    
    private static double[,] AllZeroMatrix(int rows, int columns) => new double[rows, columns];

    [TestCase(false, false, 182),
     TestCase(true, false, 182),
     TestCase(true, true, 120),
     Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario.")]
    public void with_all_components_having_defined_data_uncertainties_when_minimized_yields_the_expected_result(
        bool hasFirstGradient, bool hasLastGradient, int expectedFunctionCalls)
    {
        var cost = new CostFunctionSum(
            CubicPolynomial.LeastSquaresCost.WithGradient(hasFirstGradient).Build(),
            CubicPolynomial.LeastSquaresCost.WithParameterNames(c1: "c1_1", c3: "c3_1").WithGradient(hasLastGradient).Build());

        var parameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults
            .Concat([new ParameterConfiguration("c1_1", -2.1), new ParameterConfiguration("c3_1", -0.15)]).ToArray();

        var result = MigradMinimizer.Minimize(cost, parameterConfigurations);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(6).And
            .HaveNumberOfFunctionCallsCloseTo(expectedFunctionCalls).And
            .HaveCostValue(24.99).And
            .HaveParameters(["c0", "c1", "c2", "c3", "c1_1", "c3_1"]).And
            .HaveParameterValues([9.974, -1.959, 0.9898, -0.09931, -1.959, -0.09931]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 0.002811, -0.00215, 0.0004404, -2.635e-05, -0.00215, -2.635e-05 },
                { -0.00215, 0.002512, -0.0005887, 3.752e-05, 0.00241, 3.902e-05 },
                { 0.0004404, -0.0005887, 0.0001518, -1.033e-05, -0.0005887, -1.033e-05 },
                { -2.635e-05, 3.752e-05, -1.033e-05, 7.383e-07, 3.902e-05, 7.12e-07 },
                { -0.00215, 0.00241, -0.0005887, 3.902e-05, 0.002512, 3.752e-05 },
                { -2.635e-05, 3.902e-05, -1.033e-05, 7.12e-07, 3.752e-05, 7.384e-07 }
            });
    }
    
    [Test, Description("Expected values are generated by iminuit, the Minuit2 wrapper for Python, for the same scenario; " +
                       "Ensures parameter covariances are auto-scaled only for the (inner) cost functions with missing y-errors.")]
    public void with_some_components_missing_data_uncertainties_when_minimized_yields_the_expected_result(
        [Values] bool hasFirstGradient, [Values] bool hasLastGradient)
    {
        var cost = new CostFunctionSum(
            CubicPolynomial.LeastSquaresCost.WithGradient(hasFirstGradient).Build(),
            CubicPolynomial.LeastSquaresCost.WithMissingYErrors().WithGradient(hasLastGradient).Build());

        var result = MigradMinimizer.Minimize(cost, CubicPolynomial.ParameterConfigurations.Defaults);

        result.Should()
            .HaveExitCondition(Converged).And
            .HaveIsValid(true).And
            .HaveNumberOfVariables(4).And
            .HaveCostValue(12.62).And
            .HaveParameters(["c0", "c1", "c2", "c3"]).And
            .HaveParameterValues([9.974, -1.959, 0.9898, -0.09931]).And
            .HaveParameterCovarianceMatrix(new[,]
            {
                { 0.002465, -0.001886, 0.0003862, -2.311e-05 },
                { -0.001886, 0.002158, -0.0005162, 3.356e-05 },
                { 0.0003862, -0.0005162, 0.0001331, -9.062e-06 },
                { -2.311e-05, 3.356e-05, -9.062e-06, 6.359e-07 }
            });
    }
}