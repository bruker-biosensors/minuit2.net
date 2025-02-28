// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace minuit2.UnitTests;

public class ExceptionThrowingFcn : FCNWrap
{
    public override double Cost(VectorDouble v)
    {
        throw new Exception("test exception");
    }
}
