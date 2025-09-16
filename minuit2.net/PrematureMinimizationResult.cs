using minuit2.net.CostFunctions;

namespace minuit2.net;

internal class PrematureMinimizationResult : IMinimizationResult
{
    public PrematureMinimizationResult(
        MinimizationExitCondition exitCondition,
        ICostFunction costFunction,
        ICostFunctionMonitor costFunctionMonitor,
        MnUserParameterState parameterState, 
        int numberOfFunctionCallsCarryOver = 0)
    {
        var parametersValues = costFunctionMonitor.LastValidParameterValues.ToArray();
        CostValue = costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parametersValues)
            : costFunction.ValueFor(parametersValues);
        Parameters = costFunction.Parameters.ToArray();
        Variables = parameterState.ExtractVariablesFrom(Parameters);
        ParameterValues = parametersValues;
        ParameterCovarianceMatrix = new double[,] { };
        
        // Meta information
        IsValid = false;
        NumberOfVariables = Variables.Count;
        NumberOfFunctionCalls = costFunctionMonitor.NumberOfValidFunctionCalls + numberOfFunctionCallsCarryOver;
        ExitCondition = exitCondition;
        FaultParameterValues = costFunctionMonitor.LastParameterValues.ToArray();
    }

    public double CostValue { get; }
    public IReadOnlyCollection<string> Parameters { get; }
    public IReadOnlyCollection<string> Variables { get; }
    public IReadOnlyCollection<double> ParameterValues { get; }
    public double[,] ParameterCovarianceMatrix { get; }
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }
    public IReadOnlyCollection<double>? FaultParameterValues { get; }
}