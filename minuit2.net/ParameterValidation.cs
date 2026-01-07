namespace minuit2.net;

internal static class ParameterValidation
{ 
    public static void EnsureUniqueMappingBetween(
        IEnumerable<string> costParameters, 
        IReadOnlyCollection<string> otherParameters, 
        string otherInstance,
        string task)
    {
        var exceptions = new List<Exception>();
        foreach (var name in costParameters)
        {
            var requiresUniqueMatch = $"Cost function parameter '{name}' requires a unique match in the " +
                                      $"{otherInstance} to perform {task}";
            var numberOfMatches = otherParameters.Count(p => p == name);
            if (numberOfMatches == 0)
                exceptions.Add(new ArgumentException($"{requiresUniqueMatch}, but there is none."));
            if (numberOfMatches > 1) 
                exceptions.Add(new ArgumentException($"{requiresUniqueMatch}, but there are {numberOfMatches}."));
        }

        if (exceptions.Count == 1)
            throw exceptions.Single();
        if (exceptions.Count > 1) 
            throw new AggregateException(exceptions);
    }
}