namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithBatchEvaluationModel(
    IReadOnlyList<double> x,
    IReadOnlyList<double> y,
    Func<int, double> yErrorForIndex,
    IReadOnlyList<string> parameters,
    Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
    double errorDefinitionInSigma,
    bool isErrorDefinitionRecalculationEnabled,
    double errorDefinitionScaling = 1)
    : LeastSquaresBase(
        x.Count,
        parameters,
        false,
        false,
        ErrorDefinitionFor(errorDefinitionInSigma, errorDefinitionScaling))
{
    public override double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        var yModel = model(x, parameterValues);
        for (var i = 0; i < x.Count; i++)
        {
            var residual = (y[i] - yModel[i]) / yErrorForIndex(i);
            sum += residual * residual;
        }

        return sum;
    }

    public override IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues) =>
        throw new NotImplementedException();

    public override IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues) =>
        throw new NotImplementedException();

    public override IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues) =>
        throw new NotImplementedException();

    protected override ICostFunction CopyWith(double errorDefinitionScaling)
    {
        if (!isErrorDefinitionRecalculationEnabled) return this;

        return new LeastSquaresWithBatchEvaluationModel(
            x,
            y,
            yErrorForIndex,
            Parameters,
            model,
            errorDefinitionInSigma,
            isErrorDefinitionRecalculationEnabled,
            errorDefinitionScaling);
    }
}