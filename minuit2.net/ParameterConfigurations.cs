namespace minuit2.net;

public record ParameterConfiguration
{
    private readonly double _value;
    private readonly double? _lowerLimit;
    private readonly double? _upperLimit;

    public ParameterConfiguration(
        string Name,
        double Value,
        bool IsFixed = false,
        double? LowerLimit = null,
        double? UpperLimit = null)
    {
        this.Name = Name;
        this.Value = Value;
        this.IsFixed = IsFixed;
        this.LowerLimit = LowerLimit;
        this.UpperLimit = UpperLimit;
    }

    public string Name { get; init; }

    public double Value
    {
        get => _value;
        init
        {
            _value = value;
            ValidateValueComplianceWithLimits();
        }
    }

    public bool IsFixed { get; init; }

    public double? LowerLimit
    {
        get => _lowerLimit;
        init
        {
            _lowerLimit = value;
            ValidateValueComplianceWithLimits();
        }
    }

    public double? UpperLimit
    {
        get => _upperLimit;
        init
        {
            _upperLimit = value;
            ValidateValueComplianceWithLimits();
        }
    }
    
    public bool HasLowerLimit => _lowerLimit is > double.NegativeInfinity;

    public bool HasUpperLimit => _upperLimit is < double.PositiveInfinity;
    
    private void ValidateValueComplianceWithLimits()
    {
        if (_lowerLimit is { } lower && lower >= _value)
            throw new InvalidParameterConfiguration($"Lower limit for '{Name}' must be smaller than its value, but {lower} >= {_value}");
        
        if (_upperLimit is { } upper && upper <= _value)
            throw new InvalidParameterConfiguration($"Upper limit for '{Name}' must be greater than its value, but {upper} <= {_value}");
    }
}

internal static class ParameterConfigurationExtensions
{
    public static MnUserParameterState AsState(this IEnumerable<ParameterConfiguration> parameterConfigurations)
    {
        var state = new MnUserParameterState();
        foreach (var parameter in parameterConfigurations)
        {
            state.Add(parameter.Name, parameter.Value, parameter.Value * 0.01);
            
            if (parameter.IsFixed) state.Fix(parameter.Name);
            
            if(parameter.HasLowerLimit && parameter.HasUpperLimit) 
                state.SetLimits(parameter.Name, parameter.LowerLimit!.Value, parameter.UpperLimit!.Value);
            else if (parameter.HasLowerLimit) 
                state.SetLowerLimit(parameter.Name, parameter.LowerLimit!.Value);
            else if (parameter.HasUpperLimit) 
                state.SetUpperLimit(parameter.Name, parameter.UpperLimit!.Value);
        }
        
        return state;
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