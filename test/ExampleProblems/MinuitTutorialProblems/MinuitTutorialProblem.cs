using minuit2.net;
using minuit2.net.CostFunctions;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.MinuitTutorialProblems;

public abstract class MinuitTutorialProblem(
    IReadOnlyList<string> parameters,
    IReadOnlyList<double> initialValues,
    IReadOnlyList<double> optimumValues,
    Func<IReadOnlyList<double>, double> function,
    Func<IReadOnlyList<double>, IReadOnlyList<double>> gradient,
    Func<IReadOnlyList<double>, IReadOnlyList<double>> hessian,
    Func<IReadOnlyList<double>, IReadOnlyList<double>> hessianDiagonal,
    DerivativeConfiguration derivativeConfiguration)
    : IConfiguredProblem
{
    public ICostFunction Cost { get; } = derivativeConfiguration switch
    {
        WithoutDerivatives => new CostFunction(parameters, function),
        WithGradient => new CostFunction(parameters, function, gradient),
        WithGradientAndHessian => new CostFunction(parameters, function, gradient, hessian),
        WithGradientHessianAndHessianDiagonal => new CostFunction(parameters, function, gradient, hessian, hessianDiagonal),
        _ => throw new ArgumentOutOfRangeException(nameof(derivativeConfiguration), derivativeConfiguration, null)
    };

    public IReadOnlyCollection<double> OptimumParameterValues { get; } = optimumValues;

    public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; } =
        parameters.Zip(initialValues, (name, value) => Variable(name, value)).ToList();
}