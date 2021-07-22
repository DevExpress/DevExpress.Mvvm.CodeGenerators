using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.CodeGenerators.Tests
{
    [TestFixture]
    public class LineEnumeratorTests {
        [Test]
        public void LineEnumeratorTest() {
            AssertRanges(new (int, int)[0], string.Empty);
            AssertRanges(new[] { (0, 1) }, "a");
            AssertRanges(new[] { (0, 3) }, "abc");
            AssertRanges(new[] { (0, 3), (5, 2) }, "abc\r\nde");
            AssertRanges(new[] { (0, 3) }, "a\rc");
            AssertRanges(new[] { (0, 3) }, "a\nc");
        }
        static void AssertRanges((int start, int count)[] expected, string str) {
            CollectionAssert.AreEqual(expected, GetRanges(str).ToArray());
        }
        static IEnumerable<(int start, int count)> GetRanges(string str) {
            foreach(var range in new SourceBuilderExtensions.LineEnumerator(str)) {
                yield return range;
            }
        }
    }
}
