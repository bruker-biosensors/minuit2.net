using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.UnitTests.TestUtilities;
using static minuit2.UnitTests.MinimizationProblems.ConfigurableLeastSquaresProblem.ParameterConfigurationsBuilder;

namespace minuit2.UnitTests.MinimizationProblems;

internal abstract class ConfigurableLeastSquaresProblem
{
    public IReadOnlyList<double> X => XValues;
    public IReadOnlyList<double> Y => YValues;
    public IReadOnlyList<double> Error => Enumerable.Repeat(YError, YValues.Count).ToList();
    protected abstract Func<double, IReadOnlyList<double>, double> Model { get; }
    protected abstract Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient { get; }
    protected abstract IReadOnlyList<string> ParameterNames { get; }
    protected abstract IReadOnlyList<double> XValues { get; }
    protected abstract IReadOnlyList<double> YValues { get; }
    protected abstract double YError { get; }

    protected abstract IReadOnlyList<double> OptimumParameterValues { get; }
    protected abstract IReadOnlyList<double> DefaultInitialParameterValues { get; }

    public LeastSquaresCostBuilder Cost => new(XValues, YValues, YError, Model, ModelGradient, ParameterNames);

    public class LeastSquaresCostBuilder(
        IReadOnlyList<double> xValues,
        IReadOnlyList<double> yValues,
        double yError,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        IReadOnlyList<string> parameterNames)
    {
        private readonly string[] _parameterNames = parameterNames.ToArray();

        private bool _hasYErrors = true;
        private bool _hasGradient;
        private double _errorDefinitionInSigma = 1;

        public ICostFunction Build() => _hasYErrors switch
        {
            true when _hasGradient => CostFunction.LeastSquares(xValues, yValues, yError, _parameterNames, model, modelGradient, _errorDefinitionInSigma),
            true when !_hasGradient => CostFunction.LeastSquares(xValues, yValues, yError, _parameterNames, model, null, _errorDefinitionInSigma),
            false when _hasGradient => CostFunction.LeastSquares(xValues, yValues, _parameterNames, model, modelGradient, _errorDefinitionInSigma),
            _ => CostFunction.LeastSquares(xValues, yValues, _parameterNames, model, null, _errorDefinitionInSigma)
        };

        public LeastSquaresCostBuilder WithUnknownYErrors()
        {
            _hasYErrors = false;
            return this;
        }

        public LeastSquaresCostBuilder WithGradient(bool hasGradient = true)
        {
            _hasGradient = hasGradient;
            return this;
        }

        public LeastSquaresCostBuilder WithErrorDefinition(double sigma)
        {
            _errorDefinitionInSigma = sigma;
            return this;
        }

        public LeastSquaresCostBuilder WithParameterSuffixes(string suffix, IEnumerable<int>? indicesToSuffix = null)
        {
            indicesToSuffix ??= Enumerable.Range(0, _parameterNames.Length);
            foreach (var index in indicesToSuffix)
                _parameterNames[index] += $"_{suffix}";
            return this;
        }
    }

    public ParameterConfigurationsBuilder ParameterConfigurations =>
        new(ParameterNames, DefaultInitialParameterValues, OptimumParameterValues);

    public class ParameterConfigurationsBuilder(
        IReadOnlyList<string> parameterNames,
        IReadOnlyList<double> defaultInitialParameterValues,
        IReadOnlyList<double> optimumParameterValues)
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

        public ParameterConfigurationsBuilder WithLimits(double? lowerLimit, double? upperLimit)
        {
            _configs = _configs.WithLimits(lowerLimit, upperLimit);
            return this;
        }

        public ParameterConfigurationsBuilder InRandomOrder()
        {
            _configs = _configs.InRandomOrder().ToArray();
            return this;
        }

        public ParameterConfigurationsWithSpecialParameterBuilder WithParameter(int index) => new(this, index);

        public class ParameterConfigurationsWithSpecialParameterBuilder(ParameterConfigurationsBuilder outer, int index)
        {
            private ParameterConfiguration Config
            {
                get => outer._configs[index];
                set => outer._configs[index] = value;
            }

            public ParameterConfiguration[] Build() => outer.Build();

            public ParameterConfigurationsBuilder And => outer;

            public ParameterConfigurationsWithSpecialParameterBuilder WithValue(double value)
            {
                Config = Config.WithValue(value);
                return this;
            }

            public ParameterConfigurationsWithSpecialParameterBuilder WithLimits(double? lowerLimit, double? upperLimit)
            {
                Config = Config.WithLimits(lowerLimit, upperLimit);
                return this;
            }

            public ParameterConfigurationsWithSpecialParameterBuilder Fixed()
            {
                Config = Config.Fixed();
                return this;
            }
        }
    }

    internal ConfiguredProblem Configured(
        Func<ParameterConfigurationsBuilder, ParameterConfigurationsWithSpecialParameterBuilder>? customization = null)
    {
        var configBuilder = ParameterConfigurations.WithAnyValuesCloseToOptimumValues(maximumRelativeBias: 0.1);
        if (customization != null) configBuilder = customization(configBuilder).And;

        return new ConfiguredProblem(Cost.Build(), OptimumParameterValues, configBuilder.Build());
    }
}
