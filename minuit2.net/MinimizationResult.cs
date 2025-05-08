using static minuit2.net.MinimizationExitCondition;

namespace minuit2.net;

internal class MinimizationResult : IMinimizationResult
{
    internal MinimizationResult(FunctionMinimum functionMinimum, ICostFunction costFunction, double tolerance)
    {
        var state = functionMinimum.UserState();
        Parameters = costFunction.Parameters.ToList();
        var parameterValues = state.Params();
        ParameterValues = parameterValues.ToList();
        ParameterCovarianceMatrix = CovarianceMatrixFrom(state);

        CostValue = costFunction is ICompositeCostFunction compositeCostFunction
            ? compositeCostFunction.CompositeValueFor(parameterValues)
            : costFunction.ValueFor(parameterValues);

        // Meta information
        IsValid = functionMinimum.IsValid();
        NumberOfVariables = (int)state.VariableParameters();
        NumberOfFunctionCalls = functionMinimum.NFcn();
        ExitCondition = ExitConditionFrom(functionMinimum, tolerance);

        Variables = Enumerable.Range(0, NumberOfVariables).Select(var => Parameters.ElementAt(state.ParameterIndexOf(var))).ToList();
        
        FunctionMinimum = functionMinimum;
        Tolerance = tolerance;
    }
    
    public double CostValue { get; }

    public IReadOnlyCollection<string> Parameters { get; }
    public IReadOnlyCollection<string> Variables { get; }
    public IReadOnlyCollection<double> ParameterValues { get; }
    public double[,] ParameterCovarianceMatrix { get; }

    // The result is considered valid if the minimizer did not run into any troubles. Reasons for an invalid result are: 
    // - the number of allowed function calls has been exhausted
    // - the minimizer could not improve the values of the parameters (and knowing that it has not converged yet)
    // - a problem with the calculation of the covariance matrix
    // source: https://root.cern.ch/doc/master/Minuit2Page.html
    public bool IsValid { get; }
    public int NumberOfVariables { get; }
    public int NumberOfFunctionCalls { get; }
    public MinimizationExitCondition ExitCondition { get; }

    private static double[,] CovarianceMatrixFrom(MnUserParameterState state)
    {
        var covariance = state.Covariance();
        var covarianceValues = covariance.Data();
        
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
                covarianceMatrix[i, j] = covarianceValues[flatIndex];
                covarianceMatrix[j, i] = covarianceValues[flatIndex];
            }
        }

        return covarianceMatrix;

        int FlatIndex(int rowIndex, int columnIndex) => rowIndex * (rowIndex + 1) / 2 + columnIndex;
    }
    
    private static MinimizationExitCondition ExitConditionFrom(FunctionMinimum functionMinimum, double tolerance)
    {
        if (functionMinimum.HasConvergedFor(tolerance))
            return Converged;
        if (functionMinimum.HasReachedCallLimit())
            return FunctionCallsExhausted;
        
        return None;
    }
    
    internal FunctionMinimum FunctionMinimum { get; }
    internal double Tolerance { get; }
}

file static class UserStateExtensions
{
    public static int ParameterIndexOf(this MnUserParameterState state, int variableIndex) =>
        (int)state.ExtOfInt((uint)variableIndex);
}

file static class FunctionMinimumExtensions
{
    public static bool HasConvergedFor(this FunctionMinimum minimum, double tolerance)
    {
        // TODO: Add detailed comment
        // - formula: reference to manual
        // - additional factor of 2: reference to posting in official channel
        // - ignoring IsAboveMaxEdm: obviously not a convergence indicator although iminuit docs say it should be
        var edmThreshold = 0.002 * tolerance * minimum.Up();
        return minimum.Edm() < edmThreshold;
    }
}