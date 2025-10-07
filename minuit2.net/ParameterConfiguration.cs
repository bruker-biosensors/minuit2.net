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
        
        ThrowIfValueViolatesLimits();
        ThrowIfValueProjectionIsNumericallyUnstable();
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
    
    private void ThrowIfValueViolatesLimits()
    {
        var valueMustBe = $"The value ({Value}) must be";

        if (LowerLimit is { } lower && lower >= Value)
            throw new ParameterConfigurationException(Name,
                $"{valueMustBe} greater than the lower limit ({lower}), but " +
                (Value < lower ? "it is smaller." : "both are equal."));
        
        if (UpperLimit is { } upper && upper <= Value)
            throw new ParameterConfigurationException(Name, 
                $"{valueMustBe} smaller than the upper limit ({upper}), but " + 
                (Value > upper ? "it is greater." : "both are equal."));
    }
    
    private void ThrowIfValueProjectionIsNumericallyUnstable()
    {
        // The following constants were empirically tuned to catch parameter projection issues that would otherwise
        // lead to minimization failure, while avoiding false positives in cases where numerical imprecision does not
        // impair the minimization process.
        
        const double valueOffset = 0.1;
        const double relativeProjectionRoundtripBiasTolerance = 0.001;

        double[] projectionTestValues = Value == 0
            ? [-valueOffset, valueOffset]
            : [(1 - valueOffset) * Value, (1 + valueOffset) * Value];

        foreach (var value in projectionTestValues)
        {
            var internalValue = InternalValueFor(value, LowerLimit, UpperLimit);
            var roundTripValue = ExternalValueFor(internalValue, LowerLimit, UpperLimit);
            if (Math.Abs((value - roundTripValue) / value) > relativeProjectionRoundtripBiasTolerance)
                throw new ParameterConfigurationException(Name,
                    $"The combination of value ({Value}) and limits ({LowerLimit}, {UpperLimit}) leads to numerical " +
                    $"instability in the internal parameter projection. Reduce the limits relative to the value.");
        }
    }
    
    private static double InternalValueFor(double externalValue, double? lowerLimit, double? upperLimit)
    {
        // cf. parameter transformation section in the Minuit2 documentation (https://root.cern.ch/doc/master/Minuit2Page.html)
        
        if (lowerLimit.HasValue && upperLimit.HasValue)
            return Math.Asin(2 * (externalValue - lowerLimit.Value) / (upperLimit.Value - lowerLimit.Value) - 1);
        
        if (lowerLimit.HasValue)
            return Math.Sqrt(Math.Pow(externalValue - lowerLimit.Value + 1, 2) - 1);
        
        if (upperLimit.HasValue)
            return Math.Sqrt(Math.Pow(upperLimit.Value - externalValue + 1, 2) - 1);
        
        return externalValue;
    }

    private static double ExternalValueFor(double internalValue, double? lowerLimit, double? upperLimit)
    {
        // cf. parameter transformation section in the Minuit2 documentation (https://root.cern.ch/doc/master/Minuit2Page.html)
        
        if (lowerLimit.HasValue && upperLimit.HasValue)
            return lowerLimit.Value + (upperLimit.Value - lowerLimit.Value) / 2 * (Math.Sin(internalValue) + 1);

        if (lowerLimit.HasValue)
            return lowerLimit.Value - 1 + Math.Sqrt(Math.Pow(internalValue, 2) + 1);

        if (upperLimit.HasValue)
            return upperLimit.Value + 1 - Math.Sqrt(Math.Pow(internalValue, 2) + 1);

        return internalValue;
    }
}