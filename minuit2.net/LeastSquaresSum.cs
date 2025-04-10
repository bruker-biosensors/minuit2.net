namespace minuit2.net;

public class LeastSquaresSum(ILeastSquares left, ILeastSquares right) : ILeastSquares
{
    public static LeastSquaresSum Seed => new(LeastSquares.Seed, LeastSquares.Seed);

    private readonly List<int> _rightParameterIndices = RightParameterIndicesFrom(left.Parameters, right.Parameters);

    private static List<int> RightParameterIndicesFrom(IList<string> leftParameters, IList<string> rightParameters)
    {
        var nextFreeIndex = leftParameters.Count;
        return rightParameters.Select(p => leftParameters.Contains(p) ? leftParameters.IndexOf(p) : nextFreeIndex++).ToList();
    }

    public IList<string> Parameters { get; } = left.Parameters.Union(right.Parameters).ToList();
    public int NumberOfData { get; } = left.NumberOfData + right.NumberOfData;
    public bool ShouldScaleCovariances { get; } = left.ShouldScaleCovariances || right.ShouldScaleCovariances;

    public double ValueFor(IList<double> parameterValues) => left.ValueFor(Left(parameterValues)) + right.ValueFor(Right(parameterValues));
    
    public IList<double> GradientFor(IList<double> parameterValues)
    {
        var gradients = left.GradientFor(Left(parameterValues));
        var rightGradients = right.GradientFor(Right(parameterValues));
        foreach (var (g, i) in rightGradients.Zip(_rightParameterIndices))
        {
            if (i < left.Parameters.Count)
                gradients[i] += g;
            else
                gradients.Add(g);
        }
        return gradients;
    }

    public bool HasGradient => left.HasGradient && right.HasGradient;

    public double Up => LeastSquares.ChiSquaredUp;

    private List<double> Left(IList<double> parameterValues) => parameterValues.Take(left.Parameters.Count).ToList();
    
    private List<double> Right(IList<double> parameterValues) => _rightParameterIndices.Select(i => parameterValues[i]).ToList();

    public static LeastSquaresSum operator +(LeastSquaresSum left, ILeastSquares right) => new(left, right);
}
