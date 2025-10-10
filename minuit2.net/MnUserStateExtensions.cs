namespace minuit2.net;

internal static class MnUserStateExtensions
{
    public static IReadOnlyList<string> ExtractVariablesFrom(
        this MnUserParameterState state, 
        IReadOnlyList<string> parameters)
    {
        var numberOfVariables = (int)state.VariableParameters();
        return Enumerable.Range(0, numberOfVariables).Select(VariableName).ToArray();

        string VariableName(int variableIndex) => parameters[state.ParameterIndexOf(variableIndex)];
    }

    public static int ParameterIndexOf(this MnUserParameterState state, int variableIndex) =>
        (int)state.ExtOfInt((uint)variableIndex);
}