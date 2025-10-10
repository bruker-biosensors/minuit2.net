using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace minuit2.UnitTests.TestUtilities;

internal static class Any
{
    // This setup was inspired by https://stackoverflow.com/a/18346427/6667272
    private static readonly IFixture Fixture = new Fixture()
        .Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

    public static T InstanceOf<T>() => Fixture.Create<T>();
    
    public static AnyNumber<double> Double() => new(Fixture);
    
    public static AnyNumber<int> Integer() => new(Fixture);
    
    public static string String() => Fixture.Create<string>();
    
    public static T? OrNull<T>(this T any) where T : struct
    {
        var randomBool = new Random().NextDouble() > 0.5;
        return randomBool ? any : null;
    }
}