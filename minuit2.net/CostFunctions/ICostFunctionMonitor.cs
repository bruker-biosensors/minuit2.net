namespace minuit2.net.CostFunctions;

internal interface ICostFunctionMonitor
{
    IEnumerable<double>? NonFiniteValueParametersValues { get; }
    IEnumerable<double>? NonFiniteGradientParameterValues { get; }
}