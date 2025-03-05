namespace minuit2.net;

public class MigradResult
{
    internal MigradResult(FunctionMinimum functionMinimum)
    {
        var retList = new List<double>();
        var doublearray = DoubleArray.frompointer(functionMinimum.Parameters().Vec().Data());
        for (uint i = 0; i < functionMinimum.Parameters().Vec().size(); i++)
        {
            retList.Add(doublearray.getitem(i));
        }

        BestValues = retList;
    }
    
    public IReadOnlyCollection<double> BestValues { get; }
}