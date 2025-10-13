using minuit2.net.CostFunctions;

namespace minuit2.net;

internal class AbortedMinimizationResult(
    MinimizationAbort abort,
    ICostFunction costFunction,
    IReadOnlyList<string> variables,
    int numberOfFunctionCallsCarryOver = 0)
    : IMinimizationResult
{
    public double CostValue { get; } = CostValueFrom(costFunction, abort.ParameterValues);
    public IReadOnlyList<string> Parameters { get; } = costFunction.Parameters;
    public IReadOnlyList<string> Variables { get; } = variables;
    public IReadOnlyList<double> ParameterValues { get; } = abort.ParameterValues;
    public double[,]? ParameterCovarianceMatrix => null;

    // Meta information
    public bool IsValid => false;
    public int NumberOfVariables { get; } = variables.Count;
    public int NumberOfFunctionCalls { get; } = abort.NumberOfFunctionCalls + numberOfFunctionCallsCarryOver;
    public MinimizationExitCondition ExitCondition { get; } = abort.ExitCondition;

    private static double CostValueFrom(ICostFunction costFunction, IReadOnlyList<double> parameterValues) =>
        costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);
}