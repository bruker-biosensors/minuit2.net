using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net;

internal class StoppedMinimizationResult : IMinimizationResult
{
    public double CostValue => 0;
    public IReadOnlyCollection<string> Parameters => [];
    public IReadOnlyCollection<string> Variables => [];
    public IReadOnlyCollection<double> ParameterValues => [];
    public double[,] ParameterCovarianceMatrix => new double[,] { };
    public bool IsValid => false;
    public int NumberOfVariables => 0;
    public int NumberOfFunctionCalls => 0;
    public MinimizationExitCondition ExitCondition => ManuallyStopped;
}