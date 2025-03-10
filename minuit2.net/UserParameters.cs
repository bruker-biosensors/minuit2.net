namespace minuit2.net;

public record Parameter(
    string Name,
    double Value,
    bool IsFixed = false,
    double? LowerLimit = null,
    double? UpperLimit = null);

public class UserParameters(params Parameter[] parameters)
{
    internal MnUserParameterState AsState()
    {
        var states = new MnUserParameterState();
        foreach (var parameter in parameters)
        {
            states.Add(parameter.Name, parameter.Value, parameter.Value * 0.01);
            if (parameter.IsFixed) states.Fix(parameter.Name);
            if (parameter.LowerLimit is { } lowerLimit) states.SetLowerLimit(parameter.Name, lowerLimit);
            if (parameter.UpperLimit is { } upperLimit) states.SetUpperLimit(parameter.Name, upperLimit);
        }
        return states;
    }

    internal bool AreNotMatching(IList<string> parameterNames)
    {
        if (parameters.Length != parameterNames.Count) return true;
        if (parameterNames.Any(IsNotPresent)) return true;

        return false;
    }

    private bool IsNotPresent(string parameterName) => parameters.All(p => p.Name != parameterName);

    internal UserParameters OrderedBy(IList<string> parameterNames) =>
        new(parameters.OrderBy(p => parameterNames.IndexOf(p.Name)).ToArray());
}
