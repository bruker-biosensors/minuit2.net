namespace minuit2.net.Exceptions;

internal interface IPrematureMinimizationExit
{
    MinimizationExitCondition ExitCondition { get; }
    IReadOnlyCollection<double> LastParameterValues { get; }
}