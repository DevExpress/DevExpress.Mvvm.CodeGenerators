﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.CodeGenerators.Tests
{
    [TestFixture]
    public class SourceBuilderTests {
        StringBuilder sb;
        SourceBuilder builder;

        [SetUp]
        public void SetUp() {
            sb = new StringBuilder();
            builder = new SourceBuilder(sb);
        }

        [Test]
        public void SourceBuilderTest_Append() {
            Assert.AreSame(builder, builder.Append("123").Append("456"));
            AssertBuilderResult("123456");
        }

        [Test]
        public void SourceBuilderTest_AppendTab() {
            Assert.AreSame(builder.Tab, builder.Tab);
            Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab);
            Assert.AreSame(builder.Tab, builder.Tab.Append("123").Append("456"));
            AssertBuilderResult("    123456");

            builder.Tab.AppendLine();
            AssertBuilderResult("    123456\r\n");

            Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab.Append("789"));
            AssertBuilderResult("    123456\r\n        789");
        }

        [Test]
        public void SourceBuilderTest_Append_Range() {
            Assert.AreSame(builder, builder.Append("_123_", 1, 3).Append("_456_", 1, 3));
            AssertBuilderResult("123456");
        }

        [Test]
        public void SourceBuilderTest_AppendTab_Range() {
            Assert.AreSame(builder.Tab, builder.Tab);
            Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab);
            Assert.AreSame(builder.Tab, builder.Tab.Append("_123_", 1, 3).Append("_456_", 1, 3));
            AssertBuilderResult("    123456");
        }

        [Test]
        public void SourceBuilderTest_Append_Char() {
            Assert.AreSame(builder, builder.Append('1').Append('2'));
            AssertBuilderResult("12");
        }

        [Test]
        public void SourceBuilderTest_AppendTab_Char() {
            Assert.AreSame(builder.Tab, builder.Tab);
            Assert.AreSame(builder.Tab.Tab, builder.Tab.Tab);
            Assert.AreSame(builder.Tab, builder.Tab.Append('1').Append('2'));
            AssertBuilderResult("    12");
        }

        [Test]
        public void SourceBuilderTest_AppendLine() {
            Assert.AreSame(builder, builder.Append("123").AppendLine());
            AssertBuilderResult("123\r\n");

            Assert.AreSame(builder, builder.AppendLine());
            AssertBuilderResult("123\r\n\r\n");
        }

        [Test]
        public void SourceBuilderTest_AppendLine_Tab() {
            Assert.AreSame(builder.Tab, builder.Tab.Append("123").AppendLine());
            AssertBuilderResult("    123\r\n");

            Assert.AreSame(builder.Tab, builder.Tab.AppendLine());
            AssertBuilderResult("    123\r\n\r\n");

            Assert.AreSame(builder.Tab, builder.Tab.Append("456").AppendLine());
            AssertBuilderResult("    123\r\n\r\n    456\r\n");
        }

        [Test]
        public void SourceBuilderTest_Return() {
            Assert.Null(builder.Return);
            Assert.AreSame(builder, builder.Tab.Return);
            Assert.AreSame(builder.Tab, builder.Tab.Tab.Return);
        }

        [Test]
        public void SourceBuilderTest_TabOnlyInNewLineState() {
            builder.Append("123");
            var tabbed = builder.Tab;
            var tabbed2 = builder.Tab.Tab;
            Assert.Throws<InvalidOperationException>(() => tabbed.Append("456"));

            builder.AppendLine();
            tabbed.Append("456");
            AssertBuilderResult("123\r\n    456");

            Assert.Throws<InvalidOperationException>(() => tabbed2.Append("456"));
        }

        void AssertBuilderResult(string expectedResult) {
            Assert.AreEqual(expectedResult, sb.ToString());
        }

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