using minuit2.net.Minimizers;

namespace minuit2.UnitTests.TestUtilities;

internal record MaximumAccuracyMinimizerConfiguration(net.Strategy Strategy = net.Strategy.Balanced)
    : MinimizerConfiguration(Strategy, MaximumFunctionCalls: uint.MaxValue, Tolerance: 0);