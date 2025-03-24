namespace minuit2.net;

public record ParameterConfiguration(
    string Name,
    double Value,
    bool IsFixed = false,
    double? LowerLimit = null,
    double? UpperLimit = null);

internal static class ParameterConfigurationExtensions
{
    public static MnUserParameterState AsState(this IEnumerable<ParameterConfiguration> parameterConfigurations)
    {
        var states = new MnUserParameterState();
        foreach (var parameter in parameterConfigurations)
        {
            states.Add(parameter.Name, parameter.Value, parameter.Value * 0.01);
            if (parameter.IsFixed) states.Fix(parameter.Name);
            if (parameter.LowerLimit is { } lowerLimit and > double.NegativeInfinity) states.SetLowerLimit(parameter.Name, lowerLimit);
            if (parameter.UpperLimit is { } upperLimit and < double.PositiveInfinity) states.SetUpperLimit(parameter.Name, upperLimit);
        }
        return states;
    }
    
    public static bool AreNotMatching(this IReadOnlyCollection<ParameterConfiguration> parameterConfigurations, 
        IList<string> parameterNames)
    {
        if (parameterConfigurations.Count != parameterNames.Count) return true;
        return !parameterConfigurations.All(p => parameterNames.Contains(p.Name));
    }

    public static IEnumerable<ParameterConfiguration> OrderedBy(this IEnumerable<ParameterConfiguration> parameterConfigurations, 
        IList<string> parameterNames)
    {
        return parameterConfigurations.OrderBy(p => parameterNames.IndexOf(p.Name));
    }
}