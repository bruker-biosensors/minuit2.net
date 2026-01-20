using System.Diagnostics.CodeAnalysis;

namespace ExampleProblems.NISTProblems;

[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Crashes the native minimization processes. Needs further investigation.")]
public class Mgh10Problem(DerivativeConfiguration derivativeConfiguration)
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
    // see https://www.itl.nist.gov/div898/strd/nls/data/mgh10.shtml

    private static readonly IReadOnlyList<double> X =
        [50.0, 55.0, 60.0, 65.0, 70.0, 75.0, 80.0, 85.0, 90.0, 95.0, 100.0, 105.0, 110.0, 115.0, 120.0, 125.0];

    private static readonly IReadOnlyList<double> Y =
    [
        34780.0, 28610.0, 23650.0, 19630.0, 16370.0, 13720.0, 11540.0, 9744.0, 8261.0, 7030.0, 6005.0, 5147.0, 4427.0,
        3820.0, 3307.0, 2872.0
    ];

    private static readonly IReadOnlyList<string> Parameters = 
        ["b1", "b2", "b3"];
    
    private static readonly IReadOnlyList<double> InitialValues = 
        [2, 400000, 25000];
    
    private static readonly IReadOnlyList<double> OptimumValues = 
        [5.6096364710E-03, 6.1813463463E+03, 3.4522363462E+02];

    private static readonly Func<double, IReadOnlyList<double>, double> Model = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        return b1 * Math.Exp(b2 / (b3 + x));
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var g0 = Math.Exp(b2 / (b3 + x));
        var g1 = b1 * Math.Exp(b2 / (b3 + x)) / (b3 + x);
        var g2 = -b1 * b2 * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 2);
        return [g0, g1, g2];
    };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = (x, p) =>
    {
        var (b1, b2, b3) = (p[0], p[1], p[2]);
        var h01 = Math.Exp(b2 / (b3 + x)) / (b3 + x);
        var h02 = -b2 * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 2);
        var h11 = b1 * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 2);
        var h12 = b1 * (-b2 - b3 - x) * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 3);
        var h22 = b1 * b2 * (b2 + 2 * b3 + 2 * x) * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 4);
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
        var h11 = b1 * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 2);
        var h22 = b1 * b2 * (b2 + 2 * b3 + 2 * x) * Math.Exp(b2 / (b3 + x)) / Math.Pow(b3 + x, 4);
        return [0, h11, h22];
    };
}