using DevExpress.Mvvm.CodeGenerators.Tests.Included;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.CodeGenerators.Prism;

namespace Prism.Mvvm.Tests {
    class MyCustomAttribute : Attribute {
        public MyCustomAttribute(int value, string str, bool condition, TestEnum testEnum) { }
    }
    namespace Included {
        enum TestEnum {
            Num = 1,
            String = 2
        }
    }
    [GenerateViewModel]
    partial class AttributeTransfer {
        const int number = 1;

        [GenerateProperty]
        int noAttribute;

        [GenerateProperty, System.ComponentModel.DataAnnotations.Range(0, 1)]
        [Required, MyCustom(number,
                            "Some string",
                            true, TestEnum.Num)]
        int withMultipleAttributes;
    }

    [TestFixture]
    public class AttributeTransferTests {
        [Test]
        public void AttributeTransfer() {
            var noAttributeProperty = typeof(AttributeTransfer).GetProperty("NoAttribute");
            var attributes = Attribute.GetCustomAttributes(noAttributeProperty);
            var expectedAttributes = new Attribute[] { };
            Assert.AreEqual(expectedAttributes, attributes);

            var withMultipleAttributesProperty = typeof(AttributeTransfer).GetProperty("WithMultipleAttributes");
            attributes = Attribute.GetCustomAttributes(withMultipleAttributesProperty);
            expectedAttributes = new Attribute[] {
                new System.ComponentModel.DataAnnotations.RangeAttribute(0, 1),
                new RequiredAttribute(),
                new MyCustomAttribute(1, "Some string", true, TestEnum.Num)
            };
            Assert.AreEqual(expectedAttributes, attributes);
        }
    }
}
