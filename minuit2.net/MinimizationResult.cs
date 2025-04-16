namespace minuit2.net;

public class MinimizationResult
{
    internal MinimizationResult(FunctionMinimum functionMinimum, IList<string> parameters)
    {
        CostValue = functionMinimum.Fval();
        
        var state = functionMinimum.UserState();
        Parameters = parameters.ToList();
        ParameterValues = state.Params().ToList();
        ParameterCovarianceMatrix = CovarianceMatrixFrom(state);
        
        // Meta information
        IsValid = functionMinimum.IsValid();
        NumberOfVariables = (int)state.VariableParameters();
        NumberOfFunctionCalls = functionMinimum.NFcn();
        HasReachedFunctionCallLimit = functionMinimum.HasReachedCallLimit();
        HasConverged = !functionMinimum.IsAboveMaxEdm();
    }
    
    public double CostValue { get; private set; }

    public IReadOnlyCollection<string> Parameters { get; }
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
            .ToDictionary(ParameterIndex, variableIndex => variableIndex);
        
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

        int ParameterIndex(int variableIndex) => (int)state.ExtOfInt((uint)variableIndex);
        int FlatIndex(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }

    internal MinimizationResult WithParameterCovariancesScaledBy(double scaleFactor)
    {
        for (var i = 0; i < ParameterCovarianceMatrix.GetLength(0); i++)
        for (var j = 0; j < ParameterCovarianceMatrix.GetLength(1); j++)
            ParameterCovarianceMatrix[i, j] *= scaleFactor;
        return this;
    }

    internal MinimizationResult WithCostValue(double value)
    {
        CostValue = value;
        return this;
    }
}
