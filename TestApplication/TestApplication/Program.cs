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

LeastSquares fcn = new LeastSquares(data_x, data_y, y_err, gaussian);
//new VectorDouble(new double[] { 1, 2 }), new VectorDouble(new double[] { 0.1, 0.1 })
MnUserParameterState state = new MnUserParameterState();
state.Add("mu", 1, 0.1);
state.Add("sigma", 2, 0.1);
Stopwatch sw = Stopwatch.StartNew();
Migrad migrad = new Migrad(fcn, state);
Console.WriteLine($"Migrad took {sw.ElapsedMilliseconds} ms");
var result = migrad.Run();
var array = DoubleArray.frompointer(result.Parameters().Vec().Data());
Console.WriteLine($"{array.getitem(0)} {array.getitem(1)}");

double gaussian(double x, double lmu, double lsigma)
{
    double tobesqured = (x - lmu) / lsigma;
    return 1.0 / (lsigma * Math.Sqrt(2 * Math.PI)) * Math.Exp(-0.5 * tobesqured * tobesqured);
}
