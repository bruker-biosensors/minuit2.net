using System.Numerics;
using AutoFixture;

namespace minuit2.UnitTests.TestUtilities;

internal static class Any
{
    private static readonly Fixture Fixture = new();
    
    public static AnyNumber<double> Double() => new(Fixture);
    
    public static AnyNumber<uint> UnsignedInteger() => new(Fixture);
    
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
    private T LowNumber => _ascendingNumbers[0];
    private T MidNumber => _ascendingNumbers[1];
    private T HighNumber => _ascendingNumbers[2];
    
    public static implicit operator T(AnyNumber<T> number) => number.MidNumber;
    
    // The following MinValue and MaxValue checks ensure that overflow errors are prevented
    public T GreaterThan(T min) => T.Abs(LowNumber) < T.MaxValue - min 
        ? min + T.Abs(LowNumber) 
        : Between(min, T.MaxValue);

    public T SmallerThan(T max) => T.Abs(LowNumber) < max - T.MinValue 
        ? max - T.Abs(LowNumber) 
        : Between(T.MinValue, max);
    
    // Keep the following evaluation order of terms as is;
    // Evaluating `(Mid - Low) / (High - Low)` first would, for instance, lead to unwanted roundoff for integers
    public T Between(T min, T max) => min + (max - min) * (MidNumber - LowNumber) / (HighNumber - LowNumber);

    public T OtherThan(T value) => _ascendingNumbers.FirstOrDefault(x => x != value, GreaterThan(value));
}
