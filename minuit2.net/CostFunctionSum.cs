namespace minuit2.net;

internal class CostFunctionSum(ICostFunction left, ICostFunction right) : ICostFunction
{
    private readonly List<int> _rightParameterIndices = RightParameterIndicesFrom(left.Parameters, right.Parameters);

    private static List<int> RightParameterIndicesFrom(IList<string> leftParameters, IList<string> rightParameters)
    {
        var nextFreeIndex = leftParameters.Count;
        return rightParameters.Select(p => leftParameters.Contains(p) ? leftParameters.IndexOf(p) : nextFreeIndex++).ToList();
    }

    public IList<string> Parameters { get; } = left.Parameters.Union(right.Parameters).ToList();

    public double ValueFor(IList<double> parameterValues) => left.ValueFor(Left(parameterValues)) + right.ValueFor(Right(parameterValues));

    private List<double> Left(IList<double> parameterValues) => parameterValues.Take(left.Parameters.Count).ToList();
    
    private List<double> Right(IList<double> parameterValues) => _rightParameterIndices.Select(i => parameterValues[i]).ToList();
}
