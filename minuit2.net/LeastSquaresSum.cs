namespace minuit2.net;

public class LeastSquaresSum : CostFunctionSum, ILeastSquares
{
    public LeastSquaresSum(params ILeastSquares[] components) : base(components.Cast<ICostFunction>().ToArray())
    {
        NumberOfData = components.Sum(c => c.NumberOfData);
        ShouldScaleCovariances = components.Any(c => c.ShouldScaleCovariances);
    }
    
    public int NumberOfData { get; }
    public bool ShouldScaleCovariances { get; }
}