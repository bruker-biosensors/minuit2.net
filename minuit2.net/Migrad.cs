namespace minuit2.net;

public class Migrad
{
    private readonly MnMigradWrap migrad;

    //not sure if those are needed,
    //but otherwise they will be collected by the GC and and memory might be freed on the C++ side
    //having this variable here insures correct lifetime management.
    private readonly CostFunctionWrapper wrapper;
    private readonly MnUserParameterState parameters;


    public Migrad(ICostFunction costFunction, UserParameters userParameters)
    {
        if (userParameters.AreNotMatching(costFunction.Parameters))
            throw new ArgumentException($"{nameof(userParameters)} must match the {nameof(costFunction.Parameters)}");
        
        wrapper = new CostFunctionWrapper(costFunction);
        parameters = userParameters.OrderedBy(costFunction.Parameters).AsState();
        migrad = new MnMigradWrap(wrapper, parameters);
    }

    public MinimizationResult Run() => new(migrad.Run());
}