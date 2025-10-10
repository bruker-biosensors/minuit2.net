using System.Diagnostics.CodeAnalysis;

namespace minuit2.net;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used implicitly in tests")]
public enum Strategy
{
    Fast,
    Balanced,
    Rigorous, 
    VeryRigorous
}

internal static class StrategyExtensions
{
    public static MnStrategy AsMnStrategy(this Strategy strategy) => new((uint)strategy);
}