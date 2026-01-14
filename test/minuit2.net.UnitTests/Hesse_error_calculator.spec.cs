using AwesomeAssertions;
using ConstrainedNonDeterminism;
using ExampleProblems;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.net.UnitTests.TestUtilities;

namespace minuit2.net.UnitTests;

public class The_hesse_error_calculator
{
    [Test]
    public void when_asked_to_refine_a_minimization_result_for_an_unrelated_cost_function_throws_an_exception()
    {
        var minimizationResult = Any.InstanceOf<IMinimizationResult>();
        var unrelatedCostFunction = Any.InstanceOf<ICostFunction>();
        
        Action action = () => _ = HesseErrorCalculator.Refine(minimizationResult, unrelatedCostFunction);
        
        action.Should().Throw<ArgumentException>();
    }

    public class When_refining_a_minimization_result_for_a_related_cost_function
    {
        private readonly ICostFunction _costFunction;
        private readonly IMinimizationResult _minimizationResult;

        public When_refining_a_minimization_result_for_a_related_cost_function()
        {
            var problem = new QuadraticPolynomialLeastSquaresProblem();
            _costFunction = problem.Cost.WithGradient().Build();
            var parameterConfigurations = problem.ParameterConfigurations.Build();
            _minimizationResult = Minimizer.Migrad.Minimize(_costFunction, parameterConfigurations);
        }

        [Test]
        public void when_the_process_is_cancelled_yields_an_invalid_result_with_manually_stopped_exit_condition_and_undefined_covariances_representing_the_last_state_of_the_process()
        {
            var cts = new CancellationTokenSource();
            const int numberOfFunctionCallsBeforeCancellation = 10;
            var cost = _costFunction.WithAutoCancellation(cts, numberOfFunctionCallsBeforeCancellation);

            var result = HesseErrorCalculator.Refine(_minimizationResult, cost, cancellationToken: cts.Token);

            result.ShouldFulfill(x =>
            {
                x.Should().HaveNumberOfFunctionCalls(_minimizationResult.NumberOfFunctionCalls + numberOfFunctionCallsBeforeCancellation + 1);
                x.IsValid.Should().BeFalse();
                x.ExitCondition.Should().Be(MinimizationExitCondition.ManuallyStopped);
                x.ParameterCovarianceMatrix.Should().BeNull();
                x.CostValue.Should().BeFinite().And.Be(cost.ValueFor(x.ParameterValues));
            });
        }

        [TestCase(double.NaN)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.PositiveInfinity)]
        public void when_the_cost_function_returns_a_non_finite_value_during_the_process_yields_an_invalid_result_with_non_finite_value_exit_condition_and_undefined_covariances(
            double nonFiniteValue)
        {
            const int numberOfValidFunctionCalls = 5;
            var cost = _costFunction.WithValueOverride(_ => nonFiniteValue, numberOfValidFunctionCalls);
            
            var result = HesseErrorCalculator.Refine(_minimizationResult, cost);

            result.ShouldFulfill(x =>
            {
                x.Should().HaveNumberOfFunctionCalls(_minimizationResult.NumberOfFunctionCalls + numberOfValidFunctionCalls + 1);
                x.IsValid.Should().BeFalse();
                x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteValue);
                x.ParameterCovarianceMatrix.Should().BeNull();
            });
        }

        [Test]
        public void when_the_cost_function_value_calculation_throws_an_exception_during_the_process_forwards_that_exception()
        {
            var cost = _costFunction.WithValueOverride(_ => throw new TestException());
            
            Action action = () => HesseErrorCalculator.Refine(_minimizationResult, cost);
            
            action.Should().ThrowExactly<TestException>();
        }
    }

    [Test, 
     Description("The Hesse algorithm can be accelerated by using the analytical Hessian instead of numerical " +
                 "approximation only if both the cost function and the input minimization result have analytical " +
                 "gradients/hessians (see MnHesse.cxx implementation).")]
    public void When_refining_an_analytical_minimization_result_for_a_related_cost_function_with_an_analytical_hessian_yields_a_result_matching_the_result_obtained_for_numerical_approximation_just_with_fewer_function_calls(
        [Values(1, 2, 3)] double errorDefinition,
        [Values] bool hasReferenceGradient,
        [Values] Strategy strategy)
    {
        var problem = new QuadraticPolynomialLeastSquaresProblem();
        var cost = problem.Cost.WithErrorDefinition(errorDefinition).WithHessian().Build();
        var parameterConfigurations = problem.ParameterConfigurations.Build();
        // minimization result for the (fully) analytical cost function  
        var analyticalMinimizationResult = Minimizer.Migrad.Minimize(cost, parameterConfigurations);
        // reference cost without analytical Hessian
        var referenceCost = problem.Cost.WithErrorDefinition(errorDefinition).WithGradient(hasReferenceGradient).Build();
            
        var result = HesseErrorCalculator.Refine(analyticalMinimizationResult, cost, strategy);            
        var referenceResult = HesseErrorCalculator.Refine(analyticalMinimizationResult, referenceCost, strategy);

        result.Should()
            .MatchExcludingFunctionCalls(referenceResult, options => options.WithSmartDoubleTolerance(0.001)).And
            .HaveFewerFunctionCallsThan(referenceResult);
    }
}