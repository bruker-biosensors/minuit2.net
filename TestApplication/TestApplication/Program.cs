// See https://aka.ms/new-console-template for more information


using System.Diagnostics;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using minuit2.net;

double mu = 5;
double sigma = 1;
double y_err = 0.1;
Normal normalDist = new Normal(0, y_err);
List<double> data_x = Generate.LinearRange(0, 0.1, 100).ToList();

List<double> data_y = data_x.Select(x => gaussian(x, mu, sigma) + normalDist.Sample()).ToList();
Stopwatch sw = Stopwatch.StartNew();
for (int i = 0; i < 1000; i++)
{
    data_y = data_x.Select(x => gaussian(x, mu, sigma) + normalDist.Sample()).ToList();

    LeastSquares fcn =
        new LeastSquares(data_x, data_y, y_err, (x, pars) => gaussian(x, pars.ElementAt(0), pars.ElementAt(1)));
//new VectorDouble(new double[] { 1, 2 }), new VectorDouble(new double[] { 0.1, 0.1 })
    var state = new UserParameters(new Parameter("mu", 1), new Parameter("sigma", 2));



    Migrad migrad = new Migrad(fcn, state);

    var result = migrad.Evaluate();
}
Console.WriteLine($"Migrad took {sw.ElapsedMilliseconds} ms");

double gaussian(double x, double lmu, double lsigma)
{
    double tobesqured = (x - lmu) / lsigma;
    return 1.0 / (lsigma * Math.Sqrt(2 * Math.PI)) * Math.Exp(-0.5 * tobesqured * tobesqured);
}
