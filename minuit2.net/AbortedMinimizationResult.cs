using minuit2.net.CostFunctions;

namespace minuit2.net;

internal class AbortedMinimizationResult : IMinimizationResult
{
    public AbortedMinimizationResult(
        MinimizationAbort abort,
        ICostFunction costFunction,
        MnUserParameterState parameterState,
        int numberOfFunctionCallsCarryOver = 0)
    {
        CostValue = CostValueFrom(costFunction, abort.ParameterValues);
        Parameters = costFunction.Parameters.ToArray();
        Variables = parameterState.ExtractVariablesFrom(Parameters);
        ParameterValues = abort.ParameterValues;
        ParameterCovarianceMatrix = null;
        
        // Meta information
        IsValid = false;
        NumberOfVariables = Variables.Count;
        NumberOfFunctionCalls = abort.NumberOfFunctionCalls + numberOfFunctionCallsCarryOver;
        ExitCondition = abort.ExitCondition;
    }
    
    public double CostValue { get; }
    public IReadOnlyList<string> Parameters { get; }
    public IReadOnlyList<string> Variables { get; }
    public IReadOnlyList<double> ParameterValues { get; }
    public double[,]? ParameterCovarianceMatrix { get; }
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }

    private static double CostValueFrom(ICostFunction costFunction, IReadOnlyList<double> parameterValues) =>
        costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);
}