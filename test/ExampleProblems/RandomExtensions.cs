namespace ExampleProblems;

internal static class RandomExtensions
{
    public static double NextNormal(this Random random, double mean, double standardDeviation)
    {
        // Box-Muller transform
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var randomStandardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mean + standardDeviation * randomStandardNormal;
    }
}