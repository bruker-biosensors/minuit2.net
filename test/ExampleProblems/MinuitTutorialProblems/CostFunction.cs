using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems.MinuitTutorialProblems;

internal class CostFunction(
    IReadOnlyList<string> parameters,
    Func<IReadOnlyList<double>, double> value, 
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? gradient = null, 
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? hessian = null, 
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? hessianDiagonal = null, 
    double errorDefinition = 1) 
    : ICostFunction
{
    public IReadOnlyList<string> Parameters { get; } = parameters;
    public bool HasGradient { get; } = gradient != null;
    public bool HasHessian { get; } = hessian != null;
    public bool HasHessianDiagonal { get; } = hessianDiagonal != null;
    public double ErrorDefinition { get; } = errorDefinition;
    public double ValueFor(IReadOnlyList<double> parameterValues) => value(parameterValues);
    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues) => gradient!(parameterValues);
    public IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues) => hessian!(parameterValues);
    public IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues) => hessianDiagonal!(parameterValues);
    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => this;
}