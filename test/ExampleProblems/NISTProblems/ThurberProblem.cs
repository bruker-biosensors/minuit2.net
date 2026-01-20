using System.Diagnostics.CodeAnalysis;

namespace ExampleProblems.NISTProblems;

public class ThurberProblem(DerivativeConfiguration derivativeConfiguration)
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
    // see https://www.itl.nist.gov/div898/strd/nls/data/thurber.shtml

    private static readonly IReadOnlyList<double> X =
    [
        -3.067, -2.981, -2.921, -2.912, -2.84, -2.797, -2.702, -2.699, -2.633, -2.481, -2.363, -2.322, -1.501, -1.46,
        -1.274, -1.212, -1.1, -1.046, -0.915, -0.714, -0.566, -0.545, -0.4, -0.309, -0.109, -0.103, 0.01, 0.119, 0.377,
        0.79, 0.963, 1.006, 1.115, 1.572, 1.841, 2.047, 2.2
    ];

    private static readonly IReadOnlyList<double> Y =
    [
        80.574, 84.248, 87.264, 87.195, 89.076, 89.608, 89.868, 90.101, 92.405, 95.854, 100.696, 101.06, 401.672,
        390.724, 567.534, 635.316, 733.054, 759.087, 894.206, 990.785, 1090.109, 1080.914, 1122.643, 1178.351, 1260.531,
        1273.514, 1288.339, 1327.543, 1353.863, 1414.509, 1425.208, 1421.384, 1442.962, 1464.35, 1468.705, 1447.894,
        1457.628
    ];

    private static readonly IReadOnlyList<string> Parameters = ["b1", "b2", "b3", "b4", "b5", "b6", "b7"];

    private static readonly IReadOnlyList<double> InitialValues = [1000, 1000, 400, 40, 0.7, 0.3, 0.03];

    private static readonly IReadOnlyList<double> OptimumValues =
    [
        1.2881396800E+03, 1.4910792535E+03, 5.8323836877E+02, 7.5416644291E+01, 9.6629502864E-01, 3.9797285797E-01,
        4.9727297349E-02
    ];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2, b3, b4, b5, b6, b7) = (p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
        return (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
               (b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1);
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2, b3, b4, b5, b6, b7) = (p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
        var g0 = 1 / (b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1);
        var g1 = x / (b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1);
        var g2 = Math.Pow(x, 2) / (b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1);
        var g3 = Math.Pow(x, 3) / (b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1);
        var g4 = -x * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                 Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var g5 = Math.Pow(x, 2) * (-b1 - b2 * x - b3 * Math.Pow(x, 2) - b4 * Math.Pow(x, 3)) /
                 Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var g6 = Math.Pow(x, 3) * (-b1 - b2 * x - b3 * Math.Pow(x, 2) - b4 * Math.Pow(x, 3)) /
                 Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        return [g0, g1, g2, g3, g4, g5, g6];
    };

    [SuppressMessage("ReSharper", "InlineTemporaryVariable")] 
    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2, b3, b4, b5, b6, b7) = (p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
        var h04 = -x / Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var h05 = -Math.Pow(x, 2) / Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var h06 = -Math.Pow(x, 3) / Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var h14 = h05;
        var h15 = h06;
        var h16 = -Math.Pow(x, 4) / Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var h24 = h15;
        var h25 = h16;
        var h26 = -Math.Pow(x, 5) / Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var h34 = h16;
        var h35 = h26;
        var h36 = -Math.Pow(x, 6) / Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 2);
        var h44 = 2 * Math.Pow(x, 2) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        var h45 = 2 * Math.Pow(x, 3) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        var h46 = 2 * Math.Pow(x, 4) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        var h55 = h46;
        var h56 = 2 * Math.Pow(x, 5) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        var h66 = 2 * Math.Pow(x, 6) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        return
        [
            0.0, 0.0, 0.0, 0.0, h04, h05, h06,
            0.0, 0.0, 0.0, 0.0, h14, h15, h16,
            0.0, 0.0, 0.0, 0.0, h24, h25, h26,
            0.0, 0.0, 0.0, 0.0, h34, h35, h36,
            h04, h14, h24, h34, h44, h45, h46,
            h05, h15, h25, h35, h45, h55, h56,
            h06, h16, h26, h36, h46, h56, h66
        ];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = (x, p) =>
    {
        var (b1, b2, b3, b4, b5, b6, b7) = (p[0], p[1], p[2], p[3], p[4], p[5], p[6]);
        var h44 = 2 * Math.Pow(x, 2) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        var h55 = 2 * Math.Pow(x, 4) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        var h66 = 2 * Math.Pow(x, 6) * (b1 + b2 * x + b3 * Math.Pow(x, 2) + b4 * Math.Pow(x, 3)) /
                  Math.Pow(b5 * x + b6 * Math.Pow(x, 2) + b7 * Math.Pow(x, 3) + 1, 3);
        return [0, 0, 0, 0, h44, h55, h66];
    };
}