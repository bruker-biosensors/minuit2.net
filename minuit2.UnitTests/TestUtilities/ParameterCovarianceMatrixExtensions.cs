namespace minuit2.UnitTests.TestUtilities;

internal static class ParameterCovarianceMatrixExtensions
{
    public static double[,] MultipliedBy(this double[,] matrix, double factor)
    {
        var rows = matrix.Rows();
        var columns = matrix.Columns();
        var scaledMatrix = new double[rows, columns];
        
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
            scaledMatrix[row, column] = matrix[row, column] * factor;
        
        return scaledMatrix;
    }

    public static double[,] BlockConcat(this double[,] matrix, double[,] otherMatrix)
    {
        var combinedMatrix = new double[
            matrix.Rows() + otherMatrix.Rows(),
            matrix.Columns() + otherMatrix.Columns()];

        for (var row = 0; row < combinedMatrix.Rows(); row++)
        for (var column = 0; column < combinedMatrix.Columns(); column++)
        {
            if (matrix.IncludesBoth(row, column))
                combinedMatrix[row, column] = matrix[row, column];
            if (matrix.ExcludesBoth(row, column))
                combinedMatrix[row, column] = otherMatrix[row - matrix.Rows(), column - matrix.Columns()];
        }

        return combinedMatrix;
    }
    
    private static int Rows(this double[,] matrix) => matrix.GetLength(0);
    
    private static int Columns(this double[,] matrix) => matrix.GetLength(1);

    private static bool IncludesBoth(this double[,] matrix, int row, int column) =>
        row < matrix.GetLength(0) && column < matrix.GetLength(1);

    private static bool ExcludesBoth(this double[,] matrix, int rowIndex, int columnIndex) =>
        rowIndex >= matrix.GetLength(0) && columnIndex >= matrix.GetLength(1);
}