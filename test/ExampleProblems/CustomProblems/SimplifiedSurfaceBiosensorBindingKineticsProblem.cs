using minuit2.net;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class SimplifiedSurfaceBiosensorBindingKineticsProblem(
    DerivativeConfiguration modelDerivativeConfiguration = WithoutDerivatives)
    : AnalyticalLeastSquaresProblem(
        XValues,
        YValues,
        YError,
        Configurations,
        OptimumValues,
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        modelDerivativeConfiguration)
{
    // Maximum simplified version of binding kinetics problem where amplitude and concentration are hardcoded to 1,
    // and association start and dissociation start are hardcoded to 0 and 100, respectively.
    // All following derivatives are auto-generated using sympy.

    private static readonly Func<double, IReadOnlyList<double>, double> Model =
        (x, p) =>
        {
            var (ka, kd) = (p[0], p[1]);

            return x < 100
                ? 1 - Math.Exp(x * (-ka - kd))
                : (1 - Math.Exp(-100 * ka - 100 * kd)) * Math.Exp(-kd * (x - 100));
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient =
        (x, p) =>
        {
            var (ka, kd) = (p[0], p[1]);

            var g0 = x < 100
                ? x * Math.Exp(-x * (ka + kd))
                : 100 * Math.Exp(-100 * ka - kd * x);
            var g1 = x < 100
                ? x * Math.Exp(-x * (ka + kd))
                : ((1 - Math.Exp(100 * ka + 100 * kd)) * (x - 100) + 100) *
                  Math.Exp(-100 * ka - kd * (x - 100) - 100 * kd);

            return [g0, g1];
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian =
        (x, p) =>
        {
            var (ka, kd) = (p[0], p[1]);

            var h00 = x < 100
                ? -Math.Pow(x, 2) * Math.Exp(-x * (ka + kd))
                : -10000 * Math.Exp(-100 * ka - kd * x);
            var h01 = x < 100
                ? -Math.Pow(x, 2) * Math.Exp(-x * (ka + kd))
                : -100 * x * Math.Exp(-100 * ka - kd * x);
            var h11 = x < 100
                ? -Math.Pow(x, 2) * Math.Exp(-x * (ka + kd))
                : (-200 * x - (1 - Math.Exp(100 * ka + 100 * kd)) * Math.Pow(x - 100, 2) + 10000) *
                  Math.Exp(-100 * ka - kd * (x - 100) - 100 * kd);

            return
            [
                h00, h01,
                h01, h11
            ];
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal =
        (x, p) =>
        {
            var h = ModelHessian(x, p);
            return [h[0], h[3]];
        };

    private static readonly IReadOnlyList<double> OptimumValues =
    [
        1e-2,
        1e-2
    ];

    private static readonly IReadOnlyList<ParameterConfiguration> Configurations =
    [
        Variable("ka", 1e-7, lowerLimit: 0),
        Variable("kd", 1e-7, lowerLimit: 0)
    ];

    private static readonly IReadOnlyList<double> XValues =
    [
        0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200, 210, 220, 230,
        240, 250, 260, 270, 280, 290, 300
    ];

    // Generated using Model(x, OptimumValues) adding random normal noise with a standard deviation of YError
    private static readonly IReadOnlyList<double> YValues =
    [
        0.0014677825891482553, 0.18188013378785522, 0.32992558356212337, 0.45041553188395367, 0.54794601921519193,
        0.63195112024476019, 0.69853633449106323, 0.75286490803859663, 0.7981616977365642, 0.83331949155031482,
        0.86616863624896234, 0.78174922906099609, 0.70818268046233734, 0.64271424905745012, 0.57898830590860717,
        0.52425958636511283, 0.47459845673511991, 0.43080972773016701, 0.38834996655153947, 0.35178861510658521,
        0.31925150166578198, 0.28732404660033928, 0.26252634588235924, 0.23540772239857896, 0.21266927896262663,
        0.19394038874520433, 0.17446775399099632, 0.15841619132882989, 0.14478862098494658, 0.13111385920544755,
        0.1156468064187659
    ];

    private const double YError = 0.001;
}
