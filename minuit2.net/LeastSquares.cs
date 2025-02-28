// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace minuit2.net;

public class LeastSquares : FCNWrap
{
    private readonly List<double> yErrSquared;
    private readonly List<double> xData;
    private readonly List<double> yData;
    private readonly Func<double, IList<double>, double> model;

    public LeastSquares(List<double> x, List<double> y, List<double> yError, Func<double, IList<double>, double> model)
    {
        if (x.Count != y.Count || x.Count != yError.Count)
        {
            throw new ArgumentException("x, y and yError must have the same length");
        }
        this.yErrSquared = yError;
        this.xData = x;
        this.yData = y;
        this.model = model;
    }

    public LeastSquares(List<double> x, List<double> y, double yError, Func<double, IList<double>, double> model)
        : this(x, y, Enumerable.Range(0, y.Count()).Select(_ => yError * yError).ToList(), model)
    {
    }

    public override double Run(VectorDouble v)
    {
        double f = 0.0;

        foreach (var data in xData.Zip(yData, yErrSquared))
        {
            double diff = data.Second - model(data.First, v);
            f += diff * diff / data.Third;
        }

        return f;
    }

    public override double Up()
    {
        return 1; // according to the documentation this is the default value for chisquaredfits
    }
}
