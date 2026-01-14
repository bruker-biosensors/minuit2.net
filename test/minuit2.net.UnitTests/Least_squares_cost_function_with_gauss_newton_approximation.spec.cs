using AwesomeAssertions;
using ConstrainedNonDeterminism;
using minuit2.net.CostFunctions;
using minuit2.net.UnitTests.TestUtilities;
using NSubstitute;
using static minuit2.net.CostFunctions.CostFunction;

namespace minuit2.net.UnitTests;

[TestFixture]
public class A_least_squares_cost_function_with_gauss_newton_approximation
{
    private static int AnyCount(int min = 10, int max = 100) => Any.Integer().Between(min, max);
    private static List<double> AnyValues(int count) => Enumerable.Range(0, count).Select(_ => (double)Any.Double()).ToList();
    
    private static double TestModel(double x, IReadOnlyList<double> p) => p[0] * x + p[1] * p[1] * x;
    private static IReadOnlyList<double> TestModelGradient(double x, IReadOnlyList<double> p) => [x, 2 * p[1] * x];
    
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
            _validCost = CostFunctionWithUniformYError();
        }

        private ICostFunction CostFunctionWithUniformYError(
            IReadOnlyList<double>? yValuesOverride = null, 
            double? errorDefinitionInSigmaOverride = null)
        {
            var yValues = yValuesOverride ?? _yValues;
            return errorDefinitionInSigmaOverride is { } e
                ? LeastSquaresWithGaussNewtonApproximation(_xValues, yValues, _yError, _parameters, TestModel, TestModelGradient, e)
                : LeastSquaresWithGaussNewtonApproximation(_xValues, yValues, _yError, _parameters, TestModel, TestModelGradient);
        }

        private double Residual(int i) => (_yValues[i] - TestModel(_xValues[i], _parameterValues)) / _yError;

        [Test]
        public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var mismatchingYCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yValues = AnyValues(mismatchingYCount);

            Action action = () => _ = CostFunctionWithUniformYError(yValues);

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
            var cost = CostFunctionWithUniformYError(errorDefinitionInSigmaOverride: errorDefinitionInSigma);
            
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
            double expectedValue = 0;
            for (var i = 0; i < _valueCount; i++) 
                expectedValue += Residual(i) * Residual(i);
            
            _validCost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
        
        [Test]
        public void when_asked_for_its_gradient_returns_the_expected_vector()
        {
            var expectedGradient = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                expectedGradient[0] -= 2 * Residual(i) / _yError * _xValues[i];
                expectedGradient[1] -= 2 * Residual(i) / _yError * 2 * _parameterValues[1] * _xValues[i];
            }

            _validCost.GradientFor(_parameterValues).Should().BeEquivalentTo(expectedGradient,
                options => options.WithSmartDoubleTolerance(0.001));
        }

        [Test]
        public void when_asked_for_its_hessian_returns_the_expected_flat_matrix()
        {
            var expectedHessian = new double[4];
            for (var i = 0; i < _valueCount; i++)
            {
                var g = TestModelGradient(_xValues[i], _parameterValues);
                expectedHessian[0] += 2 * g[0] * g[0] / (_yError * _yError);
                expectedHessian[1] += 2 * g[0] * g[1] / (_yError * _yError);
                expectedHessian[2] += 2 * g[1] * g[0] / (_yError * _yError);
                expectedHessian[3] += 2 * g[1] * g[1] / (_yError * _yError);
            }

            _validCost.HessianFor(_parameterValues).Should().BeEquivalentTo(expectedHessian,
                options => options.WithSmartDoubleTolerance(0.001));
        }

        [Test]
        public void when_asked_for_its_hessian_diagonal_returns_the_expected_vector()
        {
            var expectedHessianDiagonal = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                var g = TestModelGradient(_xValues[i], _parameterValues);
                expectedHessianDiagonal[0] += 2 * g[0] * g[0] / (_yError * _yError);
                expectedHessianDiagonal[1] += 2 * g[1] * g[1] / (_yError * _yError);
            }

            _validCost.HessianDiagonalFor(_parameterValues).Should().BeEquivalentTo(expectedHessianDiagonal,
                options => options.WithSmartDoubleTolerance(0.001));
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
            _validCost = CostFunctionWithIndividualYErrors();
        }

        private ICostFunction CostFunctionWithIndividualYErrors(
            IReadOnlyList<double>? yValuesOverride = null,
            IReadOnlyList<double>? yErrorsOverride = null, 
            double? errorDefinitionInSigmaOverride = null)
        {
            var yValues = yValuesOverride ?? _yValues;
            var yErrors = yErrorsOverride ?? _yErrors;
            return errorDefinitionInSigmaOverride is { } e
                ? LeastSquaresWithGaussNewtonApproximation(_xValues, yValues, yErrors, _parameters, TestModel, TestModelGradient, e)
                : LeastSquaresWithGaussNewtonApproximation(_xValues, yValues, yErrors, _parameters, TestModel, TestModelGradient);
        }

        private double Residual(int i) => (_yValues[i] - TestModel(_xValues[i], _parameterValues)) / _yErrors[i];

        [Test]
        public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var yCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yValues = AnyValues(yCount);

            Action action = () => _ = CostFunctionWithIndividualYErrors(yValues);

            action.Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void when_constructed_with_y_errors_mismatching_the_number_of_y_values_throws_an_exception(
            [Values(-1, 1)] int countBiasDirection)
        {
            var yErrorsCount = _valueCount + countBiasDirection * AnyCount(1, 10);
            var yErrors = AnyValues(yErrorsCount);
            
            Action action = () => _ = CostFunctionWithIndividualYErrors(yErrorsOverride: yErrors);

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
            var cost = CostFunctionWithIndividualYErrors(errorDefinitionInSigmaOverride: errorDefinitionInSigma);
            
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
            double expectedValue = 0;
            for (var i = 0; i < _valueCount; i++)
                expectedValue += Residual(i) * Residual(i);

            _validCost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }

        [Test]
        public void when_asked_for_its_gradient_returns_the_expected_vector()
        {
            var expectedGradient = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                expectedGradient[0] -= 2 * Residual(i) / _yErrors[i] * _xValues[i];
                expectedGradient[1] -= 2 * Residual(i) / _yErrors[i] * 2 * _parameterValues[1] * _xValues[i];
            }

            _validCost.GradientFor(_parameterValues).Should().BeEquivalentTo(expectedGradient,
                options => options.WithSmartDoubleTolerance(0.001));
        }

        [Test]
        public void when_asked_for_its_hessian_returns_the_expected_flat_matrix()
        {
            var expectedHessian = new double[4];
            for (var i = 0; i < _valueCount; i++)
            {
                var g = TestModelGradient(_xValues[i], _parameterValues);
                expectedHessian[0] += 2 * g[0] * g[0] / (_yErrors[i] * _yErrors[i]);
                expectedHessian[1] += 2 * g[0] * g[1] / (_yErrors[i] * _yErrors[i]);
                expectedHessian[2] += 2 * g[1] * g[0] / (_yErrors[i] * _yErrors[i]);
                expectedHessian[3] += 2 * g[1] * g[1] / (_yErrors[i] * _yErrors[i]);
            }

            _validCost.HessianFor(_parameterValues).Should().BeEquivalentTo(expectedHessian,
                options => options.WithSmartDoubleTolerance(0.001));
        }

        [Test]
        public void when_asked_for_its_hessian_diagonal_returns_the_expected_vector()
        {
            var expectedHessianDiagonal = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                var g = TestModelGradient(_xValues[i], _parameterValues);
                expectedHessianDiagonal[0] += 2 * g[0] * g[0] / (_yErrors[i] * _yErrors[i]);
                expectedHessianDiagonal[1] += 2 * g[1] * g[1] / (_yErrors[i] * _yErrors[i]);
            }

            _validCost.HessianDiagonalFor(_parameterValues).Should().BeEquivalentTo(expectedHessianDiagonal,
                options => options.WithSmartDoubleTolerance(0.001));
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
                ? LeastSquaresWithGaussNewtonApproximation(_xValues, yValues, _parameters, TestModel, TestModelGradient, e)
                : LeastSquaresWithGaussNewtonApproximation(_xValues, yValues, _parameters, TestModel, TestModelGradient);
        }

        private double Residual(int i) => _yValues[i] - TestModel(_xValues[i], _parameterValues);

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
            double expectedValue = 0;
            for (var i = 0; i < _valueCount; i++)
                expectedValue += Residual(i) * Residual(i);

            _validCost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }

        [Test]
        public void when_asked_for_its_gradient_returns_the_expected_vector()
        {
            var expectedGradient = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                expectedGradient[0] -= 2 * Residual(i) * _xValues[i];
                expectedGradient[1] -= 2 * Residual(i) * 2 * _parameterValues[1] * _xValues[i];
            }

            _validCost.GradientFor(_parameterValues).Should().BeEquivalentTo(expectedGradient,
                options => options.WithSmartDoubleTolerance(0.001));
        }

        [Test]
        public void when_asked_for_its_hessian_returns_the_expected_flat_matrix()
        {
            var expectedHessian = new double[4];
            for (var i = 0; i < _valueCount; i++)
            {
                var g = TestModelGradient(_xValues[i], _parameterValues);
                expectedHessian[0] += 2 * g[0] * g[0];
                expectedHessian[1] += 2 * g[0] * g[1];
                expectedHessian[2] += 2 * g[1] * g[0];
                expectedHessian[3] += 2 * g[1] * g[1];
            }

            _validCost.HessianFor(_parameterValues).Should().BeEquivalentTo(expectedHessian,
                options => options.WithSmartDoubleTolerance(0.001));
        }

        [Test]
        public void when_asked_for_its_hessian_diagonal_returns_the_expected_vector()
        {
            var expectedHessianDiagonal = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                var g = TestModelGradient(_xValues[i], _parameterValues);
                expectedHessianDiagonal[0] += 2 * g[0] * g[0];
                expectedHessianDiagonal[1] += 2 * g[1] * g[1];
            }

            _validCost.HessianDiagonalFor(_parameterValues).Should().BeEquivalentTo(expectedHessianDiagonal,
                options => options.WithSmartDoubleTolerance(0.001));
        }
    }
}