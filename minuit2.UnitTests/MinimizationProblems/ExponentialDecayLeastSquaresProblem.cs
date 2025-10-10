namespace minuit2.UnitTests.MinimizationProblems;

internal class ExponentialDecayLeastSquaresProblem : ConfigurableLeastSquaresProblem
{
    protected override Func<double, IReadOnlyList<double>, double> Model { get; } = (x, p) => p[0] * Math.Exp(-p[1] * x) + p[2];

    protected override Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient { get; } = (x, p) =>
    [
        Math.Exp(-p[1] * x),
        -x * p[0] * Math.Exp(-p[1] * x),
        1
    ];
    
    protected override IReadOnlyList<string> ParameterNames { get; } = ["amplitude", "rate", "offset"];

    // The following values are generated using the above model with parameters amplitude = 3, rate = 2, offset = 1,
    // adding random normal noise with a standard deviation of 0.01
    protected override IReadOnlyList<double> XValues { get; } =
    [
        0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2, 2.1, 2.2,
        2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9
    ];

    protected override IReadOnlyList<double> YValues { get; } =
    [
        3.99, 3.45, 3.02, 2.65, 2.36, 2.11, 1.9, 1.75, 1.6, 1.49, 1.41, 1.32, 1.28, 1.22, 1.19, 1.15, 1.14, 1.09, 1.08,
        1.07, 1.03, 1.05, 1.05, 1.04, 1.03, 1.02, 1.03, 1.02, 1.02, 1.01
    ];
    
    protected override double YError => 0.01; // standard deviation of noise used to generate the above y-values

    // Since the standard deviation of the noise overlying the data is chosen small enough, the optimal parameter
    // values are approximately equal to the values used to generate the data
    protected override IReadOnlyList<double> OptimumParameterValues { get; } = [3, 2, 1];

    protected override IReadOnlyList<double> DefaultInitialParameterValues { get; } = [2, 1, 0];
}