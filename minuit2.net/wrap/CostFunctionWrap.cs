namespace minuit2.net.wrap;

internal class CostFunctionWrap(ICostFunction function) : FCNWrap
{
    public override double Cost(VectorDouble parameters) => function.ValueFor(parameters);

    public override VectorDouble Gradient(VectorDouble parameters) => new();

    public override bool HasGradient() => false;

    public override double Up() => function.Up;
}
