namespace minuit2.net.CostFunctions;

internal class LeastSquares(
    IReadOnlyList<double> x,
    IReadOnlyList<double> y,
    Func<int, double> yErrorForIndex,
    IReadOnlyList<string> parameters,
    Func<double, IReadOnlyList<double>, double> model,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessian,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessianDiagonal,
    double errorDefinitionInSigma,
    bool isErrorDefinitionRecalculationEnabled,
    double errorDefinitionScaling = 1)
    : LeastSquaresBase(
        x.Count,
        parameters,
        modelGradient != null,
        modelHessian != null,
        modelHessianDiagonal != null,
        ErrorDefinitionFor(errorDefinitionInSigma, errorDefinitionScaling))
{
    public override double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double value = 0;
        for (var i = 0; i < x.Count; i++)
        {
            var r = ResidualFor(parameterValues, i);
            value += r * r;
        }

        return value;
    }

    public override IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradient = new double[Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var r = ResidualFor(parameterValues, i);
            var g = modelGradient!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
                gradient[j] -= 2 * r * g[j] / yError;
        }

        return gradient;
    }

    public override IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues)
    {
        var hessian = new double[Parameters.Count * Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var r = ResidualFor(parameterValues, i);
            var g = modelGradient!(x[i], parameterValues);
            var h = modelHessian!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
            for (var k = 0; k < Parameters.Count; k++)
            {
                var jk = j * Parameters.Count + k;
                hessian[jk] -= 2 / yError * (r * h[jk] - g[j] * g[k] / yError);
            }
        } 
        
        return hessian;
    }

    public override IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues)
    {
        return modelHessianDiagonal == null
            ? HessianDiagonalFromModelHessianFor(parameterValues) 
            : HessianDiagonalFromModelHessianDiagonalFor(parameterValues);
    }
    
    private double[] HessianDiagonalFromModelHessianFor(IReadOnlyList<double> parameterValues)
    {
        var hessianDiagonal = new double[Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var r = ResidualFor(parameterValues, i);
            var g = modelGradient!(x[i], parameterValues);
            var h = modelHessian!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
            {
                var jj = j * (Parameters.Count + 1);
                hessianDiagonal[j] -= 2 / yError * (r * h[jj] - g[j] * g[j] / yError);
            }
        }

        return hessianDiagonal;
    }

    private double[] HessianDiagonalFromModelHessianDiagonalFor(IReadOnlyList<double> parameterValues)
    {
        var hessianDiagonal = new double[Parameters.Count];
        for (var i = 0; i < x.Count; i++)
        {
            var yError = yErrorForIndex(i);
            var r = ResidualFor(parameterValues, i);
            var g = modelGradient!(x[i], parameterValues);
            var h = modelHessianDiagonal!(x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++)
                hessianDiagonal[j] -= 2 / yError * (r * h[j] - g[j] * g[j] / yError);
        }

        return hessianDiagonal;
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
            modelHessianDiagonal,
            errorDefinitionInSigma,
            isErrorDefinitionRecalculationEnabled,
            errorDefinitionScaling);
    }
}