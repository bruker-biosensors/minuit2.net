namespace minuit2.net;

internal class MinimizationAbort(MinimizationExitCondition exitCondition, IEnumerable<double> parameterValues) 
    : Exception
{
    public MinimizationExitCondition ExitCondition { get; } = exitCondition;
    public IReadOnlyCollection<double> ParameterValues { get; } = parameterValues.ToArray();
}