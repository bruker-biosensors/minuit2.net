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
        DerivativeConfiguration modelDerivativeConfiguration)
    {
        var parameters = parameterConfigurations.Select(c => c.Name).ToList();
        
        Cost = (modelDerivativeConfiguration, yError) switch
        {
            (WithoutDerivatives, null) => LeastSquares(x, y, parameters, model),
            (WithoutDerivatives, not null) => LeastSquares(x, y, yError.Value, parameters, model),

            (WithGradient, null) => LeastSquares(x, y, parameters, model, modelGradient),
            (WithGradient, not null) => LeastSquares(x, y, yError.Value, parameters, model, modelGradient),
            
            (WithGradientAndHessian, null) => LeastSquares(x, y, parameters, model, modelGradient, modelHessian),
            (WithGradientAndHessian, not null) => LeastSquares(x, y, yError.Value, parameters, model, modelGradient, modelHessian),
        
            (WithGradientHessianAndHessianDiagonal, null) => LeastSquares(x, y, parameters, model, modelGradient, modelHessian, modelHessianDiagonal),
            (WithGradientHessianAndHessianDiagonal, not null) => LeastSquares(x, y, yError.Value, parameters, model, modelGradient, modelHessian, modelHessianDiagonal),
            
            _ => throw new ArgumentOutOfRangeException(nameof(modelDerivativeConfiguration), modelDerivativeConfiguration, null)
        };
        
        OptimumParameterValues = optimumParameterValues;
        ParameterConfigurations = parameterConfigurations;
    }

    public ICostFunction Cost { get; }
    public IReadOnlyCollection<double> OptimumParameterValues { get; }
    public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
}