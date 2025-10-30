namespace minuit2.net;

internal class MinimizationAbort(
    MinimizationExitCondition exitCondition,
    IEnumerable<double> parameterValues,
    int numberOfFunctionCalls) 
    : Exception
{
    public MinimizationExitCondition ExitCondition { get; } = exitCondition;
    public IReadOnlyList<double> ParameterValues { get; } = parameterValues.ToArray();
    public int NumberOfFunctionCalls { get; } = numberOfFunctionCalls;
}