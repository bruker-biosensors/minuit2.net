namespace minuit2.net.Exceptions;

internal interface IPrematureMinimizationExit
{
    IReadOnlyCollection<double> LastParameterValues { get; }
}