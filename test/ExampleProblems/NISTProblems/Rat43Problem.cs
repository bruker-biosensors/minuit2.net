namespace ExampleProblems.NISTProblems;

public class Rat43Problem(DerivativeConfiguration derivativeConfiguration)
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
    // see https://www.itl.nist.gov/div898/strd/nls/data/ratkowsky3.shtml

    private static readonly IReadOnlyList<double> X =
        [1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0];

    private static readonly IReadOnlyList<double> Y = 
        [16.08, 33.83, 65.8, 97.2, 191.55, 326.2, 386.87, 520.53, 590.03, 651.92, 724.93, 699.56, 689.96, 637.56, 717.41];

    private static readonly IReadOnlyList<string> Parameters =
        ["b1", "b2", "b3", "b4"];

    private static readonly IReadOnlyList<double> InitialValues =
        [100, 10, 1, 1];

    private static readonly IReadOnlyList<double> OptimumValues =
        [6.9964151270E+02, 5.2771253025E+00, 7.5962938329E-01, 1.2792483859E+00];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        return b1 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -1 / b4);
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        var g0 = Math.Pow(Math.Exp(b2 - b3 * x) + 1, -1 / b4);
        var g1 = -b1 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(b4 + 1) / b4) * Math.Exp(b2 - b3 * x) / b4;
        var g2 = b1 * x * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(b4 + 1) / b4) * Math.Exp(b2 - b3 * x) / b4;
        var g3 = b1 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -1 / b4) * Math.Log(Math.Exp(b2 - b3 * x) + 1) /
                 Math.Pow(b4, 2);
        return [g0, g1, g2, g3];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        var h01 = -Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(b4 + 1) / b4) * Math.Exp(b2 - b3 * x) / b4;
        var h02 = x * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(b4 + 1) / b4) * Math.Exp(b2 - b3 * x) / b4;
        var h03 = Math.Pow(Math.Exp(b2 - b3 * x) + 1, -1 / b4) * Math.Log(Math.Exp(b2 - b3 * x) + 1) / Math.Pow(b4, 2);
        var h11 = b1 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(5 * b4 + 3) / b4) *
            (-b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, 2 * (2 * b4 + 1) / b4) * Math.Exp(b2 - b3 * x) +
             b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x) +
             Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x)) / Math.Pow(b4, 2);
        var h12 = b1 * x * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(5 * b4 + 3) / b4) *
            (b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, 2 * (2 * b4 + 1) / b4) * Math.Exp(b2 - b3 * x) -
             b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x) -
             Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x)) / Math.Pow(b4, 2);
        var h13 = b1 * Math.Pow((Math.Exp(b2) + Math.Exp(b3 * x)) * Math.Exp(-b3 * x), -(b4 + 1) / b4) *
                  (b4 - Math.Log((Math.Exp(b2) + Math.Exp(b3 * x)) * Math.Exp(-b3 * x))) * Math.Exp(b2 - b3 * x) /
                  Math.Pow(b4, 3);
        var h22 = b1 * Math.Pow(x, 2) * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(5 * b4 + 3) / b4) *
            (-b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, 2 * (2 * b4 + 1) / b4) * Math.Exp(b2 - b3 * x) +
             b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x) +
             Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x)) / Math.Pow(b4, 2);
        var h23 = b1 * x * Math.Pow((Math.Exp(b2) + Math.Exp(b3 * x)) * Math.Exp(-b3 * x), -(b4 + 1) / b4) *
                  (-b4 + Math.Log((Math.Exp(b2) + Math.Exp(b3 * x)) * Math.Exp(-b3 * x))) * Math.Exp(b2 - b3 * x) /
                  Math.Pow(b4, 3);
        var h33 = b1 * (-2 * b4 + Math.Log(Math.Exp(b2 - b3 * x) + 1)) * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -1 / b4) *
            Math.Log(Math.Exp(b2 - b3 * x) + 1) / Math.Pow(b4, 4);
        return
        [
            0.0, h01, h02, h03,
            h01, h11, h12, h13,
            h02, h12, h22, h23,
            h03, h13, h23, h33
        ];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = (x, p) =>
    {
        var (b1, b2, b3, b4) = (p[0], p[1], p[2], p[3]);
        var h11 = b1 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(5 * b4 + 3) / b4) *
            (-b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, 2 * (2 * b4 + 1) / b4) * Math.Exp(b2 - b3 * x) +
             b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x) +
             Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x)) / Math.Pow(b4, 2);
        var h22 = b1 * Math.Pow(x, 2) * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -(5 * b4 + 3) / b4) *
            (-b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, 2 * (2 * b4 + 1) / b4) * Math.Exp(b2 - b3 * x) +
             b4 * Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x) +
             Math.Pow(Math.Exp(b2 - b3 * x) + 1, (3 * b4 + 2) / b4) * Math.Exp(2 * b2 - 2 * b3 * x)) / Math.Pow(b4, 2);
        var h33 = b1 * (-2 * b4 + Math.Log(Math.Exp(b2 - b3 * x) + 1)) * Math.Pow(Math.Exp(b2 - b3 * x) + 1, -1 / b4) *
            Math.Log(Math.Exp(b2 - b3 * x) + 1) / Math.Pow(b4, 4);
        return [0, h11, h22, h33];
    };
}