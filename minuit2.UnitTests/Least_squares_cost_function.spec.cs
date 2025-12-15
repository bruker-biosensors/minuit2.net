using AwesomeAssertions;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.CostFunctions.CostFunction;

namespace minuit2.UnitTests;

public class A_least_squares_cost_function
{
    private static int AnyCount(int min = 10, int max = 100) => Any.Integer().Between(min, max);
    private static List<double> AnyValues(int count) => Enumerable.Range(0, count).Select(_ => (double)Any.Double()).ToList();

    private static double TestModel(double x, IReadOnlyList<double> p) => p[0] * x + p[1] * p[1] * x;
    private static IReadOnlyList<double> TestModelGradient(double x, IReadOnlyList<double> p) => [x, 2 * p[1] * x];
    
    [Test]
    public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
        [Values(-1, 1)] int countBiasDirection)
    {
        var xCount = AnyCount(10, 50);
        var yCount = xCount + countBiasDirection * AnyCount(1, 10);
        
        Action action = () => _ = LeastSquares(AnyValues(xCount), AnyValues(yCount), [], (_, _) => 0);
        
        action.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void when_constructed_with_a_collection_of_y_errors_mismatching_the_number_of_y_values_throws_an_exception(
        [Values(-1, 1)] int countBiasDirection)
    {
        var valueCount = AnyCount(10, 50);
        var errorCount = valueCount + countBiasDirection * AnyCount(1, 10);
        
        Action action = () => _ = LeastSquares(AnyValues(valueCount), AnyValues(valueCount), AnyValues(errorCount), [], (_, _) => 0);
        
        action.Should().Throw<ArgumentException>();
    }

    public class With_a_uniform_y_error
    {
        private readonly int _valueCount;
        private readonly List<double> _xValues;
        private readonly List<double> _yValues;
        private readonly double _yError;
        private readonly double[] _parameterValues;

        public With_a_uniform_y_error()
        {
            _valueCount = AnyCount();
            _xValues = AnyValues(_valueCount);
            _yValues = AnyValues(_valueCount);
            _yError = Any.Double();
            _parameterValues = [Any.Double(), Any.Double()];
        }
        
        [Test]
        public void when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var cost = LeastSquares(_xValues, _yValues, _yError, ["a", "b"], TestModel);

            var expectedValue = 0.0;
            for (var i = 0; i < _valueCount; i++)
            {
                var residual = (_yValues[i] - TestModel(_xValues[i], _parameterValues)) / _yError;
                expectedValue += residual * residual;
            }
            
            cost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }

        [Test]
        public void and_a_batch_evaluation_model_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var cost = LeastSquares(_xValues, _yValues, _yError, ["a", "b"], 
                (x, p) => x.Select(xi => TestModel(xi, p)).ToArray());

            var expectedValue = 0.0;
            for (var i = 0; i < _valueCount; i++)
            {
                var residual = (_yValues[i] - TestModel(_xValues[i], _parameterValues)) / _yError;
                expectedValue += residual * residual;
            }
            
            cost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
        
        [Test]
        public void and_an_analytical_model_gradient_when_asked_for_its_gradient_returns_the_expected_vector()
        {
            var cost = LeastSquares(_xValues, _yValues, _yError, ["a", "b"], TestModel, TestModelGradient);

            var expectedGradient = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                var x = _xValues[i];
                var y = _yValues[i];
                var residual = (y - TestModel(x, _parameterValues)) / _yError;
                var factor = -2 * residual / _yError;
                expectedGradient[0] += factor * x;
                expectedGradient[1] += factor * 2 * _parameterValues[1] * x;
            }

            cost.GradientFor(_parameterValues).Should().BeEquivalentTo(expectedGradient, 
                options => options.WithRelativeDoubleTolerance(0.001));
        }
    }

    public class With_individual_y_errors
    {
        private readonly int _valueCount;
        private readonly List<double> _xValues;
        private readonly List<double> _yValues;
        private readonly List<double> _yErrors;
        private readonly double[] _parameterValues;

        public With_individual_y_errors()
        {
            _valueCount = AnyCount();
            _xValues = AnyValues(_valueCount);
            _yValues = AnyValues(_valueCount);
            _yErrors = AnyValues(_valueCount);
            _parameterValues = [Any.Double(), Any.Double()];
        }
        
        [Test]
        public void when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var cost = LeastSquares(_xValues, _yValues, _yErrors, ["a", "b"], TestModel);

            var expectedValue = 0.0;
            for (var i = 0; i < _valueCount; i++)
            {
                var residual = (_yValues[i] - TestModel(_xValues[i], _parameterValues)) / _yErrors[i];
                expectedValue += residual * residual;
            }
            
            cost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }

        [Test]
        public void and_a_batch_evaluation_model_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var cost = LeastSquares(_xValues, _yValues, _yErrors, ["a", "b"], 
                (x, p) => x.Select(xi => TestModel(xi, p)).ToArray());

            var expectedValue = 0.0;
            for (var i = 0; i < _valueCount; i++)
            {
                var residual = (_yValues[i] - TestModel(_xValues[i], _parameterValues)) / _yErrors[i];
                expectedValue += residual * residual;
            }
            
            cost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
        
        [Test]
        public void and_an_analytical_model_gradient_when_asked_for_its_gradient_returns_the_expected_vector()
        {
            var cost = LeastSquares(_xValues, _yValues, _yErrors, ["a", "b"], TestModel, TestModelGradient);

            var expectedGradient = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                var x = _xValues[i];
                var y = _yValues[i];
                var yError = _yErrors[i];
                var residual = (y - TestModel(x, _parameterValues)) / yError;
                var factor = -2 * residual / yError;
                expectedGradient[0] += factor * x;
                expectedGradient[1] += factor * 2 * _parameterValues[1] * x;
            }
            cost.GradientFor(_parameterValues).Should().BeEquivalentTo(expectedGradient, 
                options => options.WithRelativeDoubleTolerance(0.001));
        }
    }

    public class Without_y_errors
    {
        private readonly int _valueCount;
        private readonly List<double> _xValues;
        private readonly List<double> _yValues;
        private readonly double[] _parameterValues;

        public Without_y_errors()
        {
            _valueCount = AnyCount();
            _xValues = AnyValues(_valueCount);
            _yValues = AnyValues(_valueCount);
            _parameterValues = [Any.Double(), Any.Double()];
        }
        
        [Test]
        public void when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var cost = LeastSquares(_xValues, _yValues, ["a", "b"], TestModel);

            var expectedValue = 0.0;
            for (var i = 0; i < _valueCount; i++)
            {
                var residual = _yValues[i] - TestModel(_xValues[i], _parameterValues);
                expectedValue += residual * residual;
            }
            
            cost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }

        [Test]
        public void and_a_batch_evaluation_model_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals()
        {
            var cost = LeastSquares(_xValues, _yValues, ["a", "b"], 
                (x, p) => x.Select(xi => TestModel(xi, p)).ToArray());

            var expectedValue = 0.0;
            for (var i = 0; i < _valueCount; i++)
            {
                var residual = _yValues[i] - TestModel(_xValues[i], _parameterValues);
                expectedValue += residual * residual;
            }
            
            cost.ValueFor(_parameterValues).Should().Be(expectedValue);
        }
        
        [Test]
        public void and_an_analytical_model_gradient_when_asked_for_its_gradient_returns_the_expected_vector()
        { 
            var cost = LeastSquares(_xValues, _yValues, ["a", "b"], TestModel, TestModelGradient);
            
            var expectedGradient = new double[2];
            for (var i = 0; i < _valueCount; i++)
            {
                var x = _xValues[i];
                var y = _yValues[i];
                var residual = y - TestModel(x, _parameterValues);
                var factor = -2 * residual;
                expectedGradient[0] += factor * x;
                expectedGradient[1] += factor * 2 * _parameterValues[1] * x;
            }
            cost.GradientFor(_parameterValues).Should().BeEquivalentTo(expectedGradient, 
                options => options.WithRelativeDoubleTolerance(0.001));
        }
    }
    
    [Test]
    public void has_a_default_error_definition_of_one()
    {
        var cost = LeastSquares(x: AnyValues(10), y: AnyValues(10), parameters: [], model: (_, _) => 0);
        
        cost.ErrorDefinition.Should().Be(1);
    }
    
    [Test]
    public void with_a_custom_error_definition_in_terms_of_sigma_uses_the_square_of_that_value_for_its_absolute_error_definition()
    {
        var errorDefinitionInSigma = Any.Double().Between(2, 5);
        
        var cost = LeastSquares(
            x: AnyValues(10), 
            y: AnyValues(10), 
            parameters: [], 
            model: (_, _) => 0,
            errorDefinitionInSigma: errorDefinitionInSigma);
        
        cost.ErrorDefinition.Should().Be(errorDefinitionInSigma * errorDefinitionInSigma);
    }
}