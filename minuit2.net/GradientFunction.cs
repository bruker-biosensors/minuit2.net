// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace minuit2.net;

public class GradientFunction : FCNGradientBaseWrap
{
    public override VectorDouble Gradient(VectorDouble arg0)
    {
        Console.WriteLine("Test1");
        return arg0;
    }

    public override double Run(VectorDouble v)
    {
        Console.WriteLine("Test2");
        throw new Exception();
    }

    public override double Up()
    {
        Console.WriteLine("Test3");
        return 0.0;
    }
}
