using minuit2.net;

namespace ExampleProblems;

internal static class ParameterConfigurationExtensions
{
    public static ParameterConfiguration Fixed(this ParameterConfiguration parameter) =>
        parameter.CopyWith(isFixed: true);

    public static ParameterConfiguration WithSuffix(this ParameterConfiguration parameter, string suffix) =>
        parameter.CopyWith(name: $"{parameter.Name}_{suffix}");

    public static ParameterConfiguration WithValue(this ParameterConfiguration parameter, double value) =>
        parameter.CopyWith(value: value);
    
    public static ParameterConfiguration WithLimits(this ParameterConfiguration parameter, double? lowerLimit, double? upperLimit) =>
        parameter.CopyWith(lowerLimit: lowerLimit, upperLimit: upperLimit);
    
    public static ParameterConfiguration[] WithLimits(
        this ParameterConfiguration[] parameters, 
        double? lowerLimit, 
        double? upperLimit)
    {
        return parameters.Select(p => p.WithLimits(lowerLimit, upperLimit)).ToArray();
    }

    private static ParameterConfiguration CopyWith(
        this ParameterConfiguration parameter,
        Override<bool>? isFixed = null,
        Override<string>? name = null,
        Override<double>? value = null,
        Override<double?>? lowerLimit = null,
        Override<double?>? upperLimit = null)
    {
        var newIsFixed = Resolve(isFixed, parameter.IsFixed);
        var newName = Resolve(name, parameter.Name);
        var newValue = Resolve(value, parameter.Value);
        var newLowerLimit = Resolve(lowerLimit, parameter.LowerLimit);
        var newUpperLimit = Resolve(upperLimit, parameter.UpperLimit);
        
        return newIsFixed
            ? ParameterConfiguration.Fixed(newName, newValue)
            : ParameterConfiguration.Variable(newName, newValue, newLowerLimit, newUpperLimit);

        T Resolve<T>(Override<T>? @override, T old) => @override is not null ? @override.Value : old;
    }
    
    private record Override<T>(T Value)
    {
        public static implicit operator Override<T>(T value) => new(value);
    }
}