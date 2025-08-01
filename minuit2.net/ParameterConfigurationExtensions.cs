namespace minuit2.net;

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