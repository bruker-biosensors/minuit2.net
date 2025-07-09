using System.Numerics;
using AutoFixture;

namespace minuit2.UnitTests.TestUtilities;

internal static class Any
{
    private static readonly Fixture Fixture = new();
    
    public static AnyNumber<double> Double() => new(Fixture);
    
    public static string String() => Fixture.Create<string>();
    
    public static T? OrNull<T>(this T any) where T : struct
    {
        var randomBool = new Random().NextDouble() >= 0.5;
        return randomBool ? any : null;
    }
}

internal class AnyNumber<T>(Fixture fixture) where T : struct, INumber<T>
{
    private readonly T[] _orderedNumbers = fixture.CreateMany<T>(3).Order().ToArray();

    public static implicit operator T(AnyNumber<T> number) => number._orderedNumbers.First();

    public T GreaterThan(T min) => min + T.Abs(_orderedNumbers.First());

    public T SmallerThan(T max) => max - T.Abs(_orderedNumbers.First());

    public T Between(T min, T max)
    {
        var unitIntervalNumber = (_orderedNumbers[1] - _orderedNumbers[0]) / (_orderedNumbers[2] - _orderedNumbers[0]);
        return min + unitIntervalNumber * (max - min);
    }
}
