namespace minuit2.net.CostFunctions;

internal interface ICostFunctionMonitor
{
    IReadOnlyCollection<double>? NonFiniteValueParametersValues { get; }
    IReadOnlyCollection<double>? NonFiniteGradientParameterValues { get; }
}