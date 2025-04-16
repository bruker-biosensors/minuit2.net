using System.Diagnostics.CodeAnalysis;
using minuit2.net.wrap;

namespace minuit2.net;

[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable", Justification = 
    """
    The fields for the wrapped cost and the parameter state ensure correct lifetime management of these objects. 
    If turned into local variables, they might be collected by the GC and memory might be freed on the C++ side.
    """)]
public class Migrad
{
    private readonly ICostFunction _costFunction;
    private readonly MnMigradWrap _migrad;
    
    private readonly CostFunctionWrap _wrappedCostFunction;
    private readonly MnUserParameterState _parameterState;
    
    public Migrad(
        ICostFunction costFunction, 
        IReadOnlyCollection<ParameterConfiguration> parameterConfigurations, 
        MinimizationStrategy minimizationStrategy = MinimizationStrategy.Balanced)
    {
        _costFunction = costFunction;
        if (parameterConfigurations.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException($"The {nameof(parameterConfigurations)} must correspond to the " +
                                        $"{nameof(costFunction.Parameters)} defined by the {nameof(costFunction)}");
        
        _wrappedCostFunction = new CostFunctionWrap(costFunction);
        _parameterState = parameterConfigurations.OrderedBy(costFunction.Parameters).AsState();
        var strategy = new MnStrategy((uint)minimizationStrategy);
        _migrad = new MnMigradWrap(_wrappedCostFunction, _parameterState, strategy);
    }

    public MinimizationResult Run()
    {
        var minimum = _migrad.Run();
        var result = new MinimizationResult(minimum, _costFunction.Parameters);
        return _costFunction.Adjusted(result);
    }
}