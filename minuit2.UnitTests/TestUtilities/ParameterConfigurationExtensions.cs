using minuit2.net;

namespace minuit2.UnitTests.TestUtilities;

internal static class ParameterConfigurationExtensions
{
    public static ParameterConfiguration WithSuffix(this ParameterConfiguration parameter, string suffix)
    {
        var name = $"{parameter.Name}_{suffix}";
        return parameter.IsFixed
            ? ParameterConfiguration.Fixed(name, parameter.Value)
            : ParameterConfiguration.Variable(name, parameter.Value, parameter.LowerLimit, parameter.UpperLimit);
    }

    public static ParameterConfiguration WithValue(this ParameterConfiguration parameter, double value) =>
        parameter.IsFixed
            ? ParameterConfiguration.Fixed(parameter.Name, value)
            : ParameterConfiguration.Variable(parameter.Name, value, parameter.LowerLimit, parameter.UpperLimit);
    
    public static ParameterConfiguration WithLimits(this ParameterConfiguration parameter, double? lowerLimit, double? upperLimit) =>
        parameter.IsFixed
            ? ParameterConfiguration.Fixed(parameter.Name, parameter.Value)
            : ParameterConfiguration.Variable(parameter.Name, parameter.Value, lowerLimit, upperLimit);

    public static ParameterConfiguration Fixed(this ParameterConfiguration parameter) =>
        ParameterConfiguration.Fixed(parameter.Name, parameter.Value);
    
    public static ParameterConfiguration[] WithLimits(
        this IEnumerable<ParameterConfiguration> parameters, 
        double? lowerLimit, 
        double? upperLimit)
    {
        return parameters.Select(p => p.WithLimits(lowerLimit, upperLimit)).ToArray();
    }
}