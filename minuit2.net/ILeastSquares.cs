namespace minuit2.net;

public interface ILeastSquares : ICostFunction
{
    int NumberOfData { get; }
    bool ShouldScaleCovariances { get; }
}