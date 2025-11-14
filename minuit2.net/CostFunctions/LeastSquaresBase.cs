namespace minuit2.net.CostFunctions;

internal abstract class LeastSquaresBase : ICostFunction
{
    protected readonly IReadOnlyList<double> X;
    protected readonly IReadOnlyList<double> Y;
    protected readonly Func<int, double> YErrorForIndex;
    protected readonly Func<double, IReadOnlyList<double>, double> Model;
    protected readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? ModelGradient;
    protected readonly double ErrorDefinitionInSigma;

    protected LeastSquaresBase(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        Func<int, double> yErrorForIndex,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient,
        double errorDefinitionInSigma,
        double errorDefinitionScaling = 1)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");
        
        X = x;
        Y = y;
        YErrorForIndex = yErrorForIndex;
        Model = model;
        ModelGradient = modelGradient;
        ErrorDefinitionInSigma = errorDefinitionInSigma;

        Parameters = parameters;
        HasGradient = modelGradient != null;
        
        // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
        // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
        ErrorDefinition = errorDefinitionInSigma * errorDefinitionInSigma * errorDefinitionScaling;
    }

    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }

    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < X.Count; i++)
        {
            var residual = ResidualFor(parameterValues, i);
            sum += residual * residual;
        }

        return sum;
    }
    
    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < X.Count; i++)
        {
            var residual = ResidualFor(parameterValues, i);
            var gradients = ModelGradient!(X[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= 2 * residual * gradients[j] / YErrorForIndex(i);
        }

        return gradientSums;
    }
    
    private double ResidualFor(IReadOnlyList<double> parameterValues, int index) =>
        (Y[index] - Model(X[index], parameterValues)) / YErrorForIndex(index);
    
    public virtual ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => this;
}