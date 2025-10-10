﻿namespace minuit2.net.CostFunctions;

internal class LeastSquares : ICostFunction
{
    private readonly IReadOnlyList<double> _x;
    private readonly IReadOnlyList<double> _y;
    private readonly IReadOnlyList<double> _yError;
    private readonly Func<double, IReadOnlyList<double>, double> _model;
    private readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? _modelGradient;
    
    public LeastSquares(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y,
        IReadOnlyList<double> yError,
        IReadOnlyList<string> parameters,
        Func<double, IReadOnlyList<double>, double> model,
        Func<double, IReadOnlyList<double>, IReadOnlyList<double>>? modelGradient = null,
        double errorDefinitionInSigma = 1)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
            throw new ArgumentException($"{nameof(x)}, {nameof(y)} and {nameof(yError)} must have the same length");

        _x = x;
        _y = y;
        _yError = yError;
        _model = model;
        _modelGradient = modelGradient;
        
        Parameters = parameters;
        HasGradient = modelGradient != null;
        ErrorDefinition = ErrorDefinitionFor(errorDefinitionInSigma);
    }

    public IReadOnlyList<string> Parameters { get; }
    public bool HasGradient { get; }
    public double ErrorDefinition { get; }
    
    public double ValueFor(IReadOnlyList<double> parameterValues)
    {
        double sum = 0;
        for (var i = 0; i < _x.Count; i++)
        {
            var residual = ResidualFor(i, parameterValues);
            sum += residual * residual;
        }
        
        return sum;
    }

    public IReadOnlyList<double> GradientFor(IReadOnlyList<double> parameterValues)
    {
        var gradientSums = new double[Parameters.Count];
        for (var i = 0; i < _x.Count; i++)
        {
            var factor = 2 * ResidualFor(i, parameterValues);
            var gradients = _modelGradient!(_x[i], parameterValues);
            for (var j = 0; j < Parameters.Count; j++) 
                gradientSums[j] -= factor * gradients[j] / _yError[i];
        }
        
        return gradientSums;
    }

    private double ResidualFor(int i, IReadOnlyList<double> parameterValues) =>
        (_y[i] - _model(_x[i], parameterValues)) / _yError[i];
    
    // For least squares fits, an error definition of 1 corresponds to 1-sigma parameter errors
    // (4 would correspond to 2-sigma errors, 9 would correspond to 3-sigma errors etc.)
    internal static double ErrorDefinitionFor(double errorDefinitionInSigma) =>
        errorDefinitionInSigma * errorDefinitionInSigma;
}