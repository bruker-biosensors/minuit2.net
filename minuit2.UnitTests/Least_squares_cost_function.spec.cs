using AwesomeAssertions;
using minuit2.net.CostFunctions;
using minuit2.UnitTests.TestUtilities;
using static minuit2.net.CostFunctions.CostFunction;

namespace minuit2.UnitTests;

public class A_least_squares_cost_function
{
    private static int AnyCount(int min = 10, int max = 100) => Any.Integer().Between(min, max);
    private static List<double> AnyValues(int count) => Enumerable.Range(0, count).Select(_ => (double)Any.Double()).ToList();
    
    [Test]
    public void when_constructed_with_mismatching_numbers_of_x_and_y_values_throws_an_exception(
        [Values(-1, 1)] int countBiasDirection)
    {
        var xCount = AnyCount(10, 50);
        var yCount = xCount + countBiasDirection * AnyCount(1, 10);
        
        Action action = () => _ = LeastSquares(AnyValues(xCount), AnyValues(yCount), [], (_, _) => 0);
        
        action.Should().Throw<ArgumentException>();
    }
    
    [Test]
    public void when_constructed_with_a_collection_of_y_errors_mismatching_the_number_of_y_values_throws_an_exception(
        [Values(-1, 1)] int countBiasDirection)
    {
        var valueCount = AnyCount(10, 50);
        var errorCount = valueCount + countBiasDirection * AnyCount(1, 10);
        
        Action action = () => _ = LeastSquares(AnyValues(valueCount), AnyValues(valueCount), AnyValues(errorCount), [], (_, _) => 0);
        
        action.Should().Throw<ArgumentException>();
    }

    private static IEnumerable<TestCaseData> ConstantLevelCostFunctionWithUniformYErrorTestCases()
    {
        var valueCount = AnyCount();
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        double yError = Any.Double();
        
        yield return Case(LeastSquares(xValues, yValues, yError, ["level"], (_, p) => p[0]));
        yield return Case(LeastSquares(xValues, yValues, yError, ["level"], (_, p) => Enumerable.Repeat(p[0], valueCount).ToArray()));
        yield break;

        TestCaseData Case(ICostFunction cost) => new(cost, yValues, yError);
    }
    
    [TestCaseSource(nameof(ConstantLevelCostFunctionWithUniformYErrorTestCases))]
    public void with_a_uniform_y_error_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals(
        ICostFunction cost, 
        IReadOnlyList<double> yValues, 
        double yError)
    {
        var constantModelLevel = Any.Double();
        
        var expectedValue = yValues
            .Select(y => (y - constantModelLevel) / yError)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
    
    private static IEnumerable<TestCaseData> ConstantLevelCostFunctionWithIndividualYErrorsTestCases()
    {
        var valueCount = AnyCount();
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);
        var yErrors = AnyValues(valueCount);

        yield return Case(LeastSquares(xValues, yValues, yErrors, ["level"], (_, p) => p[0]));
        yield return Case(LeastSquares(xValues, yValues, yErrors, ["level"], (_, p) => Enumerable.Repeat(p[0], valueCount).ToArray()));
        yield break;

        TestCaseData Case(ICostFunction cost) => new(cost, yValues, yErrors);
    }
    
    [TestCaseSource(nameof(ConstantLevelCostFunctionWithIndividualYErrorsTestCases))]
    public void with_individual_y_errors_when_asked_for_its_cost_value_returns_the_sum_of_squared_error_weighted_residuals(
        ICostFunction cost, 
        IReadOnlyList<double> yValues, 
        IReadOnlyList<double> yErrors)
    {
        var constantModelLevel = Any.Double();
        
        var expectedValue = yValues
                .Zip(yErrors, (y, yError) => (y - constantModelLevel) / yError)
                .Select(r => r * r)
                .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }
    
    private static IEnumerable<TestCaseData> ConstantLevelCostFunctionWithUnknownYErrorTestCases()
    {
        var valueCount = AnyCount();
        var xValues = AnyValues(valueCount);
        var yValues = AnyValues(valueCount);

        yield return Case(LeastSquares(xValues, yValues, ["level"], (_, p) => p[0]));
        yield return Case(LeastSquares(xValues, yValues, ["level"], (_, p) => Enumerable.Repeat(p[0], valueCount).ToArray()));
        yield break;

        TestCaseData Case(ICostFunction cost) => new(cost, yValues);
    }

    [TestCaseSource(nameof(ConstantLevelCostFunctionWithUnknownYErrorTestCases))]
    public void without_y_errors_when_asked_for_its_cost_value_returns_the_sum_of_squared_unweighted_residuals(
        ICostFunction cost, 
        IReadOnlyList<double> yValues)
    {
        var constantModelLevel = Any.Double();
        
        var expectedValue = yValues
            .Select(y => y - constantModelLevel)
            .Select(r => r * r)
            .Sum();
        cost.ValueFor([constantModelLevel]).Should().Be(expectedValue);
    }

    [Test]
    public void has_a_default_error_definition_of_one()
    {
        var cost = LeastSquares(x: AnyValues(10), y: AnyValues(10), parameters: [], model: (_, _) => 0);
        
        cost.ErrorDefinition.Should().Be(1);
    }
    
    [Test]
    public void with_a_custom_error_definition_in_terms_of_sigma_uses_the_square_of_that_value_for_its_absolute_error_definition()
    {
        var errorDefinitionInSigma = Any.Double().Between(2, 5);
        
        var cost = LeastSquares(
            x: AnyValues(10), 
            y: AnyValues(10), 
            parameters: [], 
            model: (_, _) => 0,
            errorDefinitionInSigma: errorDefinitionInSigma);
        
        cost.ErrorDefinition.Should().Be(errorDefinitionInSigma * errorDefinitionInSigma);
    }
}