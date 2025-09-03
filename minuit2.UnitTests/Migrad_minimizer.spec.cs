using minuit2.net.Minimizers;

namespace minuit2.UnitTests;

[TestFixture]
public class The_migrad_minimizer() : Any_parameter_uncertainty_resolving_minimizer(Minimizer.Migrad);