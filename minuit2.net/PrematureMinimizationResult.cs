using minuit2.net.CostFunctions;
using minuit2.net.Exceptions;

namespace minuit2.net;

internal class PrematureMinimizationResult : IMinimizationResult
{
    public PrematureMinimizationResult(
        IPrematureMinimizationExit exit, 
        ICostFunction costFunction, 
        MnUserParameterState parameterState)
    {
        CostValue = CostValueFrom(costFunction, exit.LastParameterValues.ToArray());
        Parameters = costFunction.Parameters.ToArray();
        Variables = parameterState.ExtractVariablesFrom(Parameters);
        ParameterValues = exit.LastParameterValues;
        ParameterCovarianceMatrix = null;
        
        // Meta information
        IsValid = false;
        NumberOfVariables = Variables.Count;
        NumberOfFunctionCalls = null;
        ExitCondition = ExitConditionFor(exit);
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
    
    private static MinimizationExitCondition ExitConditionFor(IPrematureMinimizationExit exit) => exit switch
    {
        MinimizationCancelledException => MinimizationExitCondition.ManuallyStopped,
        NonFiniteCostValueException => MinimizationExitCondition.NonFiniteValue,
        NonFiniteCostGradientException => MinimizationExitCondition.NonFiniteGradient,
        _ => throw new ArgumentOutOfRangeException(nameof(exit), $"No exit condition defined for {exit.GetType().FullName}")
    };
}