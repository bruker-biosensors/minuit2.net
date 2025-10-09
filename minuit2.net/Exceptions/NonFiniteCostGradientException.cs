namespace minuit2.net.Exceptions;

internal class NonFiniteCostGradientException(IEnumerable<double> parameterValues)
    : NotFiniteNumberException, IPrematureMinimizationExit
{
    public IReadOnlyCollection<double> LastParameterValues { get; } = parameterValues.ToArray();
}