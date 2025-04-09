namespace minuit2.net.wrap;

internal class CostFunctionWrap(ICostFunction function) : FCNWrap
{
    public override double Cost(VectorDouble parameters) => function.ValueFor(parameters);

    public override VectorDouble Gradient(VectorDouble parameters) => new(function.GradientFor(parameters));

    public override bool HasGradient() => function.HasGradient;

    public override double Up() => function.Up;
}
