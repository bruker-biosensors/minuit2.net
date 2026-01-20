namespace ExampleProblems.NISTProblems;

public class Rat42Problem(DerivativeConfiguration derivativeConfiguration)
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
    // see https://www.itl.nist.gov/div898/strd/nls/data/ratkowsky2.shtml

    private static readonly IReadOnlyList<double> X = [9.0, 14.0, 21.0, 28.0, 42.0, 57.0, 63.0, 70.0, 79.0];
    private static readonly IReadOnlyList<double> Y = [8.93, 10.8, 18.59, 22.33, 39.35, 56.11, 61.73, 64.62, 67.08];
    private static readonly IReadOnlyList<string> Parameters = ["b1", "b2", "b3"];
    private static readonly IReadOnlyList<double> InitialValues = [100, 1, 0.1];
    private static readonly IReadOnlyList<double> OptimumValues = [7.2462237576E+01, 2.6180768402E+00, 6.7359200066E-02];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        return b1 / (Math.Exp(b2 - b3 * x) + 1);
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var g0 = 1d / (Math.Exp(b2 - b3 * x) + 1);
        var g1 = -1d / 4 * b1 / Math.Pow(Math.Cosh(1d / 2 * b2 - 1d / 2 * b3 * x), 2);
        var g2 = 1d / 4 * b1 * x / Math.Pow(Math.Cosh(1d / 2 * b2 - 1d / 2 * b3 * x), 2);
        return [g0, g1, g2];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var h01 = -(1d / 4) / Math.Pow(Math.Cosh(1d / 2 * b2 - 1d / 2 * b3 * x), 2);
        var h02 = 1d / 4 * x / Math.Pow(Math.Cosh(1d / 2 * b2 - 1d / 2 * b3 * x), 2);
        var h11 = b1 * (-(Math.Exp(b2 - b3 * x) + 1) * Math.Exp(b2 - b3 * x) + 2 * Math.Exp(2 * b2 - 2 * b3 * x)) /
                  Math.Pow(Math.Exp(b2 - b3 * x) + 1, 3);
        var h12 = b1 * x * ((Math.Exp(b2 - b3 * x) + 1) * Math.Exp(b2 - b3 * x) - 2 * Math.Exp(2 * b2 - 2 * b3 * x)) /
                  Math.Pow(Math.Exp(b2 - b3 * x) + 1, 3);
        var h22 = b1 * Math.Pow(x, 2) *
                  (-(Math.Exp(b2 - b3 * x) + 1) * Math.Exp(b2 - b3 * x) + 2 * Math.Exp(2 * b2 - 2 * b3 * x)) /
                  Math.Pow(Math.Exp(b2 - b3 * x) + 1, 3);
        return
        [
            0.0, h01, h02,
            h01, h11, h12,
            h02, h12, h22
        ];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var h11 = b1 * (-(Math.Exp(b2 - b3 * x) + 1) * Math.Exp(b2 - b3 * x) + 2 * Math.Exp(2 * b2 - 2 * b3 * x)) /
                  Math.Pow(Math.Exp(b2 - b3 * x) + 1, 3);
        var h22 = b1 * Math.Pow(x, 2) *
                  (-(Math.Exp(b2 - b3 * x) + 1) * Math.Exp(b2 - b3 * x) + 2 * Math.Exp(2 * b2 - 2 * b3 * x)) /
                  Math.Pow(Math.Exp(b2 - b3 * x) + 1, 3);
        return [0, h11, h22];
    };
}