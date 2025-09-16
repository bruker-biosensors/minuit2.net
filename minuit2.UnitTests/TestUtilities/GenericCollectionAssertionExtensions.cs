using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions;
using AwesomeAssertions.Collections;

namespace minuit2.UnitTests.TestUtilities;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global", Justification = "Adhere to convention")]
internal static class GenericCollectionAssertionExtensions
{
    public static AndConstraint<GenericCollectionAssertions<T>> Fulfill<T>(
        this GenericCollectionAssertions<T> parent,
        Action<IEnumerable<T>> assertion)
    {
        return parent.Satisfy(assertion);
    }
}