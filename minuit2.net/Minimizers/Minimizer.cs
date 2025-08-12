namespace minuit2.net.Minimizers;

public static class Minimizer
{
    public static IMinimizer Migrad() => new MigradMinimizer();
}