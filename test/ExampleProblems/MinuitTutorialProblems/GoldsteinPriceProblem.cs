namespace ExampleProblems.MinuitTutorialProblems;

public class GoldsteinPriceProblem(DerivativeConfiguration derivativeConfiguration)
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
    // see section 7.5 in https://seal.web.cern.ch/seal/documents/minuit/mntutorial.pdf

    private static readonly IReadOnlyList<string> Parameters = ["x", "y"];
    private static readonly IReadOnlyList<double> InitialValues = [-0.4, -0.6];
    private static readonly IReadOnlyList<double> OptimumValues = [0, -1];

    private static readonly Func<IReadOnlyList<double>, double> Function = p =>
    {
        var (x, y) = (p[0], p[1]);
        return (Math.Pow(2 * x - 3 * y, 2) *
                   (12 * Math.Pow(x, 2) - 36 * x * y - 32 * x + 27 * Math.Pow(y, 2) + 48 * y + 18) + 30) *
               (Math.Pow(x + y + 1, 2) * (3 * Math.Pow(x, 2) + 6 * x * y - 14 * x + 3 * Math.Pow(y, 2) - 14 * y + 19) +
                1);
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Gradient = p =>
    {
        var (x, y) = (p[0], p[1]);
        var g0 =
            4 * (2 * x - 3 * y) *
            (Math.Pow(x + y + 1, 2) * (3 * Math.Pow(x, 2) + 6 * x * y - 14 * x + 3 * Math.Pow(y, 2) - 14 * y + 19) +
             1) * (12 * Math.Pow(x, 2) - 36 * x * y - 32 * x + 27 * Math.Pow(y, 2) + 48 * y -
                (2 * x - 3 * y) * (-6 * x + 9 * y + 8) + 18) + 2 *
            (Math.Pow(2 * x - 3 * y, 2) *
                (12 * Math.Pow(x, 2) - 36 * x * y - 32 * x + 27 * Math.Pow(y, 2) + 48 * y + 18) + 30) * (x + y + 1) *
            (3 * Math.Pow(x, 2) + 6 * x * y - 14 * x + 3 * Math.Pow(y, 2) - 14 * y + (x + y + 1) * (3 * x + 3 * y - 7) +
             19);
        var g1 =
            6 * (2 * x - 3 * y) *
            (Math.Pow(x + y + 1, 2) * (3 * Math.Pow(x, 2) + 6 * x * y - 14 * x + 3 * Math.Pow(y, 2) - 14 * y + 19) +
             1) * (-12 * Math.Pow(x, 2) + 36 * x * y + 32 * x - 27 * Math.Pow(y, 2) - 48 * y +
                (2 * x - 3 * y) * (-6 * x + 9 * y + 8) - 18) + 2 *
            (Math.Pow(2 * x - 3 * y, 2) *
                (12 * Math.Pow(x, 2) - 36 * x * y - 32 * x + 27 * Math.Pow(y, 2) + 48 * y + 18) + 30) * (x + y + 1) *
            (3 * Math.Pow(x, 2) + 6 * x * y - 14 * x + 3 * Math.Pow(y, 2) - 14 * y + (x + y + 1) * (3 * x + 3 * y - 7) +
             19);
        return [g0, g1];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> Hessian = p =>
    {
        var (x, y) = (p[0], p[1]);
        var h00 = 8064 * Math.Pow(x, 6) - 12096 * Math.Pow(x, 5) * y - 32256 * Math.Pow(x, 5) -
                  19440 * Math.Pow(x, 4) * Math.Pow(y, 2) + 40320 * Math.Pow(x, 4) * y + 28560 * Math.Pow(x, 4) +
                  24480 * Math.Pow(x, 3) * Math.Pow(y, 3) + 51840 * Math.Pow(x, 3) * Math.Pow(y, 2) -
                  3360 * Math.Pow(x, 3) * y + 26880 * Math.Pow(x, 3) + 15660 * Math.Pow(x, 2) * Math.Pow(y, 4) -
                  48960 * Math.Pow(x, 2) * Math.Pow(y, 3) - 64440 * Math.Pow(x, 2) * Math.Pow(y, 2) -
                  92160 * Math.Pow(x, 2) * y - 29448 * Math.Pow(x, 2) - 11016 * x * Math.Pow(y, 5) -
                  20880 * x * Math.Pow(y, 4) + 7440 * x * Math.Pow(y, 3) + 59040 * x * Math.Pow(y, 2) + 34704 * x * y -
                  6432 * x - 2916 * Math.Pow(y, 6) + 7344 * Math.Pow(y, 5) + 17460 * Math.Pow(y, 4) +
                  10080 * Math.Pow(y, 3) +
                  15552 * Math.Pow(y, 2) + 14688 * y + 2520;
        var h01 = -2016 * Math.Pow(x, 6) - 7776 * Math.Pow(x, 5) * y + 8064 * Math.Pow(x, 5) +
                  18360 * Math.Pow(x, 4) * Math.Pow(y, 2) + 25920 * Math.Pow(x, 4) * y - 840 * Math.Pow(x, 4) +
                  20880 * Math.Pow(x, 3) * Math.Pow(y, 3) - 48960 * Math.Pow(x, 3) * Math.Pow(y, 2) -
                  42960 * Math.Pow(x, 3) * y - 30720 * Math.Pow(x, 3) - 27540 * Math.Pow(x, 2) * Math.Pow(y, 4) -
                  41760 * Math.Pow(x, 2) * Math.Pow(y, 3) + 11160 * Math.Pow(x, 2) * Math.Pow(y, 2) +
                  59040 * Math.Pow(x, 2) * y + 17352 * Math.Pow(x, 2) - 17496 * x * Math.Pow(y, 5) +
                  36720 * x * Math.Pow(y, 4) + 69840 * x * Math.Pow(y, 3) + 30240 * x * Math.Pow(y, 2) + 31104 * x * y +
                  14688 * x + 6804 * Math.Pow(y, 6) + 11664 * Math.Pow(y, 5) - 5940 * Math.Pow(y, 4) -
                  47520 * Math.Pow(y, 3) - 70848 * Math.Pow(y, 2) - 38592 * y - 4680;
        var h11 = -1296 * Math.Pow(x, 6) + 7344 * Math.Pow(x, 5) * y + 5184 * Math.Pow(x, 5) +
                  15660 * Math.Pow(x, 4) * Math.Pow(y, 2) - 24480 * Math.Pow(x, 4) * y - 10740 * Math.Pow(x, 4) -
                  36720 * Math.Pow(x, 3) * Math.Pow(y, 3) - 41760 * Math.Pow(x, 3) * Math.Pow(y, 2) +
                  7440 * Math.Pow(x, 3) * y + 19680 * Math.Pow(x, 3) - 43740 * Math.Pow(x, 2) * Math.Pow(y, 4) +
                  73440 * Math.Pow(x, 2) * Math.Pow(y, 3) + 104760 * Math.Pow(x, 2) * Math.Pow(y, 2) +
                  30240 * Math.Pow(x, 2) * y + 15552 * Math.Pow(x, 2) + 40824 * x * Math.Pow(y, 5) +
                  58320 * x * Math.Pow(y, 4) - 23760 * x * Math.Pow(y, 3) - 142560 * x * Math.Pow(y, 2) -
                  141696 * x * y -
                  38592 * x + 40824 * Math.Pow(y, 6) - 27216 * Math.Pow(y, 5) - 132840 * Math.Pow(y, 4) +
                  38880 * Math.Pow(y, 3) + 172152 * Math.Pow(y, 2) + 73728 * y + 6120;
        return
        [
            h00, h01,
            h01, h11
        ];
    };

    private static readonly Func<IReadOnlyList<double>, IReadOnlyList<double>> HessianDiagonal = p =>
    {
        var (x, y) = (p[0], p[1]);
        var h00 = 8064 * Math.Pow(x, 6) - 12096 * Math.Pow(x, 5) * y - 32256 * Math.Pow(x, 5) -
                  19440 * Math.Pow(x, 4) * Math.Pow(y, 2) + 40320 * Math.Pow(x, 4) * y + 28560 * Math.Pow(x, 4) +
                  24480 * Math.Pow(x, 3) * Math.Pow(y, 3) + 51840 * Math.Pow(x, 3) * Math.Pow(y, 2) -
                  3360 * Math.Pow(x, 3) * y + 26880 * Math.Pow(x, 3) + 15660 * Math.Pow(x, 2) * Math.Pow(y, 4) -
                  48960 * Math.Pow(x, 2) * Math.Pow(y, 3) - 64440 * Math.Pow(x, 2) * Math.Pow(y, 2) -
                  92160 * Math.Pow(x, 2) * y - 29448 * Math.Pow(x, 2) - 11016 * x * Math.Pow(y, 5) -
                  20880 * x * Math.Pow(y, 4) + 7440 * x * Math.Pow(y, 3) + 59040 * x * Math.Pow(y, 2) + 34704 * x * y -
                  6432 * x - 2916 * Math.Pow(y, 6) + 7344 * Math.Pow(y, 5) + 17460 * Math.Pow(y, 4) +
                  10080 * Math.Pow(y, 3) +
                  15552 * Math.Pow(y, 2) + 14688 * y + 2520;
        var h11 = -1296 * Math.Pow(x, 6) + 7344 * Math.Pow(x, 5) * y + 5184 * Math.Pow(x, 5) +
                  15660 * Math.Pow(x, 4) * Math.Pow(y, 2) - 24480 * Math.Pow(x, 4) * y - 10740 * Math.Pow(x, 4) -
                  36720 * Math.Pow(x, 3) * Math.Pow(y, 3) - 41760 * Math.Pow(x, 3) * Math.Pow(y, 2) +
                  7440 * Math.Pow(x, 3) * y + 19680 * Math.Pow(x, 3) - 43740 * Math.Pow(x, 2) * Math.Pow(y, 4) +
                  73440 * Math.Pow(x, 2) * Math.Pow(y, 3) + 104760 * Math.Pow(x, 2) * Math.Pow(y, 2) +
                  30240 * Math.Pow(x, 2) * y + 15552 * Math.Pow(x, 2) + 40824 * x * Math.Pow(y, 5) +
                  58320 * x * Math.Pow(y, 4) - 23760 * x * Math.Pow(y, 3) - 142560 * x * Math.Pow(y, 2) -
                  141696 * x * y -
                  38592 * x + 40824 * Math.Pow(y, 6) - 27216 * Math.Pow(y, 5) - 132840 * Math.Pow(y, 4) +
                  38880 * Math.Pow(y, 3) + 172152 * Math.Pow(y, 2) + 73728 * y + 6120;
        return [h00, h11];
    };
}