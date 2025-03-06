namespace minuit2.net;

public class MigradResult
{
    internal MigradResult(FunctionMinimum functionMinimum)
    {
        var state = functionMinimum.UserState();
        BestValues = state.Params().ToList();
        CovarianceMatrix = CovarianceMatrixFrom(state);

        // Meta information about the result
        IsValid = functionMinimum.IsValid();
        NumberOfFunctionCalls = functionMinimum.NFcn();
        HasReachedFunctionCallLimit = functionMinimum.HasReachedCallLimit();
        HasConverged = !functionMinimum.IsAboveMaxEdm();
    }
    
    public IReadOnlyCollection<double> BestValues { get; }
    public double[,] CovarianceMatrix { get; }
    
    // IsValid is "true" when the minimizer did find a minimum without running into troubles.
    // Reasons for an invalid result ("false") are 
    // - the number of allowed function calls has been exhausted
    // - the minimizer could not improve the values of the parameters (and knowing that it has not converged yet)
    // - a problem with the calculation of the covariance matrix
    // source: https://root.cern.ch/doc/master/Minuit2Page.html
    public bool IsValid { get; }
    public int NumberOfFunctionCalls { get; }
    public bool HasReachedFunctionCallLimit { get; }
    
    // The minimizer is deemed to have converged when the expected vertical distance to the minimum (EDM) falls below a
    // threshold value (computed as = 0.001 * tolerance * up).
    public bool HasConverged { get; }

    private static double[,] CovarianceMatrixFrom(MnUserParameterState state)
    { 
        var numberOfParameters = state.Params().Count;
        var covarianceMatrix = new double[numberOfParameters, numberOfParameters];

        var numberOfVariables = (int)state.VariableParameters();
        var covariances = state.Covariance().Data();

        var indexMap = Enumerable.Range(0, numberOfVariables)
            .ToDictionary(ParameterIndex, variableIndex => variableIndex);

        for (var i = 0; i < numberOfParameters; i++)
        {
            if (!indexMap.TryGetValue(i, out var rowVariableIndex)) continue;
            for (var j = 0; j < numberOfParameters; j++)
            {
                if (!indexMap.TryGetValue(j, out var columnVariableIndex)) continue;
                var flatIndex = FlatIndex(rowVariableIndex, columnVariableIndex);
                covarianceMatrix[i, j] = covariances[flatIndex];
                covarianceMatrix[j, i] = covariances[flatIndex];
            }
        }

        return covarianceMatrix;

        int ParameterIndex(int variableIndex) => (int)state.ExtOfInt((uint)variableIndex);
        int FlatIndex(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }
}
