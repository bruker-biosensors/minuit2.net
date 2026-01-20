namespace ExampleProblems.MinuitTutorialProblems;

public class FletcherPowellProblem(DerivativeConfiguration derivativeConfiguration)
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
    // see section 7.4 in https://seal.web.cern.ch/seal/documents/minuit/mntutorial.pdf
    // NOTE: start and optimum parameter values are swapped in the source

    private static readonly IReadOnlyList<string> Parameters = ["x", "y", "z"];
    private static readonly IReadOnlyList<double> InitialValues = [1, 0, 0];
    private static readonly IReadOnlyList<double> OptimumValues = [-1, 0, 0];

    private static readonly Func<IReadOnlyList<double>, double> Function = p =>
    {
        var (x, y, z) = (p[0], p[1], p[2]);
        return x < 0
            ? Math.Pow(z, 2) + 100 * Math.Pow(z - 5 * Math.Atan(y / x) / Math.PI, 2) +
              100 * Math.Pow(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1, 2)
            : Math.Pow(z, 2) + 100 * Math.Pow(z - 1d / 2 * (10 * Math.Atan(y / x) + 10 * Math.PI) / Math.PI, 2) +
              100 * Math.Pow(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1, 2);
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Gradient = p =>
    {
        var (x, y, z) = (p[0], p[1], p[2]);
        var g0 = x < 0
            ? 200 *
              (Math.Pow(Math.PI, 2) * x * (Math.Pow(x, 2) + Math.Pow(y, 2)) *
               (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1) +
               5 * y * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * (Math.PI * z - 5 * Math.Atan(y / x))) /
              (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 3d / 2))
            : 200 *
              (Math.Pow(Math.PI, 2) * x * (Math.Pow(x, 2) + Math.Pow(y, 2)) *
               (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1) +
               5 * y * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) *
               (Math.PI * z - 5 * Math.Atan(y / x) - 5 * Math.PI)) /
              (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 3d / 2));
        var g1 = x < 0
            ? 200 * (-5 * x * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) * (Math.PI * z - 5 * Math.Atan(y / x)) +
                     Math.Pow(Math.PI, 2) * y * (Math.Pow(x, 2) + Math.Pow(y, 2)) *
                     (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1)) /
              (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 3d / 2))
            : 200 *
              (-5 * x * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) *
               (Math.PI * z - 5 * Math.Atan(y / x) - 5 * Math.PI) +
               Math.Pow(Math.PI, 2) * y * (Math.Pow(x, 2) + Math.Pow(y, 2)) *
               (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1)) /
              (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 3d / 2));
        var g2 = x < 0
            ? 202 * z - 1000 * Math.Atan(y / x) / Math.PI
            : 202 * z - 1000 * Math.Atan(y / x) / Math.PI - 1000;
        return [g0, g1, g2];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Hessian = p =>
    {
        var (x, y, z) = (p[0], p[1], p[2]);
        var h00 = x < 0
            ? 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) - 10 * Math.PI * x * y * z +
                     50 * x * y * Math.Atan(y / x) + Math.Pow(Math.PI, 2) * Math.Pow(y, 4) -
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(y, 2)) /
              (Math.Pow(Math.PI, 2) * (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) + Math.Pow(y, 4)))
            : 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) - 10 * Math.PI * x * y * z +
                     50 * x * y * Math.Atan(y / x) + 50 * Math.PI * x * y + Math.Pow(Math.PI, 2) * Math.Pow(y, 4) -
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(y, 2)) / (Math.Pow(Math.PI, 2) *
                                             (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) + Math.Pow(y, 4)));
        var h01 = x < 0
            ? -5000 * x * y / (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 2)) +
              200 * x * y / Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 3d / 2) -
              2000 * Math.Pow(y, 2) * z / (Math.PI * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 2)) +
              10000 * Math.Pow(y, 2) * Math.Atan(y / x) /
              (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 2)) +
              1000 * z / (Math.PI * Math.Pow(x, 2) + Math.PI * Math.Pow(y, 2)) -
              5000 * Math.Atan(y / x) / (Math.Pow(Math.PI, 2) * Math.Pow(x, 2) + Math.Pow(Math.PI, 2) * Math.Pow(y, 2))
            : 200 * (Math.Pow(Math.PI, 2) * x * y * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 9d / 2) -
                     Math.Pow(Math.PI, 2) * x * y * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 4) *
                     (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - 1) +
                     5 * y * (-5 * x + 2 * y * (-Math.PI * z + 5 * Math.Atan(y / x) + 5 * Math.PI)) *
                     Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 7d / 2) +
                     5 * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 9d / 2) *
                     (Math.PI * z - 5 * Math.Atan(y / x) - 5 * Math.PI)) /
              (Math.Pow(Math.PI, 2) * Math.Pow(Math.Pow(x, 2) + Math.Pow(y, 2), 11d / 2));
        var h02 = 1000 * y / (Math.PI * (Math.Pow(x, 2) + Math.Pow(y, 2)));
        var h11 = x < 0
            ? 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) -
                     Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(x, 2) + 10 * Math.PI * x * y * z - 50 * x * y * Math.Atan(y / x) +
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 4)) /
              (Math.Pow(Math.PI, 2) * (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) + Math.Pow(y, 4)))
            : 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) -
                     Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(x, 2) + 10 * Math.PI * x * y * z - 50 * x * y * Math.Atan(y / x) -
                     50 * Math.PI * x * y +
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 4)) / (Math.Pow(Math.PI, 2) *
                                                               (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) +
                                                                Math.Pow(y, 4)));
        var h12 = -1000 * x / (Math.PI * (Math.Pow(x, 2) + Math.Pow(y, 2)));
        const double h22 = 202;
        return
        [
            h00, h01, h02,
            h01, h11, h12,
            h02, h12, h22
        ];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> HessianDiagonal = p =>
    {
        var (x, y, z) = (p[0], p[1], p[2]);
        var h00 = x < 0
            ? 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) - 10 * Math.PI * x * y * z +
                     50 * x * y * Math.Atan(y / x) + Math.Pow(Math.PI, 2) * Math.Pow(y, 4) -
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(y, 2)) /
              (Math.Pow(Math.PI, 2) * (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) + Math.Pow(y, 4)))
            : 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) - 10 * Math.PI * x * y * z +
                     50 * x * y * Math.Atan(y / x) + 50 * Math.PI * x * y + Math.Pow(Math.PI, 2) * Math.Pow(y, 4) -
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(y, 2)) / (Math.Pow(Math.PI, 2) *
                                             (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) + Math.Pow(y, 4)));
        var h11 = x < 0
            ? 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) -
                     Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(x, 2) + 10 * Math.PI * x * y * z - 50 * x * y * Math.Atan(y / x) +
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 4)) /
              (Math.Pow(Math.PI, 2) * (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) + Math.Pow(y, 4)))
            : 200 * (Math.Pow(Math.PI, 2) * Math.Pow(x, 4) +
                     2 * Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Pow(y, 2) -
                     Math.Pow(Math.PI, 2) * Math.Pow(x, 2) * Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) +
                     25 * Math.Pow(x, 2) + 10 * Math.PI * x * y * z - 50 * x * y * Math.Atan(y / x) -
                     50 * Math.PI * x * y +
                     Math.Pow(Math.PI, 2) * Math.Pow(y, 4)) / (Math.Pow(Math.PI, 2) *
                                                               (Math.Pow(x, 4) + 2 * Math.Pow(x, 2) * Math.Pow(y, 2) +
                                                                Math.Pow(y, 4)));
        const double h22 = 202;
        return [h00, h11, h22];
    };
}