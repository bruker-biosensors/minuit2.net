namespace minuit2.UnitTests.TestUtilities;

internal static class ParameterCovarianceMatrixExtensions
{
    public static double[,] SubMatrix(this double[,] matrix, int firstRow, int lastRow, int firstColumn, int lastColumn)
    {
        var rows = lastRow - firstRow + 1;
        var cols = lastColumn - firstColumn + 1;
        var subMatrix = new double[rows, cols];
        
        for (var i = 0; i < rows; i++)
        for (var j = 0; j < cols; j++)
            subMatrix[i, j] = matrix[firstRow + i, firstColumn + j];

        return subMatrix;
    }
    
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
}