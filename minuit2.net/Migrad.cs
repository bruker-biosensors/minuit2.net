namespace minuit2.net;

public class Migrad
{
    private readonly MnMigradWrap migrad;

    //not sure if those are needed,
    //but otherwise they will be collected by the GC and and memory might be freed on the C++ side
    //having this variable here insures correct lifetime management.
    private readonly CostFunctionWrapper wrapper;
    private readonly MnUserParameterState parameters;


    public Migrad(ICostFunction fcn, UserParameters par)
    {
        wrapper = new CostFunctionWrapper(fcn);
        parameters = par.GetParameterStates();
        migrad = new MnMigradWrap(wrapper, parameters);
    }

    public MigradResult Run() => new(migrad.Run());
}