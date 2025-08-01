namespace minuit2.net;

public enum Strategy
{
    Fast,
    Balanced,
    Precise
}

internal static class StrategyExtensions
{
    public static MnStrategy AsMnStrategy(this Strategy strategy) => new((uint)strategy);
}