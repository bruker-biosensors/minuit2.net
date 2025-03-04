namespace minuit2.net;

internal class CostFunctionWrapper(ICostFunction function) : FCNWrap
{
    public override double Cost(VectorDouble parameters) => function.ValueFor(parameters);

    public override VectorDouble Gradient(VectorDouble parameters) => new();

    public override bool HasGradient() => false;
}
