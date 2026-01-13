using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems.MinuitTutorialProblems;

public class RosenbrockProblem(bool hasGradient, bool hasHessian, bool hasHessianDiagonal) : IConfiguredProblem
{
    // see section 7.1 in https://seal.web.cern.ch/seal/documents/minuit/mntutorial.pdf

    private static readonly Func<IReadOnlyList<double>, double> Value = p =>
    {
        var (x, y) = (p[0], p[1]);
        return Math.Pow(1 - x, 2) + 100 * Math.Pow(-Math.Pow(x, 2) + y, 2);
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Gradient = p =>
    {
        var (x, y) = (p[0], p[1]);
        var g0 = 400 * x * (Math.Pow(x, 2) - y) + 2 * x - 2;
        var g1 = -200 * Math.Pow(x, 2) + 200 * y;
        return [g0, g1];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Hessian = p =>
    {
        var (x, y) = (p[0], p[1]);
        var h00 = 1200 * Math.Pow(x, 2) - 400 * y + 2;
        var h01 = -400 * x;
        const double h11 = 200;
        return
        [
            h00, h01,
            h01, h11
        ];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> HessianDiagonal = p =>
    {
        var (x, y) = (p[0], p[1]);
        var h00 = 1200 * Math.Pow(x, 2) - 400 * y + 2;
        const double h11 = 200;
        return [h00, h11];
    };

    public ICostFunction Cost { get; } = new CostFunction(
        ["x", "y"],
        Value,
        hasGradient ? Gradient : null,
        hasHessian ? Hessian : null,
        hasHessianDiagonal ? HessianDiagonal : null);

    public IReadOnlyCollection<double> OptimumParameterValues { get; } = [1, 1];

    public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; } =
        [ParameterConfiguration.Variable("x", -1.2), ParameterConfiguration.Variable("y", 1)];
}