namespace minuit2.net.CostFunctions;

public interface ICostFunction
{
    IReadOnlyList<string> Parameters { get; }
    bool HasGradient { get; }
    bool HasHessian { get; }
    bool HasHessianDiagonal { get; }
    double ErrorDefinition { get; }
    
    double ValueFor(IReadOnlyList<double> parameterValues);
    IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues);
    IReadOnlyList<double> HessianFor(IReadOnlyList<double> parameterValues);
    
    // When available, the Hessian diagonal (referred to as G2 in Minuit2) takes precedence over the full Hessian
    // during minimizer seeding. This not only provides performance benefits but — for reasons not yet understood —
    // also improves robustness. In Minuit2, regularization of the full initial Hessian appears problematic during
    // seeding, whereas regularization of the diagonal works as intended (cf. https://github.com/root-project/root/issues/20665).
    IReadOnlyList<double> HessianDiagonalFor(IReadOnlyList<double> parameterValues);

    ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result);
}