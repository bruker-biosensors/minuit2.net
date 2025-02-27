// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace minuit2.net;

public class LeastSquares(List<double> x, List<double> y, double y_err, Func<double,double,double,double> model) :FCNWrap
{
    public override double Run(VectorDouble v)
    {
        double f = 0.0;

        foreach(var data in x.Zip(y))
        {
            double diff = data.Second - model(data.First, v.ElementAt(0), v.ElementAt(1));
            f += diff * diff;
        }
        f /= x.Count();
        return f;
    }

    public override double Up()
    {
        return 1; // according to the documentation this is the default value for chisquaredfits
    }
}
