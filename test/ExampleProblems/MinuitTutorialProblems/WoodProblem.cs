namespace ExampleProblems.MinuitTutorialProblems;

public class WoodProblem(DerivativeConfiguration derivativeConfiguration)
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
    // see section 7.2 in https://seal.web.cern.ch/seal/documents/minuit/mntutorial.pdf

    private static readonly IReadOnlyList<string> Parameters = ["w", "x", "y", "z"];
    private static readonly IReadOnlyList<double> InitialValues = [-3, -1, -3, -1];
    private static readonly IReadOnlyList<double> OptimumValues = [1, 1, 1, 1];

    private static readonly Func<IReadOnlyList<double>, double> Function = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        return Math.Pow(1 - y, 2) + Math.Pow(w - 1, 2) + 100 * Math.Pow(-Math.Pow(w, 2) + x, 2) +
               10.1 * Math.Pow(x - 1, 2) + (19.8 * x - 19.8) * (z - 1) + 90 * Math.Pow(-Math.Pow(y, 2) + z, 2) +
               10.1 * Math.Pow(z - 1, 2);
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Gradient = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        var g0 = 400 * w * (Math.Pow(w, 2) - x) + 2 * w - 2;
        var g1 = -200 * Math.Pow(w, 2) + 220.2 * x + 19.8 * z - 40.0;
        var g2 = 360 * y * (Math.Pow(y, 2) - z) + 2 * y - 2;
        var g3 = 19.8 * x - 180 * Math.Pow(y, 2) + 200.2 * z - 40.0;
        return [g0, g1, g2, g3];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Hessian = p =>
    {
        var (w, x, y, z) = (p[0], p[1], p[2], p[3]);
        var h00 = 1200 * Math.Pow(w, 2) - 400 * x + 2;
        var h01 = -400 * w;
        const double h02 = 0;
        const double h03 = 0;
        const double h11 = 220.2;
        const double h12 = 0;
        const double h13 = 19.8;
        var h22 = 1080 * Math.Pow(y, 2) - 360 * z + 2;
        var h23 = -360 * y;
        const double h33 = 200.2;
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
        var h00 = 1200 * Math.Pow(w, 2) - 400 * x + 2;
        const double h11 = 220.2;
        var h22 = 1080 * Math.Pow(y, 2) - 360 * z + 2;
        const double h33 = 200.2;
        return [h00, h11, h22, h33];
    };
}