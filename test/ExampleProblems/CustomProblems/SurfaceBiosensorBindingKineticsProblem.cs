using minuit2.net;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class SurfaceBiosensorBindingKineticsProblem(
    ParameterConfiguration? amplitude = null,
    ParameterConfiguration? associationRate = null,
    ParameterConfiguration? analyteConcentration = null,
    ParameterConfiguration? dissociationRate = null,
    ParameterConfiguration? dissociationStart = null,
    DerivativeConfiguration modelDerivativeConfiguration = WithoutDerivatives,
    double actualAnalyteConcentrationInNanoMolar = 10,
    string? localParameterSuffix = null,
    int randomSeed = 0)
    : AnalyticalLeastSquaresProblem(
        XValues,
        YValuesFor(actualAnalyteConcentrationInNanoMolar, randomSeed),
        yError: null,  // we pretend we don't know the noise level
        ConfigurationsFor(amplitude,
            associationRate,
            analyteConcentration,
            dissociationRate,
            dissociationStart,
            actualAnalyteConcentrationInNanoMolar,
            localParameterSuffix, randomSeed),
        OptimumValuesFor(actualAnalyteConcentrationInNanoMolar),
        Model,
        ModelGradient,
        ModelHessian,
        ModelHessianDiagonal,
        modelDerivativeConfiguration)
{
    // Assumptions:
    // - ligand is immobilized on surface with surface density remaining constant
    // - analyte association kinetics (starting at t = 0) is directly followed by analyte dissociation kinetics
    // - analyte concentration is 0 during the dissociation measurement (no rebinding)
    // - analyte concentration remains constant during the association measurement
    // - analyte mobility is no restricting factor for the association kinetics (no mass transport limitations)
    // (e.g. cf. https://www.sprpages.nl/data-fitting/kinetic-models/one-to-one)
    
    public static IConfiguredProblem Global(
        IEnumerable<double> analyteConcentrationsInNanoMolar,
        DerivativeConfiguration modelDerivativeConfiguration = WithoutDerivatives,
        int randomSeed = 0)
    {
        var problems = analyteConcentrationsInNanoMolar.Select((conc, index) =>
            new SurfaceBiosensorBindingKineticsProblem(
                actualAnalyteConcentrationInNanoMolar: conc,
                modelDerivativeConfiguration: modelDerivativeConfiguration,
                localParameterSuffix: index.ToString(),
                randomSeed: index + randomSeed)).Cast<IConfiguredProblem>().ToArray();

        return ConfiguredProblem.Sum(problems);
    }

    private static readonly Func<double, IReadOnlyList<double>, double> Model = 
        (x, p) =>
        {
            var (amp, ka, c, kd, td) = (p[0], p[1], p[2], p[3], p[4]);
            var kaObs = ka * c + kd;

            return td > x
                ? amp * (1 - Math.Exp(-kaObs * x))
                : amp * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td));
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient = 
        (x, p) =>
        {
            var (amp, ka, c, kd, td) = (p[0], p[1], p[2], p[3], p[4]);
            var kaObs = ka * c + kd;

            var g0 = td > x ? 1 - Math.Exp(-kaObs * x) : (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td));
            var g1 = td > x
                ? amp * c * x * Math.Exp(-kaObs * x)
                : amp * c * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var g2 = td > x
                ? amp * ka * x * Math.Exp(-kaObs * x)
                : amp * ka * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var g3 = td > x
                ? amp * x * Math.Exp(-kaObs * x)
                : amp * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * (1 - Math.Exp(-kaObs * td)) * (td - x) * Math.Exp(-kd * (x - td));
            var g4 = td > x
                ? 0
                : amp * kd * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td)) -
                  amp * -kaObs * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);

            return [g0, g1, g2, g3, g4];
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian = 
        (x, p) =>
        {
            var (amp, ka, c, kd, td) = (p[0], p[1], p[2], p[3], p[4]);
            var kaObs = ka * c + kd;

            const double h00 = 0;
            var h01 = td > x
                ? c * x * Math.Exp(-kaObs * x)
                : c * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h02 = td > x
                ? ka * x * Math.Exp(-kaObs * x)
                : ka * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h03 = td > x
                ? x * Math.Exp(-kaObs * x)
                : td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  (1 - Math.Exp(-kaObs * td)) * (td - x) * Math.Exp(-kd * (x - td));
            var h04 = td > x
                ? 0
                : kd * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td)) -
                  -kaObs * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h11 = td > x
                ? -amp * Math.Pow(c, 2) * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * Math.Pow(c, 2) * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h12 = td > x
                ? -amp * c * ka * Math.Pow(x, 2) * Math.Exp(-kaObs * x) + amp * x * Math.Exp(-kaObs * x)
                : -amp * c * ka * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h13 = td > x
                ? -amp * c * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * c * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * c * td * (td - x) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h14 = td > x
                ? 0
                : amp * c * kd * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * c * -kaObs * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * c * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h22 = td > x
                ? -amp * Math.Pow(ka, 2) * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * Math.Pow(ka, 2) * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h23 = td > x
                ? -amp * ka * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * ka * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * ka * td * (td - x) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h24 = td > x
                ? 0
                : amp * ka * kd * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * ka * -kaObs * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * ka * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h33 = td > x
                ? -amp * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  2 * amp * td * (td - x) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) + amp *
                  (1 - Math.Exp(-kaObs * td)) * Math.Pow(td - x, 2) * Math.Exp(-kd * (x - td));
            var h34 = td > x
                ? 0
                : amp * kd * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * kd * (1 - Math.Exp(-kaObs * td)) * (td - x) * Math.Exp(-kd * (x - td)) +
                  amp * -kaObs * td * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td)) -
                  amp * (td - x) * -kaObs * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  amp * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h44 = td > x
                ? 0
                : amp * Math.Pow(kd, 2) * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td)) -
                  2 * amp * kd * -kaObs * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) -
                  amp * Math.Pow(-kaObs, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);

            return
            [
                h00, h01, h02, h03, h04,
                h01, h11, h12, h13, h14,
                h02, h12, h22, h23, h24,
                h03, h13, h23, h33, h34,
                h04, h14, h24, h34, h44
            ];
        };

    private static readonly Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal = 
        (x, p) =>
        {
            var (amp, ka, c, kd, td) = (p[0], p[1], p[2], p[3], p[4]);
            var kaObs = ka * c + kd;

            const double h00 = 0;
            var h11 = td > x
                ? -amp * Math.Pow(c, 2) * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * Math.Pow(c, 2) * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h22 = td > x
                ? -amp * Math.Pow(ka, 2) * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * Math.Pow(ka, 2) * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);
            var h33 = td > x
                ? -amp * Math.Pow(x, 2) * Math.Exp(-kaObs * x)
                : -amp * Math.Pow(td, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) +
                  2 * amp * td * (td - x) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) + amp *
                  (1 - Math.Exp(-kaObs * td)) * Math.Pow(td - x, 2) * Math.Exp(-kd * (x - td));
            var h44 = td > x
                ? 0
                : amp * Math.Pow(kd, 2) * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td)) -
                  2 * amp * kd * -kaObs * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td) -
                  amp * Math.Pow(-kaObs, 2) * Math.Exp(-kd * (x - td)) * Math.Exp(-kaObs * td);

            return [h00, h11, h22, h33, h44];
        };
    
    private static readonly IReadOnlyList<double> XValues = Values.LinearlySpacedBetween(0, 1000, 1);
    
    private static double[] YValuesFor(double analyteConcentration, int randomSeed)
    {
        var random = new Random(randomSeed);
        var optimumValues = OptimumValuesFor(analyteConcentration);     
        return XValues.Select(x => Model(x, optimumValues) + random.NextNormal(0, YError)).ToArray();
    }
    
    // The standard deviation of the noise overlying the data is chosen small enough such that the optimum parameter
    // values are approximately equal to the values used to generate the data
    private const double YError = 0.001;  
    
    private static IReadOnlyList<double> OptimumValuesFor(double analyteConcentrationInNanoMolar)
    {
        const double ka = 1e-3;  // (s * nM)-1
        const double kd = 1e-2;  // s-1
        var kaObs = ka * analyteConcentrationInNanoMolar + kd;
        var amp = ka * analyteConcentrationInNanoMolar / kaObs;
        var td = Math.Max(5 / kaObs, 100);

        return [amp, ka, analyteConcentrationInNanoMolar, kd, td];
    }
    
    private static IReadOnlyList<ParameterConfiguration> ConfigurationsFor(
        ParameterConfiguration? amplitude,
        ParameterConfiguration? associationRate,
        ParameterConfiguration? analyteConcentration,
        ParameterConfiguration? dissociationRate,
        ParameterConfiguration? dissociationStart,
        double actualAnalyteConcentration,
        string? localParameterSuffix,
        int randomSeed)
    {
        var random = new Random(randomSeed);
        var optimumValues = OptimumValuesFor(actualAnalyteConcentration);
        return
        [
            Local(amplitude ?? Variable("amp", optimumValues[0] * random.NextNormal(1, 0.1))),
            associationRate ?? Variable("ka", optimumValues[1] * random.NextNormal(1, 0.1), lowerLimit: 0),
            Local(analyteConcentration ?? Fixed("c", optimumValues[2])),
            dissociationRate ?? Variable("kd", optimumValues[3] * random.NextNormal(1, 0.1), lowerLimit: 0),
            Local(dissociationStart ?? Variable("td", optimumValues[4] + random.NextNormal(0, 3)))
        ];

        ParameterConfiguration Local(ParameterConfiguration config) =>
            localParameterSuffix is null ? config : Suffixed(config, localParameterSuffix);
    }

    private static ParameterConfiguration Suffixed(ParameterConfiguration config, string suffix)
    {
        var suffixedName = $"{config.Name}_{suffix}";
        return config.IsFixed
            ? Fixed(suffixedName, config.Value)
            : Variable(suffixedName, config.Value, config.LowerLimit, config.UpperLimit);
    }
}