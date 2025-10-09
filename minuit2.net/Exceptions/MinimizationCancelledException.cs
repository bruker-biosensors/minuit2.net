namespace minuit2.net.Exceptions;

internal class MinimizationCancelledException(IEnumerable<double> parameterValues)
    : OperationCanceledException, IPrematureMinimizationExit
{
    public MinimizationExitCondition ExitCondition => MinimizationExitCondition.ManuallyStopped;
    public IReadOnlyCollection<double> LastParameterValues { get; } = parameterValues.ToArray();
}