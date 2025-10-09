namespace minuit2.net.Exceptions;

internal class NonFiniteCostValueException(IEnumerable<double> parameterValues)
    : NotFiniteNumberException, IPrematureMinimizationExit
{
    public IReadOnlyCollection<double> LastParameterValues { get; } = parameterValues.ToArray();
}