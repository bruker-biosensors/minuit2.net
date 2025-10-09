namespace minuit2.net.Exceptions;

internal class MinimizationCancelledException(IEnumerable<double> parameterValues)
    : OperationCanceledException, IPrematureMinimizationExit
{
    public IReadOnlyCollection<double> LastParameterValues { get; } = parameterValues.ToArray();
}