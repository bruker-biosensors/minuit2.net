using AwesomeAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.MinimizationProblems;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

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
                x.IsValid.Should().BeFalse();
                x.ExitCondition.Should().Be(MinimizationExitCondition.ManuallyStopped);
                x.IssueParameterValues.Should().BeNull();
                x.ParameterCovarianceMatrix.Should().BeNull();
                x.NumberOfFunctionCalls.Should().Be(numberOfFunctionCallsBeforeCancellation + _minimizationResult.NumberOfFunctionCalls);
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
                x.IsValid.Should().BeFalse();
                x.ExitCondition.Should().Be(MinimizationExitCondition.NonFiniteValue);
                x.IssueParameterValues.Should().NotBeNull();
                x.ParameterCovarianceMatrix.Should().BeNull();
                x.NumberOfFunctionCalls.Should().Be(numberOfValidFunctionCalls + _minimizationResult.NumberOfFunctionCalls);
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
}