namespace minuit2.net.CostFunctions;

internal interface ICostFunctionMonitor
{
    int NumberOfFunctionCalls { get; }
    IReadOnlyCollection<double>? LastParameterValues { get; }
    IReadOnlyCollection<double>? IssueParameterValues { get; }
}