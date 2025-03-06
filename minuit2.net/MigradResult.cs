namespace minuit2.net;

public class MigradResult
{
    internal MigradResult(FunctionMinimum functionMinimum)
    {
        BestValues = BestValuesFrom(functionMinimum);
        CovarianceMatrix = CovarianceMatrixFrom(functionMinimum);

        // Meta information about the result
        IsValid = functionMinimum.IsValid();
        HasReachedCallLimit = functionMinimum.HasReachedCallLimit();
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
    public bool HasReachedCallLimit { get; }
    
    private static List<double> BestValuesFrom(FunctionMinimum functionMinimum) =>
        functionMinimum.UserParameters().Params().ToList();

    private static double[,] CovarianceMatrixFrom(FunctionMinimum functionMinimum)
    {
        var numberOfParameters = functionMinimum.UserParameters().Params().Count;
        var covarianceMatrix = new double[numberOfParameters, numberOfParameters];

        var covariancesOfVariables = functionMinimum.UserCovariance().Data() ?? [];
        var numberOfVariables = (int)functionMinimum.UserCovariance().Nrow();
        var indexMap = Enumerable.Range(0, numberOfVariables).ToDictionary(
            variableIndex => (int)functionMinimum.UserState().ExtOfInt((uint)variableIndex), i => i);

        for (var i = 0; i < numberOfParameters; i++)
        {
            if (!indexMap.TryGetValue(i, out var iMapped)) continue;
            for (var j = 0; j < numberOfParameters; j++)
            {
                if (!indexMap.TryGetValue(j, out var jMapped)) continue;
                var flatIndex = FlatIndexFrom(iMapped, jMapped);
                covarianceMatrix[i, j] = covariancesOfVariables[flatIndex];
                covarianceMatrix[j, i] = covariancesOfVariables[flatIndex];
            }
        }

        return covarianceMatrix;

        int FlatIndexFrom(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }
}
