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