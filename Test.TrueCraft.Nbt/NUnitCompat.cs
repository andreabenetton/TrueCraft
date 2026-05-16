// Compatibility shims for NUnit-style helpers used during the xUnit migration.
// These let us preserve existing call-sites without rewriting every assertion.
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Xunit;

namespace Test.TrueCraft.Nbt;

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

internal static class FileAssert
{
    public static void AreEqual(string expectedPath, string actualPath)
    {
        using var ex = File.OpenRead(expectedPath);
        using var ac = File.OpenRead(actualPath);
        AreEqual(ex, ac);
    }

    public static void AreEqual(Stream expected, Stream actual)
    {
        if (expected.CanSeek) expected.Position = 0;
        if (actual.CanSeek) actual.Position = 0;
        const int BufSize = 4096;
        var bufA = new byte[BufSize];
        var bufB = new byte[BufSize];
        while (true)
        {
            int readA = expected.Read(bufA, 0, BufSize);
            int readB = actual.Read(bufB, 0, BufSize);
            Assert.Equal(readA, readB);
            if (readA == 0) break;
            for (int i = 0; i < readA; i++)
                if (bufA[i] != bufB[i])
                    Assert.Fail($"Files differ at byte {i}: expected 0x{bufA[i]:X2}, actual 0x{bufB[i]:X2}");
        }
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
