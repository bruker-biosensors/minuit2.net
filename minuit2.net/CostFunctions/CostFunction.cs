namespace minuit2.net.CostFunctions;

public static class CostFunction
{
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUnknownYError(
            x,
            y,
            parameters,
            model,
            null,
            null,
            null,
            errorDefinitionInSigma);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUnknownYError(
            x,
            y,
            parameters,
            model,
            modelGradient,
            null,
            null,
            errorDefinitionInSigma);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUnknownYError(
            x,
            y,
            parameters,
            model,
            modelGradient,
            modelHessian,
            null,
            errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessianDiagonal,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUnknownYError(
            x,
            y,
            parameters,
            model,
            modelGradient,
            modelHessian,
            modelHessianDiagonal,
            errorDefinitionInSigma);
    }

    private static LeastSquares LeastSquaresWithUnknownYError(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessianDiagonal,
        double errorDefinitionInSigma)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquares(
            x,
            y,
            _ => 1,
            parameters,
            model,
            modelGradient,
            modelHessian,
            modelHessianDiagonal,
            errorDefinitionInSigma,
            true);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUniformYError(
            x,
            y,
            yError,
            parameters,
            model,
            null,
            null,
            null,
            errorDefinitionInSigma);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUniformYError(
            x,
            y,
            yError,
            parameters,
            model,
            modelGradient,
            null,
            null,
            errorDefinitionInSigma);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUniformYError(
            x,
            y,
            yError,
            parameters,
            model,
            modelGradient,
            modelHessian,
            null,
            errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessianDiagonal,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithUniformYError(
            x,
            y,
            yError,
            parameters,
            model,
            modelGradient,
            modelHessian,
            modelHessianDiagonal,
            errorDefinitionInSigma);
    }

    private static LeastSquares LeastSquaresWithUniformYError(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessianDiagonal,
        double errorDefinitionInSigma)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquares(
            x,
            y,
            _ => yError,
            parameters,
            model,
            modelGradient,
            modelHessian,
            modelHessianDiagonal,
            errorDefinitionInSigma,
            false);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithIndividualYErrors(
            x,
            y,
            yError,
            parameters,
            model,
            null,
            null,
            null,
            errorDefinitionInSigma);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithIndividualYErrors(
            x,
            y,
            yError,
            parameters,
            model,
            modelGradient,
            null,
            null,
            errorDefinitionInSigma);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithIndividualYErrors(
            x,
            y,
            yError,
            parameters,
            model,
            modelGradient,
            modelHessian,
            null,
            errorDefinitionInSigma);
    }
    
    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessianDiagonal,
        double errorDefinitionInSigma = 1)
    {
        return LeastSquaresWithIndividualYErrors(
            x,
            y,
            yError,
            parameters,
            model,
            modelGradient,
            modelHessian,
            modelHessianDiagonal,
            errorDefinitionInSigma);
    }

    private static LeastSquares LeastSquaresWithIndividualYErrors(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessianDiagonal,
        double errorDefinitionInSigma)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)), (yError, nameof(yError)));
        return new LeastSquares(
            x,
            y,
            index => yError[index],
            parameters,
            model,
            modelGradient,
            modelHessian,
            modelHessianDiagonal,
            errorDefinitionInSigma,
            false);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma = 1)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquaresWithBatchEvaluationModel(
            x,
            y,
            _ => 1,
            parameters,
            model,
            errorDefinitionInSigma,
            true);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma = 1)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquaresWithBatchEvaluationModel(
            x,
            y,
            _ => yError,
            parameters,
            model,
            errorDefinitionInSigma,
            false);
    }

    public static ICostFunction LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> model,
        double errorDefinitionInSigma = 1)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)), (yError, nameof(yError)));
        return new LeastSquaresWithBatchEvaluationModel(
            x,
            y,
            index => yError[index],
            parameters,
            model,
            errorDefinitionInSigma,
            false);
    }
    
    public static ICostFunction LeastSquaresWithGaussNewtonApproximation(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        double errorDefinitionInSigma = 1)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquaresWithGaussNewtonApproximation(
            x,
            y,
            _ => 1,
            parameters,
            model,
            modelGradient,
            errorDefinitionInSigma,
            true);
    }
    
    public static ICostFunction LeastSquaresWithGaussNewtonApproximation(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        double errorDefinitionInSigma = 1)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)));
        return new LeastSquaresWithGaussNewtonApproximation(
            x,
            y,
            _ => yError,
            parameters,
            model,
            modelGradient,
            errorDefinitionInSigma,
            false);
    }
    
    public static ICostFunction LeastSquaresWithGaussNewtonApproximation(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        double errorDefinitionInSigma = 1)
    {
        DataValidation.EnsureMatchingSizesBetween((x, nameof(x)), (y, nameof(y)), (yError, nameof(yError)));
        return new LeastSquaresWithGaussNewtonApproximation(
            x,
            y,
            i => yError[i],
            parameters,
            model,
            modelGradient,
            errorDefinitionInSigma,
            false);
    }

    public static ICostFunction Sum(params ICostFunction[] components) => new CostFunctionSum(components);
}