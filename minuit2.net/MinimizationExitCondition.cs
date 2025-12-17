namespace minuit2.net;

public enum MinimizationExitCondition
{
    None,
    Converged,
    FunctionCallsExhausted,
    ManuallyStopped,
    NonFiniteValue,
    NonFiniteGradient,
    NonFiniteHessian,
    NonFiniteHessianDiagonal
}