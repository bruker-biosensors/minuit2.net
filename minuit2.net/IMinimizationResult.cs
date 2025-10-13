namespace minuit2.net;

public interface IMinimizationResult
{
    double CostValue { get; }
    IReadOnlyList<string> Parameters { get; }
    IReadOnlyList<string> Variables { get; }
    IReadOnlyList<double> ParameterValues { get; }
    double[,]? ParameterCovarianceMatrix { get; }
    bool IsValid { get; }
    int NumberOfFunctionCalls { get; }
    MinimizationExitCondition ExitCondition { get; }
}