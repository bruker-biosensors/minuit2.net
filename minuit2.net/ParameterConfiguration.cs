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