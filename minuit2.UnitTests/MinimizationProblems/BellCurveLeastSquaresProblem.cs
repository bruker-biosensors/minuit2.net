namespace minuit2.UnitTests.MinimizationProblems;

internal class BellCurveLeastSquaresProblem : ConfigurableLeastSquaresProblem
{
    private static double Bell(double x, double location, double variance)
    {
        var dx = x - location;
        return 1 / Math.Sqrt(2 * Math.PI * variance) * Math.Exp(-dx * dx / (2 * variance));
    }
    
    protected override Func<double, IList<double>, double> Model { get; } = (x, p) => Bell(x, p[0], p[1]);
    
    protected override Func<double, IList<double>, IList<double>> ModelGradient { get; } = (x, p) =>
    [
        (x - p[0]) / p[1] * Bell(x, p[0], p[1]),
        ((x - p[0]) * (x - p[0]) / p[1] - 1) / (2 * p[1]) * Bell(x, p[0], p[1])
    ];

    protected override IReadOnlyCollection<string> ParameterNames { get; } = ["location", "variance"];

    // The following values are generated using the above model with parameters location = 5, variance = 2,
    // adding random normal noise with a standard deviation of 0.001
    protected override IReadOnlyCollection<double> XValues { get; } =
    [
        0, 0.2, 0.4, 0.6, 0.8, 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2, 2.4, 2.6, 2.8, 3, 3.2, 3.4, 3.6, 3.8, 4, 4.2, 4.4, 4.6,
        4.8, 5, 5.2, 5.4, 5.6, 5.8, 6, 6.2, 6.4, 6.6, 6.8, 7, 7.2, 7.4, 7.6, 7.8, 8, 8.2, 8.4, 8.6, 8.8, 9, 9.2, 9.4,
        9.6, 9.8
    ];

    protected override IReadOnlyCollection<double> YValues { get; } =
    [
        0, 0.001, 0.003, 0.002, 0.004, 0.006, 0.007, 0.012, 0.015, 0.021, 0.03, 0.038, 0.053, 0.066, 0.085, 0.104,
        0.127, 0.148, 0.173, 0.197, 0.217, 0.241, 0.259, 0.272, 0.28, 0.282, 0.281, 0.272, 0.258, 0.24, 0.219, 0.196,
        0.174, 0.147, 0.125, 0.104, 0.085, 0.069, 0.053, 0.039, 0.03, 0.021, 0.017, 0.011, 0.007, 0.005, 0.003, 0.002,
        0.004, 0
    ];

    protected override double YError => 0.001; // standard deviation of noise used to generate the above y-values

    // Since the standard deviation of the noise overlying the data is chosen small enough, the optimal parameter
    // values are approximately equal to the values used to generate the data
    public override IReadOnlyCollection<double> OptimumParameterValues { get; } = [5, 2];

    protected override IReadOnlyCollection<double> DefaultInitialParameterValues { get; } = [4, 3];
}