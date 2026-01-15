using minuit2.net;
using minuit2.net.CostFunctions;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.CostFunctions.CostFunction;

namespace ExampleProblems.NISTProblems;

public abstract class NistProblem(
    IReadOnlyList<double> x,
    IReadOnlyList<double> y,
    IReadOnlyList<string> parameters,
    IReadOnlyList<double> initialValues,
    IReadOnlyList<double> optimumValues,
    Func<double, IReadOnlyList<double>, double> model,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelGradient,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessian,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>> modelHessianDiagonal,
    DerivativeConfiguration modelDerivativeConfiguration)
    : IConfiguredProblem
{
    public ICostFunction Cost { get; } = modelDerivativeConfiguration switch
    {
        WithoutDerivatives => LeastSquares(x, y, parameters, model),
        WithGradient => LeastSquares(x, y, parameters, model, modelGradient),
        WithGradientAndHessian => LeastSquares(x, y, parameters, model, modelGradient, modelHessian),
        WithGradientHessianAndHessianDiagonal => LeastSquares(x, y, parameters, model, modelGradient, modelHessian, modelHessianDiagonal),
        _ => throw new ArgumentOutOfRangeException(nameof(modelDerivativeConfiguration), modelDerivativeConfiguration, null)
    };

    public IReadOnlyCollection<double> OptimumParameterValues { get; } = optimumValues;

    public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; } =
        parameters.Zip(initialValues, (name, value) => ParameterConfiguration.Variable(name, value)).ToList();
}