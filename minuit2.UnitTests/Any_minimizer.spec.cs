using FluentAssertions;
using minuit2.net;
using minuit2.net.CostFunctions;
using minuit2.net.Minimizers;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.ParameterConfiguration;

namespace minuit2.UnitTests;

public abstract class Any_minimizer(IMinimizer minimizer)
{
    private static IEnumerable<TestCaseData> MismatchingParameterConfigurationTestCases()
    {
        var cost = Any.InstanceOf<ICostFunction>();

        var mismatchingNames = cost.Parameters.Select(name => ParameterConfiguration(name + Any.String()));
        yield return new TestCaseData(cost, mismatchingNames).SetName("Mismatching parameter names");

        var tooFew = cost.Parameters.Skip(1).Select(ParameterConfiguration);
        yield return new TestCaseData(cost, tooFew).SetName("Too few parameter configurations");

        var tooMany = cost.Parameters.Select(ParameterConfiguration).Concat([ParameterConfiguration(Any.String())]);
        yield return new TestCaseData(cost, tooMany).SetName("Too many parameter configurations");
    }
    
    private static ParameterConfiguration ParameterConfiguration(string name) => Variable(name, Any.Double());
    
    [TestCaseSource(nameof(MismatchingParameterConfigurationTestCases))]
    public void when_asked_to_minimize_a_cost_function_with_mismatching_parameter_configurations_throws_an_exception(
        ICostFunction cost, IEnumerable<ParameterConfiguration> mismatchingParameterConfigurations)
    {
        Action action = () => _ = minimizer.Minimize(cost, mismatchingParameterConfigurations.ToList());
        action.Should().Throw<ArgumentException>();
    }
    
    [Test, Description("Ensures correct parameter-configuration-to-cost-function-parameter mapping.")]
    public void when_minimizing_a_cost_function_yields_the_same_result_independent_of_the_order_parameter_configurations_are_provided_in()
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var orderedConfigurations = CubicPolynomial.ParameterConfigurations.Defaults; 
        var disorderedConfigurations = CubicPolynomial.ParameterConfigurations.Defaults.InRandomOrder().ToArray();
        
        var resultForOrderedConfigurations = minimizer.Minimize(cost, orderedConfigurations);
        var resultForDisorderedConfigurations = minimizer.Minimize(cost, disorderedConfigurations);
        
        resultForDisorderedConfigurations.Should().BeEquivalentTo(resultForOrderedConfigurations);
    }
    
    [TestCase(double.NegativeInfinity, double.PositiveInfinity)]
    [TestCase(double.NaN, double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity, double.NaN)]
    [TestCase(double.NaN, double.NaN)]
    [Description("Ensures minimizer handles infinite parameter limits the same way as if there were no limits.")]
    public void when_minimizing_a_cost_function_yields_the_same_result_for_unlimited_parameters_and_parameters_with_infinite_limits(
        double lowerLimit, double upperLimit)
    {
        var cost = CubicPolynomial.LeastSquaresCost.Build();
        var unlimitedParameterConfigurations = CubicPolynomial.ParameterConfigurations.Defaults;
        var parameterConfigurationsWithInfiniteLimits = new[]
        {
            CubicPolynomial.ParameterConfigurations.C0.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C1.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C2.WithLimits(lowerLimit, upperLimit),
            CubicPolynomial.ParameterConfigurations.C3.WithLimits(lowerLimit, upperLimit),
        };

        var resultForUnlimited = minimizer.Minimize(cost, unlimitedParameterConfigurations);
        var resultForInfiniteLimits = minimizer.Minimize(cost, parameterConfigurationsWithInfiniteLimits);
        
        resultForInfiniteLimits.Should().HaveIsValid(true).And.BeEquivalentTo(resultForUnlimited);
    }
}