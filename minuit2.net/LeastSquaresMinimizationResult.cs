namespace minuit2.net;

public class LeastSquaresMinimizationResult : MinimizationResult
{
    internal LeastSquaresMinimizationResult(FunctionMinimum functionMinimum, ILeastSquares costFunction)
        : base(functionMinimum, costFunction)
    {
        var numberOfData = costFunction.NumberOfData;
        var degreesOfFreedom = numberOfData - NumberOfVariables;
        var reducedChiSquared = CostValue / degreesOfFreedom;

        if (costFunction.ShouldScaleCovariances)
            // Auto-scale the covariances to match the values that would be obtained when data uncertainties were
            // chosen such that the reduced chi-squared becomes 1. This is the default behaviour in lmfit.
            // source: https://lmfit.github.io/lmfit-py/fitting.html#uncertainties-in-variable-parameters-and-their-correlations
            ParameterCovarianceMatrix = CovarianceMatrixFrom(functionMinimum.UserState(), reducedChiSquared);
    }
}