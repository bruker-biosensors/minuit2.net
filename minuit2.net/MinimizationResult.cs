using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net;

internal class MinimizationResult : IMinimizationResult
{
    internal MinimizationResult(FunctionMinimum minimum, ICostFunction costFunction, double edmThreshold)
    {
        var state = minimum.UserState();
        Parameters = costFunction.Parameters.ToList();
        Variables = VariablesFrom(Parameters, state);
        var parameterValues = state.Params();
        ParameterValues = parameterValues.ToList();
        ParameterCovarianceMatrix = CovarianceMatrixFrom(state);

        CostValue = costFunction is IComposite compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);

        // Meta information
        IsValid = minimum.IsValid();
        NumberOfVariables = (int)state.VariableParameters();
        NumberOfFunctionCalls = minimum.NFcn();
        ExitCondition = ExitConditionFrom(minimum, edmThreshold);
        
        Minimum = minimum;
    }
    
    public double CostValue { get; }

    public IReadOnlyCollection<string> Parameters { get; }
    public IReadOnlyCollection<string> Variables { get; }
    public IReadOnlyCollection<double> ParameterValues { get; }
    public double[,] ParameterCovarianceMatrix { get; private set; }

    // The result is considered valid if the minimizer did not run into any troubles. Reasons for an invalid result are: 
    // - the number of allowed function calls has been exhausted
    // - the minimizer could not improve the values of the parameters (and knowing that it has not converged yet)
    // - a problem with the calculation of the covariance matrix
    // source: https://root.cern.ch/doc/master/Minuit2Page.html
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }
    
    private static List<string> VariablesFrom(IReadOnlyCollection<string> parameters, MnUserParameterState state)
    {
        var numberOfVariables = (int)state.VariableParameters();
        return Enumerable.Range(0, numberOfVariables).Select(VariableName).ToList();

        string VariableName(int variableIndex) => parameters.ElementAt(state.ParameterIndexOf(variableIndex));
    }
    
    private static double[,] CovarianceMatrixFrom(MnUserParameterState state)
    {
        var covariancesOfVariables = state.Covariance().Data();  // can be empty (when covariance calculation fails)
        var numberOfVariables = (int)state.VariableParameters();
        var indexMap = Enumerable.Range(0, numberOfVariables)
            .ToDictionary(state.ParameterIndexOf, variableIndex => variableIndex);
        
        var numberOfParameters = state.Params().Count;
        var covarianceMatrix = new double[numberOfParameters, numberOfParameters];
        for (var i = 0; i < numberOfParameters; i++)
        {
            if (!indexMap.TryGetValue(i, out var rowVariableIndex)) continue;
            for (var j = 0; j < numberOfParameters; j++)
            {
                if (!indexMap.TryGetValue(j, out var columnVariableIndex)) continue;
                var flatIndex = FlatIndex(rowVariableIndex, columnVariableIndex);
                covarianceMatrix[i, j] = covariancesOfVariables.ElementAtOrDefault(flatIndex, double.NaN);
                covarianceMatrix[j, i] = covariancesOfVariables.ElementAtOrDefault(flatIndex, double.NaN);
            }
        }

        return covarianceMatrix;

        int FlatIndex(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }
    
    private static MinimizationExitCondition ExitConditionFrom(FunctionMinimum minimum, double edmThreshold)
    {
        if (minimum.Edm() < edmThreshold)
            return Converged;
        if (minimum.HasReachedCallLimit())
            return FunctionCallsExhausted;
        
        return None;
    }
    
    internal FunctionMinimum Minimum { get; }

    internal void UpdateParameterCovariancesWith(FunctionMinimum minimum) =>
        ParameterCovarianceMatrix = CovarianceMatrixFrom(minimum.UserState());
}

file static class UserStateExtensions
{
    public static int ParameterIndexOf(this MnUserParameterState state, int variableIndex) =>
        (int)state.ExtOfInt((uint)variableIndex);
}