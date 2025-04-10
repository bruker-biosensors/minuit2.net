namespace minuit2.net.wrap;

internal class CostFunctionWrap(ICostFunction function) : FCNWrap
{
    public override double Cost(VectorDouble parameterValues) => function.ValueFor(parameterValues);

    public override VectorDouble Gradient(VectorDouble parameterValues) => new(function.GradientFor(parameterValues));

    public override bool HasGradient() => function.HasGradient;

    public override double Up() => function.Up;
}
