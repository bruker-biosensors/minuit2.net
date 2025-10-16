using AwesomeAssertions;
using minuit2.net;
using minuit2.net.Minimizers;
using minuit2.UnitTests.MinimizationProblems;
using minuit2.UnitTests.TestUtilities;

namespace minuit2.UnitTests;

[TestFixture]
public class The_combined_minimizer() : Any_parameter_uncertainty_resolving_minimizer(CombinedMinimizer)
{
    private static readonly IMinimizer CombinedMinimizer = Minimizer.Combined;

    [TestCaseSource(nameof(WellPosedMinimizationProblems))]
    [Description("The combined minimizer leverages Migrad, and only temporarily switches to the Simplex method when " +
                 "Migrad runs into problems (see Minuit documentation). Therefore, for well-posed problems, where " +
                 "Migrad shouldn't face issues, there should be no difference between results obtained by Migrad and " +
                 "the combined minimizer.")]
    public void when_minimizing_a_well_posed_problem_yields_the_same_result_as_the_migrad_minimizer(
        ConfiguredProblem problem,
        Strategy strategy)
    {
        var result = CombinedMinimizer.Minimize(problem.Cost, problem.ParameterConfigurations);
        var migradResult = Minimizer.Migrad.Minimize(problem.Cost, problem.ParameterConfigurations);

        result.Should().BeEquivalentTo(migradResult, options => options.WithRelativeDoubleTolerance(0.001));
    }
}
