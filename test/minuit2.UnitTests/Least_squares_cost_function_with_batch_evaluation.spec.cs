using AwesomeAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.UnitTests.TestUtilities;
using NSubstitute;

namespace minuit2.UnitTests;

[TestFixture]
public class A_least_squares_cost_function_with_batch_evaluation
{
    private static int AnyCount(int min = 10, int max = 100) => Any.Integer().Between(min, max);
    private static List<double> AnyValues(int count) => Enumerable.Range(0, count).Select(_ => (double)Any.Double()).ToList();
    
    private static double[] TestModel(IReadOnlyList<double> x, IReadOnlyList<double> p) =>
        x.Select(xx => p[0] * xx + p[1] * p[1] * xx).ToArray();

    public class With_a_uniform_y_error
    {
        private readonly int _valueCount;
        private readonly List<double> _xValues;
        private readonly List<double> _yValues;
        private readonly double _yError;
        private readonly string[] _parameters;
        private readonly double[] _parameterValues;
        private readonly ICostFunction _validCost;

        public With_a_uniform_y_error()
        {
            _valueCount = AnyCount();
            _xValues = AnyValues(_valueCount);
            _yValues = AnyValues(_valueCount);
            _yError = Any.Double();
            _parameters = ["a", "b"];
            _parameterValues = [Any.Double(), Any.Double()];
            _validCost = CostWithUniformYError();
        }
        
        private ICostFunction CostWithUniformYError(
            IReadOnlyList<double>? yValuesOverride = null,
            double? errorDefinitionInSigmaOverride = null)
        {
            var yValues = yValuesOverride ?? _yValues;
            return errorDefinitionInSigmaOverride is { } e
                ? CostFunction.LeastSquares(_xValues, yValues, _yError, _parameters, TestModel, e)
                : CostFunction.LeastSquares(_xValues, yValues, _yError, _parameters, TestModel);
        }
        
        [Test]
        public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var mismatchingYCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yValues = AnyValues(mismatchingYCount);

            Action action = () => _ = CostWithUniformYError(yValues);

            action.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void has_a_default_error_definition_of_one()
        {
            _validCost.ErrorDefinition.Should().Be(1);
        }
        
        [Test]
        public void with_a_custom_error_definition_in_terms_of_sigma_has_an_error_definition_equal_to_the_square_of_that_value()
        {
            var errorDefinitionInSigma = Any.Double().Between(2, 5);
            var cost = CostWithUniformYError(errorDefinitionInSigmaOverride: errorDefinitionInSigma);
            
            cost.ErrorDefinition.Should().Be(errorDefinitionInSigma * errorDefinitionInSigma);
        }
        
        [Test]
        public void when_asked_for_an_adjusted_version_of_itself_with_recalculated_error_definition_based_on_a_minimization_result_returns_an_unmodified_version_of_itself()
        {
            var result = Any.InstanceOf<IMinimizationResult>();
            result.Parameters.Returns(_parameters);
            result.Variables.Returns(_parameters);
            result.ParameterValues.Returns(AnyValues(2));
            
            var adjustedCost = _validCost.WithErrorDefinitionRecalculatedBasedOnValid(result);
            
            adjustedCost.Should().BeEquivalentTo(_validCost);
        }
        
        [Test]
        public void when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var yModel = TestModel(_xValues, _parameterValues);
            var residuals = _yValues.Select((y, i) => (y - yModel[i]) / _yError);
            var expectedValue = residuals.Sum(r => r * r);
            
            _validCost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
    }

    public class With_individual_y_errors
    {
        private readonly int _valueCount;
        private readonly List<double> _xValues;
        private readonly List<double> _yValues;
        private readonly List<double> _yErrors;
        private readonly string[] _parameters;
        private readonly double[] _parameterValues;
        private readonly ICostFunction _validCost;
        
        public With_individual_y_errors()
        {
            _valueCount = AnyCount();
            _xValues = AnyValues(_valueCount);
            _yValues = AnyValues(_valueCount);
            _yErrors = AnyValues(_valueCount);
            _parameters = ["a", "b"];
            _parameterValues = [Any.Double(), Any.Double()];
            _validCost = CostWithIndividualYErrors();
        }
        
        private ICostFunction CostWithIndividualYErrors(
            IReadOnlyList<double>? yValuesOverride = null,
            IReadOnlyList<double>? yErrorsOverride = null, 
            double? errorDefinitionInSigmaOverride = null)
        {
            var yValues = yValuesOverride ?? _yValues;
            var yErrors = yErrorsOverride ?? _yErrors;
            return errorDefinitionInSigmaOverride is { } e
                ? CostFunction.LeastSquares(_xValues, yValues, yErrors, _parameters, TestModel, e)
                : CostFunction.LeastSquares(_xValues, yValues, yErrors, _parameters, TestModel);
        }
        
        [Test]
        public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var yCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yValues = AnyValues(yCount);

