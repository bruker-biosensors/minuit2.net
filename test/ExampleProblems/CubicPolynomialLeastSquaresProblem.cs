namespace ExampleProblems;

public class CubicPolynomialLeastSquaresProblem : ConfigurableLeastSquaresProblem
{
    protected override Func<double, IReadOnlyList<double>, double> Model { get; } =
        (x, c) => c[0] + c[1] * x + c[2] * x * x + c[3] * x * x * x;
    
    protected override Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient { get; } =
        (x, _) => [1, x, x * x, x * x * x];

    protected override Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian { get; } =
        (_, _) => Enumerable.Repeat(0d, 4 * 4).ToArray();

    protected override IReadOnlyList<string> ParameterNames { get; } = ["c0", "c1", "c2", "c3"];
    
    // The following values are generated using the above model with coefficients c0 = 10, c1 = -2, c2 = 1, c3 = -0.1,
    // adding random normal noise with a standard deviation of 0.1
    protected override IReadOnlyList<double> XValues { get; } =
    [
        0, 0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5
    ];
    
    protected override IReadOnlyList<double> YValues { get; } = 
    [
        9.9, 9.2, 9.03, 8.93, 9.29, 9.75, 10.24, 11.02, 11.57, 12.11, 12.51, 12.46, 12.52, 11.72, 10.8, 9.08, 6.95,
        3.77, 0.07, -4.45
    ];

    protected override double YError => 0.1; // standard deviation of noise used to generate the above y-values
    
    // Since the model is linear in its coefficients, the optimal parameter values are fully determined by the
    // closed-form solution of a linear regression (here, rounded to 2-digit precision)
    protected override IReadOnlyList<double> OptimumParameterValues { get; } = [9.97, -1.96, 0.99, -0.1];

    protected override IReadOnlyList<double> DefaultInitialParameterValues { get; } = [10.75, -1.97, 1.13, -0.11];
}