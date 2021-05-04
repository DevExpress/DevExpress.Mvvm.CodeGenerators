using NUnit.Framework;
using System;

namespace DevExpress.Mvvm.CodeGenerators.Tests {
    [GenerateViewModel]
    partial class WithoutMemberNotNullAttribute {
#nullable enable
        [GenerateProperty]
        string noAttribute;

        public WithoutMemberNotNullAttribute() {
            noAttribute = "";
        }
#nullable restore
    }

    [TestFixture]
    public class MemberNotNullTest {
        [Test]
        public void CheckAttribute() {
            var noAttributePropertySetter = typeof(WithoutMemberNotNullAttribute).GetProperty("NoAttribute").SetMethod;
            var attributes = Attribute.GetCustomAttributes(noAttributePropertySetter);
            var expectedAttributes = new Attribute[] { };
            Assert.AreEqual(expectedAttributes, attributes);
        }
    }
}
