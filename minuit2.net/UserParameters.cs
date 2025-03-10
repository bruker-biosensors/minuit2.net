namespace minuit2.net;

public record Parameter(string Name, double Value, bool IsFixed = false, double? LowerLimit = null, double? UpperLimit = null);

public class UserParameters(params Parameter[] parameters)
{
    internal MnUserParameterState GetParameterStates()
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
}
