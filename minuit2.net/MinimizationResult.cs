using minuit2.net.CostFunctions;
using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net;

internal class MinimizationResult : IMinimizationResult
{
    internal MinimizationResult(FunctionMinimum minimum, ICostFunction costFunction)
    {
        var state = minimum.UserState();
        Parameters = costFunction.Parameters;
        Variables = state.ExtractVariablesFrom(Parameters);
        var parameterValues = state.Params().ToArray();
        ParameterValues = parameterValues;
        ParameterCovarianceMatrix = CovarianceMatrixFrom(state);

        CostValue = costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);

        // Meta information
        IsValid = minimum.IsValid();
        NumberOfVariables = (int)state.VariableParameters();
        NumberOfFunctionCalls = minimum.NFcn();
        ExitCondition = ExitConditionFrom(minimum);
        Minimum = minimum;
    }
    
    public double CostValue { get; }
    
    public IReadOnlyList<string> Parameters { get; }
    public IReadOnlyList<string> Variables { get; }
    public IReadOnlyList<double> ParameterValues { get; }
    public double[,]? ParameterCovarianceMatrix { get; }

    // The result is considered valid if the minimizer did not run into any troubles. Reasons for an invalid result are: 
    // - the number of allowed function calls has been exhausted
    // - the minimizer could not improve the values of the parameters (and knowing that it has not converged yet)
    // - a problem with the calculation of the covariance matrix
    // source: https://root.cern.ch/doc/master/Minuit2Page.html
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int? NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }

    private static double[,]? CovarianceMatrixFrom(MnUserParameterState state)
    {
        var covariancesOfVariables = state.Covariance().Data();
        if (covariancesOfVariables.IsEmpty) return null;
        
        var numberOfVariables = (int)state.VariableParameters();
        var indexMap = Enumerable.Range(0, numberOfVariables)
            .ToDictionary(state.ParameterIndexOf, variableIndex => variableIndex);
        
        var numberOfParameters = state.Params().Count;
        var covarianceMatrix = new double[numberOfParameters, numberOfParameters];
        for (var i = 0; i < numberOfParameters; i++)
        {
            if (!indexMap.TryGetValue(i, out var rowVariableIndex)) continue;
            for (var j = 0; j < numberOfParameters; j++)
            {
                if (!indexMap.TryGetValue(j, out var columnVariableIndex)) continue;
                var flatIndex = FlatIndex(rowVariableIndex, columnVariableIndex);
                covarianceMatrix[i, j] = covariancesOfVariables[flatIndex];
                covarianceMatrix[j, i] = covariancesOfVariables[flatIndex];
            }
        }

        return covarianceMatrix;

        int FlatIndex(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }
    
    private static MinimizationExitCondition ExitConditionFrom(FunctionMinimum minimum)
    {
        if (minimum.HasReachedCallLimit())
            return FunctionCallsExhausted;
        if (!minimum.IsAboveMaxEdm())
            return Converged;
        
        return None;
    }

    internal FunctionMinimum Minimum { get; }
}