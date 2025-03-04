// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace minuit2.net;

public record Parameter(string Name, double Value);

public class UserParameters(params Parameter[] parameters)
{
    internal MnUserParameterState GetParameterStates()
    {
        var states = new MnUserParameterState();
        foreach (var parameter in parameters)
        {
            states.Add(parameter.Name, parameter.Value, parameter.Value * 0.01);
        }
        return states;
    }
}
