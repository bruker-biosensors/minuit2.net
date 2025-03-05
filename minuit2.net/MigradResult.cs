namespace minuit2.net;

public class MigradResult
{
    internal MigradResult(FunctionMinimum functionMinimum)
    {
        BestValues = BestValuesFrom(functionMinimum);
    }

    private static List<double> BestValuesFrom(FunctionMinimum functionMinimum) =>
        functionMinimum.UserParameters().Params().ToList();

    public IReadOnlyCollection<double> BestValues { get; }
}
