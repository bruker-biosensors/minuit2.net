namespace minuit2.net;

public interface IMinimizationResult
{
    double CostValue { get; }
    IReadOnlyCollection<string> Parameters { get; }
    IReadOnlyCollection<string> Variables { get; }
    IReadOnlyCollection<double> ParameterValues { get; }
    double[,] ParameterCovarianceMatrix { get; }
    bool IsValid { get; }
    int NumberOfVariables { get; }
    int NumberOfFunctionCalls { get; }
    MinimizationExitCondition ExitCondition { get; }
    MinimizationFault? Fault { get; }
}