using minuit2.net.CostFunctions;

namespace minuit2.net;

internal class PrematureMinimizationResult : IMinimizationResult
{
    public PrematureMinimizationResult(
        MinimizationExitCondition exitCondition,
        ICostFunction costFunction,
        ICostFunctionMonitor costFunctionMonitor,
        MnUserParameterState initialState, 
        int numberOfFunctionCallsCarryOver = 0)
    {
        var parameterValues = ParameterValuesFrom(costFunctionMonitor, initialState);
        CostValue = CostValueFrom(costFunction, parameterValues);
        Parameters = costFunction.Parameters.ToArray();
        Variables = initialState.ExtractVariablesFrom(Parameters);
        ParameterValues = parameterValues;
        ParameterCovarianceMatrix = null;
        
        // Meta information
        IsValid = false;
        NumberOfVariables = Variables.Count;
        NumberOfFunctionCalls = costFunctionMonitor.NumberOfFunctionCalls + numberOfFunctionCallsCarryOver;
        ExitCondition = exitCondition;
        IssueParameterValues = costFunctionMonitor.IssueParameterValues;
    }
    
    public double CostValue { get; }
    public IReadOnlyCollection<string> Parameters { get; }
    public IReadOnlyCollection<string> Variables { get; }
    public IReadOnlyCollection<double> ParameterValues { get; }
    public double[,]? ParameterCovarianceMatrix { get; }
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }
    public IReadOnlyCollection<double>? IssueParameterValues { get; }
    
    private static double[] ParameterValuesFrom(ICostFunctionMonitor monitor, MnUserParameterState initialState) => 
        monitor.BestParameterValues?.ToArray() ?? initialState.Params().ToArray();
    
    private static double CostValueFrom(ICostFunction costFunction, double[] parameterValues) =>
        costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);
}