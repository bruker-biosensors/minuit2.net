namespace minuit2.net.costFunctions;

public class LeastSquaresWithUniformYError : ICostFunction
{
    protected readonly IList<double> X;
    protected readonly IList<double> Y;
    private readonly double _yError;
    protected readonly Func<double, IList<double>, double> Model;
    protected readonly Func<double, IList<double>, IList<double>>? ModelGradient;

    public LeastSquaresWithUniformYError(
        IList<double> x,
        IList<double> y,
        double yError,
        IList<string> parameters,
        Func<double, IList<double>, double> model,
        Func<double, IList<double>, IList<double>>? modelGradient = null)
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
        ErrorDefinition = LeastSquares.OneSigmaErrorDefinition;
    }
    
    public IList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; protected init; }
    
    public double ValueFor(IList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < X.Count; i++)
        {
            var residual = ResidualFor(i, parameterValues);
            sum += residual * residual;
        }
        
        return sum;
    }
    
    public IList<double> GradientFor(IList<double> parameterValues)
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
    
    private double ResidualFor(int i, IList<double> parameterValues) => (Y[i] - Model(X[i], parameterValues)) / _yError;
}