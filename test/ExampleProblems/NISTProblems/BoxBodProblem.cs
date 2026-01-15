namespace ExampleProblems.NISTProblems;

public class BoxBodProblem(DerivativeConfiguration derivativeConfiguration)
    : NistProblem(
        X,
        Y,
        Parameters,
        InitialValues,
        OptimumValues,
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        derivativeConfiguration)
{
    // see https://www.itl.nist.gov/div898/strd/nls/data/boxbod.shtml

    private static readonly IReadOnlyList<double> X = [1, 2, 3, 5, 7, 10];
    private static readonly IReadOnlyList<double> Y = [109, 149, 149, 191, 213, 224];
    private static readonly IReadOnlyList<string> Parameters = ["b1", "b2"];
    private static readonly IReadOnlyList<double> InitialValues = [1, 1];
    private static readonly IReadOnlyList<double> OptimumValues = [2.1380940889E+02, 5.4723748542E-01];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2) = (p[0], p[1]);
        return b1 * (1 - Math.Exp(-b2 * x));
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2) = (p[0], p[1]);
        var g0 = 1 - Math.Exp(-b2 * x);
        var g1 = b1 * x * Math.Exp(-b2 * x);
        return [g0, g1];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2) = (p[0], p[1]);
        var h01 = x * Math.Exp(-b2 * x);
        var h11 = -b1 * Math.Pow(x, 2) * Math.Exp(-b2 * x);
        return
        [
            0.0, h01,
            h01, h11
        ];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = (x, p) =>
    {
        var (b1, b2) = (p[0], p[1]);
        var h11 = -b1 * Math.Pow(x, 2) * Math.Exp(-b2 * x);
        return [0, h11];
    };
}