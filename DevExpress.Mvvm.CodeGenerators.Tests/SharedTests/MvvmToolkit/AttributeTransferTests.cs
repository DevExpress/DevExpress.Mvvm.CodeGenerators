using DevExpress.Mvvm.CodeGenerators.Tests.Included;
using NUnit.Framework;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm.CodeGenerators.MvvmToolkit;

namespace MvvmToolkit.Mvvm.Tests {
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
        public void AttributeTransferTest() {
            var noAttributeProperty = typeof(AttributeTransfer).GetProperty(nameof(AttributeTransfer.NoAttribute));
            var attributes = Attribute.GetCustomAttributes(noAttributeProperty);
            var expectedAttributes = new Attribute[] { };
            Assert.AreEqual(expectedAttributes, attributes);

            var withMultipleAttributesProperty = typeof(AttributeTransfer).GetProperty(nameof(AttributeTransfer.WithMultipleAttributes));
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
