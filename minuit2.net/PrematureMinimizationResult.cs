using minuit2.net.CostFunctions;

namespace minuit2.net;

internal class PrematureMinimizationResult : IMinimizationResult
{
    public PrematureMinimizationResult(
        MinimizationExitCondition exitCondition,
        ICostFunction costFunction,
        MnUserParameterState parameterState, 
        IReadOnlyCollection<double> lastParameterValues)
    {
        CostValue = CostValueFrom(costFunction, lastParameterValues.ToArray());
        Parameters = costFunction.Parameters.ToArray();
        Variables = parameterState.ExtractVariablesFrom(Parameters);
        ParameterValues = lastParameterValues;
        ParameterCovarianceMatrix = null;
        
        // Meta information
        IsValid = false;
        NumberOfVariables = Variables.Count;
        NumberOfFunctionCalls = null;
        ExitCondition = exitCondition;
    }
    
    public double CostValue { get; }
    public IReadOnlyCollection<string> Parameters { get; }
    public IReadOnlyCollection<string> Variables { get; }
    public IReadOnlyCollection<double> ParameterValues { get; }
    public double[,]? ParameterCovarianceMatrix { get; }
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int? NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }

    private static double CostValueFrom(ICostFunction costFunction, IList<double> parameterValues) =>
        costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);
}