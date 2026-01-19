using minuit2.net;
using minuit2.net.CostFunctions;
using static ExampleProblems.DerivativeConfiguration;
using static minuit2.net.ParameterConfiguration;

namespace ExampleProblems.CustomProblems;

public class SurfaceBiosensorBindingKineticsProblem : IConfiguredProblem
{
   // Simplified assumptions:
   // - ligand is immobilized on surface with surface density remaining constant
   // - analyte association kinetics (starting at t = 0) is directly followed by analyte dissociation kinetics
   // - analyte concentration is 0 during the dissociation measurement (no rebinding)
   // - analyte concentration remains constant during the association measurement
   // - analyte mobility is no restricting factor for the association kinetics (no mass transport limitations)
   
   private readonly DerivativeConfiguration _modelDerivativeConfiguration;
   private readonly IReadOnlyList<double> _x;
   private readonly IReadOnlyList<double> _y;
   private readonly IReadOnlyList<string> _parameters;

   public SurfaceBiosensorBindingKineticsProblem(
      DerivativeConfiguration modelDerivativeConfiguration = WithoutDerivatives,
      double analyteConcentrationValueInNanoMolar = 10,
      string? localParameterSuffix = null,
      int randomSeed = 0)
   {
      _modelDerivativeConfiguration = modelDerivativeConfiguration;
      var random = new Random(randomSeed);

      var amplitude = Local("amp");
      const string associationRate = "ka";
      var analyteConcentration = Local("c");
      const string dissociationRate = "kd";
      var dissociationStart = Local("td");
      
      _parameters = [amplitude, associationRate, analyteConcentration, dissociationRate, dissociationStart];
      var parameterValues = ParameterValuesFor(analyteConcentrationValueInNanoMolar);

      _x = Values.LinearlySpacedBetween(0, 1000, 1);
      _y = _x.Select(x => Model(x, parameterValues) + random.NextNormal(0, 0.001)).ToArray();

      OptimumParameterValues = parameterValues;
      ParameterConfigurations =
      [
         Variable(amplitude, parameterValues[0] * random.NextNormal(1, 0.1)),
         Variable(associationRate, parameterValues[1] * random.NextNormal(1, 0.1), lowerLimit: 0),
         Fixed(analyteConcentration, parameterValues[2]),
         Variable(dissociationRate, parameterValues[3] * random.NextNormal(1, 0.1), lowerLimit: 0),
         Variable(dissociationStart, parameterValues[4] + random.NextNormal(0, 3))
      ];
      return;

      string Local(string parameter) => localParameterSuffix == null ? parameter : parameter + localParameterSuffix;
   }
   
   public static IConfiguredProblem Global(
      IEnumerable<double> analyteConcentrationValuesInNanoMolar,
      DerivativeConfiguration modelDerivativeConfiguration = WithoutDerivatives,
      int randomSeed = 0)
   {
      var problems = analyteConcentrationValuesInNanoMolar.Select((c, i) =>
         new SurfaceBiosensorBindingKineticsProblem(modelDerivativeConfiguration, c, i.ToString(), i + randomSeed));
      
      var costs = new List<ICostFunction>();
      var parameterConfigs = new List<ParameterConfiguration>();
      var optimumValues = new List<double>();
      foreach (var problem in problems)
      {
         costs.Add(problem.Cost);
         foreach (var (config, value) in problem.ParameterConfigurations.Zip(problem.OptimumParameterValues))
         {
            if (parameterConfigs.Any(c => c.Name == config.Name)) continue;
            parameterConfigs.Add(config);
            optimumValues.Add(value);
         }
      }

      return new ConfiguredProblem(CostFunction.Sum(costs.ToArray()), optimumValues, parameterConfigs);
   }

   private static IReadOnlyList<double> ParameterValuesFor(double analyteConcentrationInNanoMolar)
   {
      const double ka = 1e-3; // (s * nM)-1
      const double kd = 1e-2; // s-1
      var kaObs = ka * analyteConcentrationInNanoMolar + kd;
      var amp = ka * analyteConcentrationInNanoMolar / kaObs;
      var td = Math.Max(5 / kaObs, 100);
      return [amp, ka, analyteConcentrationInNanoMolar, kd, td];
   }

   private static Func<double, IReadOnlyList<double>, double> Model { get; } = (x, p) =>
   {
      var (amp, ka, c, kd, td) = (p[0], p[1], p[2], p[3], p[4]);
      var kaObs = ka * c + kd;

      return td > x
         ? amp * (1 - Math.Exp(-kaObs * x))
         : amp * (1 - Math.Exp(-kaObs * td)) * Math.Exp(-kd * (x - td));
   };

   private static Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelGradient { get; } = (x, p) =>
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

   private static Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessian { get; } = (x, p) =>
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
         h04, h14, h24, h34, h44,
      ];
   };

   private static Func<double, IReadOnlyList<double>, IReadOnlyList<double>> ModelHessianDiagonal { get; } = (x, p) =>
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

   public ICostFunction Cost => _modelDerivativeConfiguration switch
   {
      WithoutDerivatives => 
         CostFunction.LeastSquares(_x, _y, _parameters, Model),
      WithGradient => 
         CostFunction.LeastSquares(_x, _y, _parameters, Model, ModelGradient),
      WithGradientAndHessian => 
         CostFunction.LeastSquares(_x, _y, _parameters, Model, ModelGradient, ModelHessian),
      WithGradientHessianAndHessianDiagonal => 
         CostFunction.LeastSquares(_x, _y, _parameters, Model, ModelGradient, ModelHessian, ModelHessianDiagonal),
      _ => throw new ArgumentOutOfRangeException(nameof(_modelDerivativeConfiguration), _modelDerivativeConfiguration, null)
   };

   public IReadOnlyCollection<double> OptimumParameterValues { get; }

   public IReadOnlyCollection<ParameterConfiguration> ParameterConfigurations { get; }
}