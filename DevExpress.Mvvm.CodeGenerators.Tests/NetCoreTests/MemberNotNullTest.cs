using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel]
    partial class WithMemberNotNullAttribute {
#nullable enable
        [GenerateProperty]
        string withAttribute;

        public WithMemberNotNullAttribute() {
            WithAttribute = string.Empty;
        }
#nullable restore
    }

    [TestFixture]
    public class MemberNotNullTest {
        [Test]
        public void CheckAttribute() {
            var withAttributePropertySetter = typeof(WithMemberNotNullAttribute).GetProperty("WithAttribute").SetMethod;
            var attributes = Attribute.GetCustomAttributes(withAttributePropertySetter);
            var expectedAttributes = new Attribute[] { new MemberNotNullAttribute("withAttribute") };
            Assert.AreEqual(expectedAttributes, attributes);
        }
    }
}
