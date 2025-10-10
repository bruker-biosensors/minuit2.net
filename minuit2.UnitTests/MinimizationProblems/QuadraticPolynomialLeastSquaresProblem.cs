namespace minuit2.UnitTests.MinimizationProblems;

internal class QuadraticPolynomialLeastSquaresProblem : ConfigurableLeastSquaresProblem
{
    protected override Func<double, IReadOnlyList<double>, double> Model { get; } = (x, c) => c[0] + c[1] * x + c[2] * x * x;
    
    protected override Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient { get; } = (x, _) => [1, x, x * x];
    
    protected override IReadOnlyList<string> ParameterNames { get; } = ["c0", "c1", "c2"];
    
    // The following values are generated using the above model with coefficients c0 = 10, c1 = -5, c2 = 0.5,
    // adding random normal noise with a standard deviation of 0.1
    protected override IReadOnlyList<double> XValues { get; } =
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];
    
    protected override IReadOnlyList<double> YValues { get; } = 
    [
        9.9, 7.59, 5.63, 3.64, 2.09, 0.68, -0.56, -1.32, -2.03, -2.41, -2.49, -2.53, -1.88, -1.44, -0.4, 0.64, 2.15,
        3.56, 5.47, 7.66
    ];

    protected override double YError => 0.1; // standard deviation of noise used to generate the above y-values
    
    // Since the model is linear in its coefficients, the optimal parameter values are fully determined by the
    // closed-form solution of a linear regression (here, rounded to 2-digit precision)
    protected override IReadOnlyList<double> OptimumParameterValues { get; } = [10, -5, 0.5];
    
    protected override IReadOnlyList<double> DefaultInitialParameterValues { get; } = [10.9, -6.06, 0.58];
}