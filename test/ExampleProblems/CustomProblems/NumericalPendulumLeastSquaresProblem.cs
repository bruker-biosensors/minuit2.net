using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems.CustomProblems;

public class NumericalPendulumLeastSquaresProblem : IConfiguredProblem
{
    public NumericalPendulumLeastSquaresProblem()
    {
        OptimumParameterValues = [1];
        ParameterConfigurations = [ParameterConfiguration.Variable("pendulumLength", 2)];

        var model = NumericalPendulumModelFor(startAngle: 1.5, startAngleVelocity: 0.0);
        var xValues = LinearlySpacedValues(0, 3, 0.001);
        var yValues = model(xValues, OptimumParameterValues.ToArray());
        Cost = CostFunction.LeastSquares(xValues, yValues, ["pendulumLength"], model);
    }

    public ICostFunction Cost { get; }
    public IReadOnlyCollection<double> OptimumParameterValues { get; }
    public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }

    private static Func<IReadOnlyList<double>, IReadOnlyList<double>, IReadOnlyList<double>> NumericalPendulumModelFor(
        double startAngle,
        double startAngleVelocity)
    {
        return (x, p) =>
        {
            const double acceleration = 9.81; // standard gravity (m/s2)
            var deltaX = x.Zip(x.Skip(1), (lowerX, upperX) => upperX - lowerX).ToArray();
            var angle = Enumerable.Repeat(startAngle, x.Count).ToArray();
            var angleVelocity = Enumerable.Repeat(startAngleVelocity, x.Count).ToArray();
            for (var i = 0; i < deltaX.Length; i++)
            {
                // ODE system (see e.g. https://de.wikipedia.org/wiki/Mathematisches_Pendel) solved using Euler method
                angle[i + 1] = angle[i] + angleVelocity[i] * deltaX[i];
                angleVelocity[i + 1] = angleVelocity[i] - acceleration / p[0] * Math.Sin(angle[i]) * deltaX[i];
            }

            return angle;
        };
    }

    private static double[] LinearlySpacedValues(double start, double end, double step)
    {
        var number = (int)((end - start) / step) + 1;
        return Enumerable.Range(0, number).Select(i => start + i * step).ToArray();
    }
}