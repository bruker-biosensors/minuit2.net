namespace minuit2.net.CostFunctions;

internal interface ICostFunctionMonitor
{
    bool HasFiniteValue { get; }
    bool HasFiniteGradient { get; }
}