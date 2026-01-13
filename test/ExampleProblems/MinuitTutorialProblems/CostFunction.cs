using minuit2.net;
using minuit2.net.CostFunctions;

namespace ExampleProblems.MinuitTutorialProblems;

internal class CostFunction(
    IReadOnlyList<string> parameters,
    Func<IReadOnlyList<double>, double> valueFunction, 
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? gradientFunction = null, 
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? hessianFunction = null, 
    Func<IReadOnlyList<double>, IReadOnlyList<double>>? hessianDiagonalFunction = null, 
    double errorDefinition = 1) 
    : ICostFunction
{
    public IReadOnlyList<string> Parameters { get; } = parameters;
    public bool HasGradient { get; } = gradientFunction != null;
    public bool HasHessian { get; } = hessianFunction != null;
    public bool HasHessianDiagonal { get; } = hessianDiagonalFunction != null;
    public double ErrorDefinition { get; } = errorDefinition;
    public double ValueFor(IReadOnlyList<double> parameterValues) => valueFunction(parameterValues);
    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues) => gradientFunction!(parameterValues);
    public IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues) => hessianFunction!(parameterValues);
    public IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues) => hessianDiagonalFunction!(parameterValues);
    public ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => this;
}