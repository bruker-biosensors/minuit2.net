namespace minuit2.net.CostFunctions;

internal class LeastSquaresWithUniformYError : ICostFunction
{
    protected readonly IReadOnlyList<double> X;
    protected readonly IReadOnlyList<double> Y;
    private readonly double _yError;
    protected readonly Func<double, IReadOnlyList<double>, double> Model;
    protected readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? ModelGradient;

    public LeastSquaresWithUniformYError(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        double yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        if (x.Count != y.Count)
            throw new ArgumentException($"{nameof(x)} and {nameof(y)} must have the same length");
        
        X = x;
        Y = y;
        _yError = yError;
        Model = model;
        ModelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = LeastSquares.ErrorDefinitionFor(errorDefinitionInSigma);
    }
    
    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; protected init; }
    
    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < X.Count; i++)
        {
            var residual = ResidualFor(i, parameterValues);
            sum += residual * residual;
        }
        
        return sum;
    }
    
    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < X.Count; i++)
        {
            var factor = 2 * ResidualFor(i, parameterValues);
            var gradients = ModelGradient!(X[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= factor * gradients[j] / _yError;
        }
        
        return gradientSums;
    }

    public virtual ICostFunction WithErrorDefinitionRecalculatedBasedOnValid(IMinimizationResult result) => this;

    private double ResidualFor(int i, IReadOnlyList<double> parameterValues) =>
        (Y[i] - Model(X[i], parameterValues)) / _yError;
}