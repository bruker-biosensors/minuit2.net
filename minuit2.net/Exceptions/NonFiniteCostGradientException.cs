namespace minuit2.net.Exceptions;

internal class NonFiniteCostGradientException(IEnumerable<double> parameterValues)
    : NotFiniteNumberException, IPrematureMinimizationExit
{
    public MinimizationExitCondition ExitCondition => MinimizationExitCondition.NonFiniteGradient;
    public IReadOnlyCollection<double> LastParameterValues { get; } = parameterValues.ToArray();
}