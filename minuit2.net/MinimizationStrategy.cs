namespace minuit2.net;

public enum MinimizationStrategy
{
    Fast,
    Balanced,
    Precise
}

internal static class MinimizationStrategyExtensions
{
    public static MnStrategy ToMnStrategy(this MinimizationStrategy strategy) => new((uint)strategy);
}