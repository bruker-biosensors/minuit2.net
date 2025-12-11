namespace minuit2.net.CostFunctions;

internal class LeastSquares(
    IReadOnlyList<double> x,
    IReadOnlyList<double> y,
    Func<int, double> yErrorForIndex,
    IReadOnlyList<string> parameters,
    Func<double, IReadOnlyList<double>, double> model,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessian,
    double errorDefinitionInSigma,
    bool isErrorDefinitionRecalculationEnabled,
    double errorDefinitionScaling = 1)
    : LeastSquaresBase(
        x.Count,
        parameters,
        modelGradient != null,
        modelHessian != null,
        ErrorDefinitionFor(errorDefinitionInSigma, errorDefinitionScaling))
{
    public override double ValueFor(IReadOnlyList<double> parameterValues)
    {
        var sum = 0.0;
        for (var i = 0; i < x.Count; i++)
        {
            var residual = ResidualFor(parameterValues, i);
            sum += residual * residual;
        }

        return sum;
    }

    public override IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradientSum = new double[Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var residual = ResidualFor(parameterValues, i);
            var gradient = modelGradient!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
                gradientSum[j] -= 2.0 / yError * residual * gradient[j];
        }

        return gradientSum;
    }

    public override IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues)
    {
        var hessianSum = new double[Parameters.Count * Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var residual = ResidualFor(parameterValues, i);
            var gradient = modelGradient!(x[i], parameterValues);
            var hessian = modelHessian!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
            for (var k = 0; k < Parameters.Count; k++)
            {
                var jk = j * Parameters.Count + k;
                hessianSum[jk] -= 2.0 / yError * (residual * hessian[jk] - gradient[j] * gradient[k] / yError);
            }
        } 
        
        return hessianSum;
    }

    public override IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues)
    {
        var g2Sum = new double[Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var residual = ResidualFor(parameterValues, i);
            var gradient = modelGradient!(x[i], parameterValues);
            var hessian = modelHessian!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
            {
                var jj = j * (Parameters.Count + 1);
                g2Sum[j] -= 2.0 / yError * (residual * hessian[jj] - gradient[j] * gradient[j] / yError);
            }
        }

        return g2Sum;
    }

    private double ResidualFor(IReadOnlyList<double> parameterValues, int index) =>
        (y[index] - model(x[index], parameterValues)) / yErrorForIndex(index);

    protected override ICostFunction CopyWith(double errorDefinitionScaling)
    {
        if (!isErrorDefinitionRecalculationEnabled) return this;

        return new LeastSquares(
            x,
            y,
            yErrorForIndex,
            Parameters,
            model,
            modelGradient,
            modelHessian,
            errorDefinitionInSigma,
            isErrorDefinitionRecalculationEnabled,
            errorDefinitionScaling);
    }
}