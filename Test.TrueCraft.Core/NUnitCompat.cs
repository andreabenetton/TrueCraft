// Compatibility shims for NUnit-style helpers used during the xUnit migration.
// These let us preserve existing call-sites without rewriting every assertion.
using System;
using System.Collections;
using System.Linq;
using Xunit;

namespace Test.TrueCraft.Core
{
    internal static class CollectionAssert
    {
        public static void AreEqual(IEnumerable expected, IEnumerable actual)
        {
            var ex = expected?.Cast<object>().ToList();
            var ac = actual?.Cast<object>().ToList();
            Assert.Equal(ex, ac);
        }

        public static void AreEquivalent(IEnumerable expected, IEnumerable actual)
        {
            var ex = expected.Cast<object>().ToList();
            var ac = actual.Cast<object>().ToList();
            Assert.Equal(ex.Count, ac.Count);
            foreach (var item in ex) Assert.Contains(item, ac);
        }
    }

    internal static class XAssert
    {
        public static void DoesNotThrow(Action action)
        {
            var ex = Record.Exception(action);
            Assert.Null(ex);
        }
    }
}
