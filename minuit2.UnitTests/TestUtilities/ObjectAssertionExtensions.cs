using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Primitives;

namespace minuit2.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class ObjectAssertionExtensions
{
    public static AndConstraint<ObjectAssertions> ShouldFulfill<T>(this T subject, Action<T> assertion) =>
        subject.Should().Satisfy(assertion);
}