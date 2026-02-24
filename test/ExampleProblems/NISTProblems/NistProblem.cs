using static minuit2.net.ParameterConfiguration;

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
    : AnalyticalLeastSquaresProblem(
        x,
        y,
        null,
        parameters.Zip(initialValues, (name, value) => Variable(name, value)).ToList(),
        optimumValues,
        model,
        modelGradient,
        modelHessian,
        modelHessianDiagonal,
        modelDerivativeConfiguration);