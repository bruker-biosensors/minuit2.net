namespace minuit2.net;

public class MigradResult
{
    internal MigradResult(FunctionMinimum functionMinimum)
    {
        BestValues = BestValuesFrom(functionMinimum);
        CovarianceMatrix = CovarianceMatrixFrom(functionMinimum);
    }

    public IReadOnlyCollection<double> BestValues { get; }
    public double[,] CovarianceMatrix { get; }

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