            Action action = () => _ = CostWithIndividualYErrors(yValues);

            action.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void when_constructed_with_y_errors_mismatching_the_number_of_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var yErrorsCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yErrors = AnyValues(yErrorsCount);
            
            Action action = () => _ = CostWithIndividualYErrors(yErrorsOverride: yErrors);

            action.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void has_a_default_error_definition_of_one()
        {
            _validCost.ErrorDefinition.Should().Be(1);
        }
        
        [Test]
        public void with_a_custom_error_definition_in_terms_of_sigma_has_an_error_definition_equal_to_the_square_of_that_value()
        {
            var errorDefinitionInSigma = Any.Double().Between(2, 5);
            var cost = CostWithIndividualYErrors(errorDefinitionInSigmaOverride: errorDefinitionInSigma);
            
            cost.ErrorDefinition.Should().Be(errorDefinitionInSigma * errorDefinitionInSigma);
        }
        
        [Test]
        public void when_asked_for_an_adjusted_version_of_itself_with_recalculated_error_definition_based_on_a_minimization_result_returns_an_unmodified_version_of_itself()
        {
            var result = Any.InstanceOf<IMinimizationResult>();
            result.Parameters.Returns(_parameters);
            result.Variables.Returns(_parameters);
            result.ParameterValues.Returns(AnyValues(2));
            
            var adjustedCost = _validCost.WithErrorDefinitionRecalculatedBasedOnValid(result);
            
            adjustedCost.Should().BeEquivalentTo(_validCost);
        }
        
        [Test]
        public void when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var yModel = TestModel(_xValues, _parameterValues);
            var residuals = Enumerable.Range(0, _valueCount).Select(i => (_yValues[i] - yModel[i]) / _yErrors[i]);
            var expectedValue = residuals.Sum(r => r * r);

            _validCost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
    }

    public class Without_y_errors
    {
        private readonly int _valueCount;
        private readonly List<double> _xValues;
        private readonly List<double> _yValues;
        private readonly string[] _parameters;
        private readonly double[] _parameterValues;
        private readonly ICostFunction _validCost;

        public Without_y_errors()
        {
            _valueCount = AnyCount();
            _xValues = AnyValues(_valueCount);
            _yValues = AnyValues(_valueCount);
            _parameters = ["a", "b"];
            _parameterValues = [Any.Double(), Any.Double()];
            _validCost = CostFunctionWithoutYError();
        }

        private ICostFunction CostFunctionWithoutYError(
            IReadOnlyList<double>? yValuesOverride = null, 
            double? errorDefinitionInSigmaOverride = null)
        {
            var yValues = yValuesOverride ?? _yValues;
            return errorDefinitionInSigmaOverride is { } e
                ? CostFunction.LeastSquares(_xValues, yValues, _parameters, TestModel, e)
                : CostFunction.LeastSquares(_xValues, yValues, _parameters, TestModel);
        }
        
        [Test]
        public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var mismatchingYCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yValues = AnyValues(mismatchingYCount);

            Action action = () => _ = CostFunctionWithoutYError(yValues);

            action.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void has_a_default_error_definition_of_one()
        {
            _validCost.ErrorDefinition.Should().Be(1);
        }
        
        [Test]
        public void with_a_custom_error_definition_in_terms_of_sigma_has_an_error_definition_equal_to_the_square_of_that_value()
        {
            var errorDefinitionInSigma = Any.Double().Between(2, 5);
            var cost = CostFunctionWithoutYError(errorDefinitionInSigmaOverride: errorDefinitionInSigma);
            
            cost.ErrorDefinition.Should().Be(errorDefinitionInSigma * errorDefinitionInSigma);
        }
        
        [Test]
        public void when_asked_for_an_adjusted_version_of_itself_with_recalculated_error_definition_based_on_a_minimization_result_returns_a_version_of_itself_with_the_original_error_definition_scaled_by_the_reduced_chi2_value_of_the_result()
        {
            double[] resultParameterValues = [1, 2];
            var result = Any.InstanceOf<IMinimizationResult>();
            result.Parameters.Returns(_parameters);
            result.Variables.Returns(_parameters);
            result.ParameterValues.Returns(resultParameterValues);
            var degreesOfFreedom = _valueCount - _parameters.Length;
            var reducedChi2 = _validCost.ValueFor(resultParameterValues) / degreesOfFreedom;
            
            var adjustedCost = _validCost.WithErrorDefinitionRecalculatedBasedOnValid(result);
            
            adjustedCost.ErrorDefinition.Should().Be(_validCost.ErrorDefinition * reducedChi2);
        }
        
        [Test]
        public void when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var yModel = TestModel(_xValues, _parameterValues);
            var residuals = _yValues.Select((y, i) => y - yModel[i]);
            var expectedValue = residuals.Sum(r => r * r);

            _validCost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
    }
}