using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal abstract class ConfigurableLeastSquaresProblem
{
    protected abstract Func<double, IList<double>, double> Model { get; }
    protected abstract Func<double, IList<double>, IList<double>> ModelGradient { get; }
    protected abstract IReadOnlyCollection<string> ParameterNames { get; }
    
    protected abstract IReadOnlyCollection<double> XValues { get; }
    protected abstract IReadOnlyCollection<double> YValues { get; }
    protected abstract double YError { get; }

    public abstract IReadOnlyCollection<double> OptimumParameterValues { get; }
    protected abstract IReadOnlyCollection<double> DefaultInitialParameterValues { get; }
    
    public LeastSquaresCostBuilder Cost => new(XValues, YValues, YError, Model, ModelGradient, ParameterNames);
    
    public class LeastSquaresCostBuilder(
        IReadOnlyCollection<double> xValues, 
        IReadOnlyCollection<double> yValues,
        double yError,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>> modelGradient,
        IReadOnlyCollection<string> parameterNames)
    {
        private readonly IList<double> _xValues = xValues.ToArray();
        private readonly IList<double> _yValues = yValues.ToArray();
        private IList<string> _parameterNames = parameterNames.ToArray();
        
        private bool _hasGradient;
        private double _errorDefinitionInSigma = 1;

        public ICostFunction Build() => _hasGradient 
            ? CostFunction.LeastSquares(_xValues, _yValues, yError, _parameterNames, model, modelGradient, _errorDefinitionInSigma) 
            : CostFunction.LeastSquares(_xValues, _yValues, yError, _parameterNames, model, errorDefinitionInSigma: _errorDefinitionInSigma);

        public LeastSquaresCostBuilder WithGradient(bool hasGradient = true)
        {
            _hasGradient = hasGradient;
            return this;
        }
        
        public LeastSquaresCostBuilder WithParameterSuffix(int suffix)
        {
            _parameterNames = _parameterNames.Select(p => $"{p}_{suffix}").ToArray();
            return this;
        }
        
        public LeastSquaresCostBuilder WithErrorDefinition(double sigma)
        {
            _errorDefinitionInSigma = sigma;
            return this;
        }
    }

    public ParameterConfigurationsBuilder ParameterConfigurations =>
        new(ParameterNames, DefaultInitialParameterValues, OptimumParameterValues);
    
    public class ParameterConfigurationsBuilder(
        IReadOnlyCollection<string> parameterNames, 
        IReadOnlyCollection<double> defaultInitialParameterValues,
        IReadOnlyCollection<double> optimumParameterValues)
    {
        private ParameterConfiguration[] _configs = parameterNames.Zip(defaultInitialParameterValues, 
            (name, value) => ParameterConfiguration.Variable(name, value)).ToArray();
        
        public ParameterConfiguration[] Build() => _configs;
        
        public ParameterConfigurationsBuilder WithSuffix(string suffix)
        {
            _configs = _configs.Select(c => c.WithSuffix(suffix)).ToArray();
            return this;
        }
        
        public ParameterConfigurationsBuilder WithAnyValuesCloseToOptimumValues(double maximumRelativeBias)
        {
            _configs = _configs.Zip(optimumParameterValues, (c, optimumValue) => 
                c.WithValue(AnyValueCloseTo(optimumValue, maximumRelativeBias))).ToArray();
            return this;
        }

        private static double AnyValueCloseTo(double value, double maximumRelativeBias)
        {
            var maximumBias = Math.Abs(value * maximumRelativeBias);
            return Any.Double().Between(value - maximumBias, value + maximumBias);
        }
    }
}