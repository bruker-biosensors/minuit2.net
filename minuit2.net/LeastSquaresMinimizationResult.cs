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
            ParameterCovarianceMatrix = CovarianceMatrixFrom(functionMinimum.UserState(), reducedChiSquared);
    }
}