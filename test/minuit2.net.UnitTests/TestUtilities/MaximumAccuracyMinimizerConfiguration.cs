using minuit2.net.Minimizers;

namespace minuit2.net.UnitTests.TestUtilities;

internal record MaximumAccuracyMinimizerConfiguration(Strategy Strategy = Strategy.Balanced)
    : MinimizerConfiguration(Strategy, MaximumFunctionCalls: uint.MaxValue, Tolerance: 0);