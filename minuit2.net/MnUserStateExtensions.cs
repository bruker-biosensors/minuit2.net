namespace minuit2.net;

internal static class MnUserStateExtensions
{
    public static IReadOnlyCollection<string> ExtractVariablesFrom(
        this MnUserParameterState state, 
        IReadOnlyCollection<string> parameters)
    {
        var numberOfVariables = (int)state.VariableParameters();
        return Enumerable.Range(0, numberOfVariables).Select(VariableName).ToArray();

        string VariableName(int variableIndex)
        {
            var parameterIndex = state.ParameterIndexOf(variableIndex);
            return parameters.ElementAt(parameterIndex);
        }
    }

    public static int ParameterIndexOf(this MnUserParameterState state, int variableIndex) =>
        (int)state.ExtOfInt((uint)variableIndex);
}