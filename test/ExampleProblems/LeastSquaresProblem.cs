using minuit2.net;
using minuit2.net.CostFunctions;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.CostFunctions.CostFunction;

namespace ExampleProblems;

public abstract class LeastSquaresProblem : IConfiguredProblem
{
    protected LeastSquaresProblem(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double? yError,
        IReadOnlyList<ParameterConfiguration> parameterConfigurations,
        IReadOnlyList<double> optimumParameterValues,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessianDiagonal,
        DerivativeConfiguration modelDerivativeConfiguration,
        double errorDefinitionInSigma = 1)
    {
        var parameters = parameterConfigurations.Select(c => c.Name).ToList();
        
        Cost = (modelDerivativeConfiguration, yError) switch
        {
            (WithoutDerivatives, null) => LeastSquares(x, y, parameters, model, errorDefinitionInSigma),
            (WithoutDerivatives, not null) => LeastSquares(x, y, yError.Value, parameters, model, errorDefinitionInSigma),

            (WithGradient, null) => LeastSquares(x, y, parameters, model, modelGradient, errorDefinitionInSigma),
            (WithGradient, not null) => LeastSquares(x, y, yError.Value, parameters, model, modelGradient, errorDefinitionInSigma),
            
            (WithGradientAndHessian, null) => LeastSquares(x, y, parameters, model, modelGradient, modelHessian, errorDefinitionInSigma),
            (WithGradientAndHessian, not null) => LeastSquares(x, y, yError.Value, parameters, model, modelGradient, modelHessian, errorDefinitionInSigma),
        
            (WithGradientHessianAndHessianDiagonal, null) => LeastSquares(x, y, parameters, model, modelGradient, modelHessian, modelHessianDiagonal, errorDefinitionInSigma),
            (WithGradientHessianAndHessianDiagonal, not null) => LeastSquares(x, y, yError.Value, parameters, model, modelGradient, modelHessian, modelHessianDiagonal, errorDefinitionInSigma),
            
            _ => throw new ArgumentOutOfRangeException(nameof(modelDerivativeConfiguration), modelDerivativeConfiguration, null)
        };
        
        OptimumParameterValues = optimumParameterValues;
        ParameterConfigurations = parameterConfigurations;
    }

    public ICostFunction Cost { get; }
    public IReadOnlyCollection<double> OptimumParameterValues { get; }
    public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
}