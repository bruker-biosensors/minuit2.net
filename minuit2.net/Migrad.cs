// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace minuit2.net;

public class Migrad : MnMigradWrap
{
    public Migrad(FCNWrap fcn, MnUserParameterState par, MnStrategy str) : base(fcn, par, str)
    {
    }

    public Migrad(FCNWrap fcn, MnUserParameterState par) : base(fcn, par)
    {
    }
}
