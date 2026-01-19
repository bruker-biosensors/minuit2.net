namespace ExampleProblems;

public static class Values
{
    public static double[] LinearlySpacedBetween(double start, double end, double step)
    {
        var number = (int)((end - start) / step) + 1;
        return LinearlySpacedBetween(start, end, number);
    }
    
    private static double[] LinearlySpacedBetween(double start, double end, int number)
    {
        var step = (end - start) / (number - 1);
        return Enumerable.Range(0, number).Select(i => start + i * step).ToArray();
    }

    public static double[] LogarithmicallySpacedBetween(double start, double end, int number)
    {
        var logValues = LinearlySpacedBetween(Math.Log(start), Math.Log(end), number);
        return logValues.Select(Math.Exp).ToArray();
    }
}