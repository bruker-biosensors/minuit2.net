using System.Numerics;
using AutoFixture;

namespace minuit2.UnitTests.TestUtilities;

internal static class Any
{
    private static readonly Fixture Fixture = new();
    
    public static AnyNumber<double> Double() => new(Fixture);
    
    public static AnyNumber<int> Integer() => new(Fixture);
    
    public static string String() => Fixture.Create<string>();
    
    public static T? OrNull<T>(this T any) where T : struct
    {
        var randomBool = new Random().NextDouble() > 0.5;
        return randomBool ? any : null;
    }
}

internal class AnyNumber<T>(Fixture fixture) where T : INumber<T>, IMinMaxValue<T>
{
    private readonly T[] _ascendingNumbers = fixture.CreateMany<T>(3).Order().ToArray();
    
    private T Number => _ascendingNumbers[0];
    
    // The inverse is used to prevent unwanted roundoff, e.g. for integers
    private T InverseUnitIntervalNumber => _ascendingNumbers[1] > _ascendingNumbers[0]
        ? (_ascendingNumbers[2] - _ascendingNumbers[0]) / (_ascendingNumbers[1] - _ascendingNumbers[0])
        : _ascendingNumbers[2] - _ascendingNumbers[0];
    
    public static implicit operator T(AnyNumber<T> number) => number.Number;
    
    // The following MinValue and MaxValue checks ensure that overflow errors are prevented
    public T GreaterThan(T min) => T.Abs(Number) < T.MaxValue - min 
        ? min + T.Abs(Number) 
        : Between(min, T.MaxValue);

    public T SmallerThan(T max) => T.Abs(Number) < max - T.MinValue 
        ? max - T.Abs(Number) 
        : Between(T.MinValue, max);
    
    public T Between(T min, T max) => min + (max - min) / InverseUnitIntervalNumber;
}
