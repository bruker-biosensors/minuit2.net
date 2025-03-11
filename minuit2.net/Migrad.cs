namespace minuit2.net;

public class Migrad
{
    private readonly ICostFunction _costFunction;
    private readonly MnMigradWrap _migrad;

    //not sure if those are needed,
    //but otherwise they will be collected by the GC and and memory might be freed on the C++ side
    //having this variable here insures correct lifetime management.
    private readonly CostFunctionWrapper wrapper;
    private readonly MnUserParameterState parameters;


    public Migrad(ICostFunction costFunction, UserParameters userParameters)
    {
        _costFunction = costFunction;
        if (userParameters.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException($"The {nameof(userParameters)} must correspond to the {nameof(costFunction.Parameters)} defined by the {nameof(costFunction)}");
        
        wrapper = new CostFunctionWrapper(costFunction);
        parameters = userParameters.OrderedBy(costFunction.Parameters).AsState();
        _migrad = new MnMigradWrap(wrapper, parameters);
    }

    public MinimizationResult Run()
    {
        var migradResult = _migrad.Run();

        if (_costFunction is ILeastSquares leastSquares)
            return new LeastSquaresMinimizationResult(migradResult, leastSquares);
        
        return new MinimizationResult(migradResult, _costFunction);
    }
}