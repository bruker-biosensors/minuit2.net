namespace minuit2.net;

public class MinimizationResult
{
    internal MinimizationResult(FunctionMinimum functionMinimum, ICostFunction costFunction)
    {
        FunctionMinimum = functionMinimum;
        
        var state = functionMinimum.UserState();
        Parameters = costFunction.Parameters.ToList();
        var parameterValues = state.Params();
        ParameterValues = parameterValues.ToList();
        ParameterCovarianceMatrix = CovarianceMatrixFrom(state);

        CostValue = costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);

        // Meta information
        IsValid = functionMinimum.IsValid();
        NumberOfVariables = (int)state.VariableParameters();
        NumberOfFunctionCalls = functionMinimum.NFcn();
        HasReachedFunctionCallLimit = functionMinimum.HasReachedCallLimit();
        HasConverged = !functionMinimum.IsAboveMaxEdm();

        Variables = Enumerable.Range(0, NumberOfVariables).Select(var => Parameters.ElementAt(state.ParameterIndexOf(var))).ToList();
    }
    
    public double CostValue { get; private set; }

    public IReadOnlyCollection<string> Parameters { get; }
    public IReadOnlyCollection<string> Variables { get; }
    public IReadOnlyCollection<double> ParameterValues { get; }
    public double[,] ParameterCovarianceMatrix { get; }

    // The result is considered valid if the minimizer did not run into any troubles. Reasons for an invalid result are: 
    // - the number of allowed function calls has been exhausted
    // - the minimizer could not improve the values of the parameters (and knowing that it has not converged yet)
    // - a problem with the calculation of the covariance matrix
    // source: https://root.cern.ch/doc/master/Minuit2Page.html
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int NumberOfFunctionCalls { get; }
    public bool HasReachedFunctionCallLimit { get; }
    
    // The minimizer is deemed to have converged when the expected vertical distance to the minimum (EDM) falls below a
    // threshold value (computed as = 0.001 * tolerance * up).
    public bool HasConverged { get; }

    private static double[,] CovarianceMatrixFrom(MnUserParameterState state)
    {
        var covariance = state.Covariance();
        var covarianceValues = covariance.Data();
        
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
                covarianceMatrix[i, j] = covarianceValues[flatIndex];
                covarianceMatrix[j, i] = covarianceValues[flatIndex];
            }
        }

        return covarianceMatrix;

        int FlatIndex(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }
    
    internal FunctionMinimum FunctionMinimum { get; }
}


file static class UserStateExtensions
{
    public static int ParameterIndexOf(this MnUserParameterState state, int variableIndex) =>
        (int)state.ExtOfInt((uint)variableIndex);
}
