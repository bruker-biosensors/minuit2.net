namespace ExampleProblems.MinuitTutorialProblems;

public class PowellProblem(DerivativeConfiguration derivativeConfiguration)
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
    // see section 7.3 in https://seal.web.cern.ch/seal/documents/minuit/mntutorial.pdf

    private static readonly IReadOnlyList<string> Parameters = ["w", "x", "y", "z"];
    private static readonly IReadOnlyList<double> InitialValues = [3, -1, 0, 1];
    private static readonly IReadOnlyList<double> OptimumValues = [0, 0, 0, 0];

    private static readonly Func<IReadOnlyList<double>, double> Function = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        return Math.Pow(w + 10 * x, 2) + 10 * Math.Pow(w - z, 4) + Math.Pow(x - 2 * y, 4) + 5 * Math.Pow(y - z, 2);
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Gradient = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        var g0 = 2 * w + 20 * x + 40 * Math.Pow(w - z, 3);
        var g1 = 20 * w + 200 * x + 4 * Math.Pow(x - 2 * y, 3);
        var g2 = 10 * y - 10 * z - 8 * Math.Pow(x - 2 * y, 3);
        var g3 = -10 * y + 10 * z - 40 * Math.Pow(w - z, 3);
        return [g0, g1, g2, g3];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Hessian = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        var h00 = 120 * Math.Pow(w - z, 2) + 2;
        const double h01 = 20;
        const double h02 = 0;
        var h03 = -120 * Math.Pow(w - z, 2);
        var h11 = 12 * Math.Pow(x - 2 * y, 2) + 200;
        var h12 = -24 * Math.Pow(x - 2 * y, 2);
        const double h13 = 0;
        var h22 = 48 * Math.Pow(x - 2 * y, 2) + 10;
        const double h23 = -10;
        var h33 = 120 * Math.Pow(w - z, 2) + 10;
        return
        [
            h00, h01, h02, h03,
            h01, h11, h12, h13,
            h02, h12, h22, h23,
            h03, h13, h23, h33
        ];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> HessianDiagonal = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        var h00 = 120 * Math.Pow(w - z, 2) + 2;
        var h11 = 12 * Math.Pow(x - 2 * y, 2) + 200;
        var h22 = 48 * Math.Pow(x - 2 * y, 2) + 10;
        var h33 = 120 * Math.Pow(w - z, 2) + 10;
        return [h00, h11, h22, h33];
    };
}