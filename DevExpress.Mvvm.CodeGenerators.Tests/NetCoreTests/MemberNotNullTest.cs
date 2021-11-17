using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
#nullable enable
    [Prism.GenerateViewModel]
    partial class WithMemberNotNullAttributeDx {
        [Prism.GenerateProperty]
        string withAttribute;

        public WithMemberNotNullAttributeDx() {
            WithAttribute = string.Empty;
        }
    }
    [GenerateViewModel]
    partial class WithMemberNotNullAttributePrism {
        [GenerateProperty]
        string withAttribute;

        public WithMemberNotNullAttributePrism() {
            WithAttribute = string.Empty;
        }
    }
#nullable restore

    [TestFixture]
    public class MemberNotNullTest {
        [Test]
        public void CheckAttributeDx() {
            var withAttributePropertySetter = typeof(WithMemberNotNullAttributeDx).GetProperty("WithAttribute").SetMethod;
            var attributes = Attribute.GetCustomAttributes(withAttributePropertySetter);
            var expectedAttributes = new Attribute[] { new MemberNotNullAttribute("withAttribute") };
            Assert.AreEqual(expectedAttributes, attributes);
        }
        [Test]
        public void CheckAttributePrism() {
            var withAttributePropertySetter = typeof(WithMemberNotNullAttributePrism).GetProperty("WithAttribute").SetMethod;
            var attributes = Attribute.GetCustomAttributes(withAttributePropertySetter);
            var expectedAttributes = new Attribute[] { new MemberNotNullAttribute("withAttribute") };
            Assert.AreEqual(expectedAttributes, attributes);
        }
    }
}
