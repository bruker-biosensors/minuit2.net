namespace minuit2.net.Minimizers;

public static class Minimizer
{
    public static IMinimizer Migrad { get; } = new MigradMinimizer();
    public static IMinimizer Simplex { get; } = new SimplexMinimizer();
    public static IMinimizer Combined { get; } = new CombinedMinimizer();
}