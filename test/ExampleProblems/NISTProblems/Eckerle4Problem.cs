using System.Diagnostics.CodeAnalysis;

namespace ExampleProblems.NISTProblems;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Crashes the native minimization processes. Needs further investigation.")]
public class Eckerle4Problem(DerivativeConfiguration derivativeConfiguration)
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
    // see https://www.itl.nist.gov/div898/strd/nls/data/eckerle4.shtml

    private static readonly IReadOnlyList<double> X =
    [
        400.0, 405.0, 410.0, 415.0, 420.0, 425.0, 430.0, 435.0, 436.5, 438.0, 439.5, 441.0, 442.5, 444.0, 445.5, 447.0,
        448.5, 450.0, 451.5, 453.0, 454.5, 456.0, 457.5, 459.0, 460.5, 462.0, 463.5, 465.0, 470.0, 475.0, 480.0, 485.0,
        490.0, 495.0, 500.0
    ];

    private static readonly IReadOnlyList<double> Y =
    [
        0.0001575, 0.0001699, 0.000235, 0.0003102, 0.0004917, 0.000871, 0.0017418, 0.00464, 0.0065895, 0.0097302,
        0.0149002, 0.023731, 0.0401683, 0.0712559, 0.1264458, 0.2073413, 0.2902366, 0.3445623, 0.3698049, 0.3668534,
        0.3106727, 0.2078154, 0.1164354, 0.0616764, 0.03372, 0.0194023, 0.0117831, 0.0074357, 0.0022732, 0.00088,
        0.0004579, 0.0002345, 0.0001586, 0.0001143, 7.1e-05
    ];

    private static readonly IReadOnlyList<string> Parameters =
        ["b1", "b2", "b3"];

    private static readonly IReadOnlyList<double> InitialValues =
        [1, 10, 500];

    private static readonly IReadOnlyList<double> OptimumValues =
        [1.5543827178E+00, 4.0888321754E+00, 4.5154121844E+02];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        return b1 * Math.Exp(-0.5 * Math.Pow(-b3 + x, 2) / Math.Pow(b2, 2)) / b2;
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var g0 = Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / b2;
        var g1 = b1 * (-Math.Pow(b2, 2) + 1.0 * Math.Pow(b3 - x, 2)) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 4);
        var g2 = -1.0 * b1 * (b3 - x) * Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 3);
        return [g0, g1, g2];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var h01 = (-Math.Pow(b2, 2) + 1.0 * Math.Pow(b3 - x, 2)) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 4);
        var h02 = 1.0 * (-b3 + x) * Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 3);
        var h11 = b1 * (2 * Math.Pow(b2, 4) - 5.0 * Math.Pow(b2, 2) * Math.Pow(b3 - x, 2) + 1.0 * Math.Pow(b3 - x, 4)) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 7);
        var h12 = b1 * (3.0 * Math.Pow(b2, 2) - 1.0 * Math.Pow(b3 - x, 2)) * (b3 - x) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 6);
        var h22 = 1.0 * b1 * (-Math.Pow(b2, 2) + Math.Pow(b3 - x, 2)) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 5);
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
        var h11 = b1 * (2 * Math.Pow(b2, 4) - 5.0 * Math.Pow(b2, 2) * Math.Pow(b3 - x, 2) + 1.0 * Math.Pow(b3 - x, 4)) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 7);
        var h22 = 1.0 * b1 * (-Math.Pow(b2, 2) + Math.Pow(b3 - x, 2)) *
            Math.Exp(-0.5 * Math.Pow(b3 - x, 2) / Math.Pow(b2, 2)) / Math.Pow(b2, 5);
        return [0, h11, h22];
    };
}