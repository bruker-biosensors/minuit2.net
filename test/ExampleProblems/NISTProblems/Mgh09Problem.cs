namespace ExampleProblems.NISTProblems;

public class Mgh09Problem(DerivativeConfiguration derivativeConfiguration)
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
    // see https://www.itl.nist.gov/div898/strd/nls/data/mgh09.shtml

    private static readonly IReadOnlyList<double> X =
        [4.0, 2.0, 1.0, 0.5, 0.25, 0.167, 0.125, 0.1, 0.0833, 0.0714, 0.0625];

    private static readonly IReadOnlyList<double> Y =
        [0.1957, 0.1947, 0.1735, 0.16, 0.0844, 0.0627, 0.0456, 0.0342, 0.0323, 0.0235, 0.0246];

    private static readonly IReadOnlyList<string> Parameters =
        ["b1", "b2", "b3", "b4"];

    private static readonly IReadOnlyList<double> InitialValues =
        [25, 39, 41.5, 39];

    private static readonly IReadOnlyList<double> OptimumValues =
        [1.9280693458E-01, 1.9128232873E-01, 1.2305650693E-01, 1.3606233068E-01];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        return b1 * (b2 * x + Math.Pow(x, 2)) / (b3 * x + b4 + Math.Pow(x, 2));
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        var g0 = x * (b2 + x) / (b3 * x + b4 + Math.Pow(x, 2));
        var g1 = b1 * x / (b3 * x + b4 + Math.Pow(x, 2));
        var g2 = -b1 * Math.Pow(x, 2) * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 2);
        var g3 = -b1 * x * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 2);
        return [g0, g1, g2, g3];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        var h01 = x / (b3 * x + b4 + Math.Pow(x, 2));
        var h02 = -Math.Pow(x, 2) * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 2);
        var h03 = x * (-b2 - x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 2);
        var h12 = -b1 * Math.Pow(x, 2) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 2);
        var h13 = -b1 * x / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 2);
        var h22 = 2 * b1 * Math.Pow(x, 3) * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 3);
        var h23 = 2 * b1 * Math.Pow(x, 2) * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 3);
        var h33 = 2 * b1 * x * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 3);
        return
        [
            0.0, h01, h02, h03,
            h01, 0.0, h12, h13,
            h02, h12, h22, h23,
            h03, h13, h23, h33
        ];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        var h22 = 2 * b1 * Math.Pow(x, 3) * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 3);
        var h33 = 2 * b1 * x * (b2 + x) / Math.Pow(b3 * x + b4 + Math.Pow(x, 2), 3);
        return [0, 0, h22, h33];
    };
}