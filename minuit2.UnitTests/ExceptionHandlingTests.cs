using FluentAssertions;
using minuit2.net;

namespace minuit2.UnitTests;

public class ExceptionHandlingTests
{
    private static Func<double, IList<double>, double> ModelFunctionThrowing(Exception exception) =>
        (x, _) => x > 0 ? throw exception : 0;
    
    [Test]
    public void The_minimization_forwards_exceptions_thrown_by_the_model_function()
    {
        var cost = new LeastSquares([1], [2], 3, ModelFunctionThrowing(new TestException("Test message")), []);
        var minimizer = new Migrad(cost, new UserParameters());
        minimizer.Invoking(m => m.Run()).Should().ThrowExactly<TestException>().WithMessage("Test message");
    }

    private class TestException(string message) : Exception(message);
}




