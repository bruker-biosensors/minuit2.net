namespace minuit2.net;

public class Migrad
{
    private readonly MnMigradWrap migrad;

    //not sure if those are needed,
    //but otherwise they will be collected by the GC and and memory might be freed on the C++ side
    //having this variable here insures correct lifetime management.
    private readonly CostFunctionWrapper wrapper;
    private readonly MnUserParameterState parameters;
    private readonly IList<string> _costFunctionParameters;


    public Migrad(ICostFunction costFunction, UserParameters userParameters)
    {
        _costFunctionParameters = costFunction.Parameters;
        if (userParameters.AreNotMatching(_costFunctionParameters))
            throw new ArgumentException($"The {nameof(userParameters)} must correspond to the {nameof(costFunction.Parameters)} defined by the {nameof(costFunction)}");
        
        wrapper = new CostFunctionWrapper(costFunction);
        parameters = userParameters.OrderedBy(_costFunctionParameters).AsState();
        migrad = new MnMigradWrap(wrapper, parameters);
    }

    public MinimizationResult Run() => new(migrad.Run(), _costFunctionParameters.ToList());
}