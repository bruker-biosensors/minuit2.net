namespace minuit2.net.CostFunctions;

internal interface ICostFunctionMonitor
{
    int NumberOfValidFunctionCalls { get; }
    IReadOnlyCollection<double> LastValidParameterValues { get; }
    IReadOnlyCollection<double> LastParameterValues { get; }
}