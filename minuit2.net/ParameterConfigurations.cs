namespace minuit2.net;

public class ParameterConfiguration
{
    private ParameterConfiguration(string name, double value, bool isFixed, double? lowerLimit, double? upperLimit)
    {
        Name = name;
        Value = value;
        IsFixed = isFixed;
        LowerLimit = lowerLimit;
        UpperLimit = upperLimit; 
        
        ValidateValueComplianceWithLimits();
    } 
    
    public static ParameterConfiguration Fixed(string name, double value) => 
        new(name, value, true, null, null);
    
    public static ParameterConfiguration Variable(string name, double value, double? lowerLimit = null, double? upperLimit = null) => 
        new(name, value, false, lowerLimit, upperLimit);
    
    public string Name { get; }
    public double Value { get; }
    public bool IsFixed { get; }
    public double? LowerLimit { get; }
    public double? UpperLimit { get; }
    public bool HasLowerLimit => LowerLimit is > double.NegativeInfinity;
    public bool HasUpperLimit => UpperLimit is < double.PositiveInfinity;
    
    private void ValidateValueComplianceWithLimits()
    {
        if (LowerLimit is { } lower && lower >= Value)
            throw new InvalidParameterConfiguration($"Lower limit for '{Name}' must be smaller than its value, but {lower} >= {Value}");
        
        if (UpperLimit is { } upper && upper <= Value)
            throw new InvalidParameterConfiguration($"Upper limit for '{Name}' must be greater than its value, but {upper} <= {Value}");
    }
}

internal static class ParameterConfigurationExtensions
{
    public static MnUserParameterState AsState(this IEnumerable<ParameterConfiguration> parameterConfigurations)
    {
        var state = new MnUserParameterState();
        foreach (var parameter in parameterConfigurations) state.Add(parameter);
        return state;
    }

    private static void Add(this MnUserParameterState state, ParameterConfiguration parameter)
    {
        state.Add(parameter.Name, parameter.Value, parameter.Value * 0.01);

        if (parameter.IsFixed) 
            state.Fix(parameter.Name);

        if (parameter.HasLowerLimit && parameter.HasUpperLimit)
            state.SetLimits(parameter.Name, parameter.LowerLimit!.Value, parameter.UpperLimit!.Value);
        else if (parameter.HasLowerLimit) 
            state.SetLowerLimit(parameter.Name, parameter.LowerLimit!.Value);
        else if (parameter.HasUpperLimit) 
            state.SetUpperLimit(parameter.Name, parameter.UpperLimit!.Value);
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