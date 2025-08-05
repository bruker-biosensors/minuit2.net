namespace minuit2.net.Minimizers;

public static class Minimizer
{
    public static IMinimizer Migrad() => new MigradMinimizer();
    public static IMinimizer Simplex() => new SimplexMinimizer();
    public static IMinimizer Combined() => new CombinedMinimizer();
}