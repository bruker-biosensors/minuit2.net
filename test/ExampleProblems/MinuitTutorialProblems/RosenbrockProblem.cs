namespace ExampleProblems.MinuitTutorialProblems;

public class RosenbrockProblem(DerivativeConfiguration derivativeConfiguration)
    : MinuitTutorialProblem(
        Parameters,
        InitialValues,
        OptimumValues,
        Function,
        Gradient,
        Hessian,
        HessianDiagonal,
        derivativeConfiguration)
{
    // see section 7.1 in https://seal.web.cern.ch/seal/documents/minuit/mntutorial.pdf

    private static readonly IReadOnlyList<string> Parameters = ["x", "y"];
    private static readonly IReadOnlyList<double> InitialValues = [-1.2, 1];
    private static readonly IReadOnlyList<double> OptimumValues = [1, 1];

    private static readonly Func<IReadOnlyList<double>, double> Function = p =>
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
}