namespace minuit2.net.costFunctions;

internal static class LeastSquares
{
    // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
    // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
    public const double OneSigmaErrorDefinition = 1;
}