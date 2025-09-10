namespace minuit2.UnitTests.TestUtilities;

internal static class ParameterCovarianceMatrixExtensions
{
    public static double[,] MultipliedBy(this double[,] matrix, double factor)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var multipliedMatrix = new double[rows, cols];
        for (var i = 0; i < rows; i++)
        for (var j = 0; j < cols; j++)
            multipliedMatrix[i, j] = matrix[i, j] * factor;
        return multipliedMatrix;
    }

    public static double[,] BlockConcat(this double[,] matrix, double[,] otherMatrix)
    {
        var combinedMatrix = new double[
            matrix.GetLength(0) + otherMatrix.GetLength(0),
            matrix.GetLength(1) + otherMatrix.GetLength(1)];

        for (var i = 0; i < combinedMatrix.GetLength(0); i++)
        for (var j = 0; j < combinedMatrix.GetLength(1); j++)
        {
            if (i < matrix.GetLength(0) && j < matrix.GetLength(1))
                combinedMatrix[i, j] = matrix[i, j];
            if (i >= matrix.GetLength(0) && j >= matrix.GetLength(1))
                combinedMatrix[i, j] = otherMatrix[i - matrix.GetLength(0), j - matrix.GetLength(1)];
        }

        return combinedMatrix;
    }
}