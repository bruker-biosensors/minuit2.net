using minuit2.net;
using minuit2.net.CostFunctions;

namespace minuit2.UnitTests.TestUtilities;

internal class ModelEvaluatingCostFunction(
    double x,
    IReadOnlyList<string> parameters,
    Func<double, IReadOnlyList<double>, double> model,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessian = null,
    Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelHessianDiagonal = null)
    : ICostFunction
{
    public IReadOnlyList<string> Parameters { get; } = parameters;
    public bool HasGradient { get; } = modelGradient != null;
    public bool HasHessian { get; } = modelHessian != null;
    public bool HasHessianDiagonal { get; } = modelHessianDiagonal != null;
    public double ErrorDefinition => 1;

    public double ValueFor(IReadOnlyList<double> parameterValues) => model(x, parameterValues);

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues) => modelGradient!(x, parameterValues);

    public IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues) => modelHessian!(x, parameterValues);

    public IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues) => modelHessianDiagonal!(x, parameterValues);

    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => this;
}