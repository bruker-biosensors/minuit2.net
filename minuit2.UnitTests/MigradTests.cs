using FluentAssertions;
using minuit2.net;

namespace minuit2.UnitTests;

public class MigradTests
{
    [Test]
    public void A_migrad_minimizer_when_constructed_with_mismatching_parameters_throws_an_exception()
    {
        ParameterConfiguration[] userParameters = [new("a", 0), new("b", 0)];
        var costParameters = new[] { "a", "b", "c" };
        var cost = new LeastSquares([0, 1, 2], [0, 1, 2], 1, (_, _) => 0, costParameters);

        var construction = void () => _ = new Migrad(cost, userParameters);
        construction.Should().Throw<ArgumentException>();
    }
}